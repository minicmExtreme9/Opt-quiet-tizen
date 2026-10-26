using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public static class AtomicFileWriter
{
    [DllImport("libc.so.6", EntryPoint = "fsync", SetLastError = true)]
    static extern int libc_fsync(int fd);

    [DllImport("libc.so.6", EntryPoint = "sync")]
    static extern void libc_sync();

    /// <summary>
    /// Writes content to a file atomically with explicit fsync to survive hard power loss.
    /// 1. Writes to path.tmp in the same directory using FileOptions.WriteThrough.
    /// 2. Explicitly flushes stream buffers to physical disk with Flush(true) and libc fsync().
    /// 3. Atomically replaces destination path using File.Move(..., overwrite: true).
    /// 4. Calls libc sync() to ensure directory entry / inode metadata is committed to flash.
    /// </summary>
    public static bool WriteAllTextSafe(string path, string content)
    {
        if (string.IsNullOrEmpty(path)) return false;
        string tmpPath = path + ".tmp";
        try
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);

            // 1. Write to tmp file with WriteThrough
            using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush(flushToDisk: true);

                // Secondary direct libc fsync on raw file descriptor
                try
                {
                    int fd = fs.SafeFileHandle.DangerousGetHandle().ToInt32();
                    if (fd > 0)
                    {
                        libc_fsync(fd);
                    }
                }
                catch (Exception ex)
                {
                    DebugLog.Warn("Storage", "libc_fsync call skipped or failed", ex);
                }
            }

            // 2. Atomic rename in same filesystem
            File.Move(tmpPath, path, overwrite: true);

            // 3. Flush filesystem directory metadata to survive immediate power cut
            try
            {
                libc_sync();
            }
            catch (Exception ex)
            {
                DebugLog.Warn("Storage", "libc_sync call skipped or failed", ex);
            }

            return true;
        }
        catch (Exception ex)
        {
            DebugLog.Error("Storage", "WriteAllTextSafe failed for " + path, ex);
            try
            {
                if (File.Exists(tmpPath))
                {
                    File.Delete(tmpPath);
                }
            }
            catch {}
            return false;
        }
    }

    /// <summary>
    /// Checks for and cleans up any orphan .tmp file left behind by an interrupted write.
    /// </summary>
    public static void CleanOrphanTmp(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            string tmpPath = path + ".tmp";
            if (File.Exists(tmpPath))
            {
                DebugLog.Warn("Storage", $"Cleaning orphan temp file: {tmpPath}");
                File.Delete(tmpPath);
            }
        }
        catch (Exception ex)
        {
            DebugLog.Warn("Storage", $"Failed cleaning orphan temp file for {path}", ex);
        }
    }
}

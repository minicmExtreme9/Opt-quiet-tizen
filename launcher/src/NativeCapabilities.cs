using System;
using System.Runtime.InteropServices;

/// <summary>
/// Encapsulates native Tizen platform P/Invoke calls and runtime capability detection.
/// Safely probes platform libraries at startup so the launcher can gracefully
/// adapt UI controls and fallbacks without crashing on missing libraries.
/// </summary>
public static class NativeCapabilities
{
    internal static class NativeMethods
    {
        [DllImport("libsource-api.so")]
        public static extern int connect_source(int sourceType);

        [DllImport("libvconf.so.0")]
        public static extern int vconf_get_int(string key, out int value);

        [DllImport("libcapi-appfw-app-control.so.0")]
        public static extern int app_control_create(out IntPtr app);

        [DllImport("libcapi-appfw-app-control.so.0")]
        public static extern int app_control_set_app_id(IntPtr app, string id);

        [DllImport("libcapi-appfw-app-control.so.0")]
        public static extern int app_control_set_operation(IntPtr app, string operation);

        [DllImport("libcapi-appfw-app-control.so.0")]
        public static extern int app_control_add_extra_data(IntPtr app, string key, string value);

        [DllImport("libcapi-appfw-app-control.so.0")]
        public static extern int app_control_send_launch_request(IntPtr app, IntPtr cb, IntPtr data);

        [DllImport("libcapi-appfw-app-control.so.0")]
        public static extern int app_control_destroy(IntPtr app);

        [DllImport("libc.so.6")]
        public static extern int system(string command);

        [DllImport("libcapi-media-sound-manager-tv.so.0")]
        public static extern int sound_manager_get_master_volume(out int value);

        [DllImport("libcapi-media-sound-manager-tv.so.0")]
        public static extern int sound_manager_set_master_volume(int value);

        [DllImport("libcapi-media-sound-manager-tv.so.0")]
        public static extern int sound_manager_get_master_mute(out int value);

        [DllImport("libcapi-media-sound-manager-tv.so.0")]
        public static extern int sound_manager_set_master_mute(int value);

        [DllImport("libmmfkeysound.so.0")]
        public static extern int mm_sound_play_keysound(string filename, int type);

        [DllImport("libsettings-api.so")]
        public static extern bool CSharpGetItemValue(string id, out string name, out string value);

        [DllImport("libsettings-api.so")]
        public static extern bool CSharpSetItemValue(string id, string value);

        [DllImport("libsettings-api.so")]
        public static extern bool CSharpGetItemEnable(string id, out bool enabled);

        [DllImport("libsettings-api.so")]
        public static extern bool CSharpGetItemMinMaxStepValue(string id, out double min, out double max, out double step);
    }

    // Subsystem availability cached once at startup
    public static bool HasSoundManager { get; private set; }
    public static bool HasSettingsApi { get; private set; }
    public static bool HasAppControl { get; private set; }
    public static bool HasVconf { get; private set; }
    public static bool HasKeySound { get; private set; }
    public static bool HasSourceApi { get; private set; }
    public static bool HasSystemCommand { get; private set; }

    // Feature-level availability flags
    public static bool HasVolume => HasSoundManager;
    public static bool HasMute => HasSoundManager;
    public static bool HasBrightness => HasSettingsApi;
    public static bool HasGameMode => HasSettingsApi;
    public static bool HasSleepTimer => HasSettingsApi;
    public static bool HasHdmiSwitch => HasAppControl;
    public static bool HasControllers => HasAppControl;
    public static bool HasNetworkSettings => HasAppControl;
    public static bool HasAdvancedSettings => HasAppControl;

    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        // Probe SoundManager (volume and mute)
        try
        {
            int testVol;
            int res = NativeMethods.sound_manager_get_master_volume(out testVol);
            HasSoundManager = (res == 0);
            if (!HasSoundManager) DebugLog.Warn("Native", $"SoundManager probe returned {res}");
        }
        catch (Exception ex)
        {
            HasSoundManager = false;
            DebugLog.Warn("Native", "SoundManager probe failed", ex);
        }

        // Probe Settings API (brightness, game mode, sleep timer)
        try
        {
            bool enabled;
            NativeMethods.CSharpGetItemEnable("brightness", out enabled);
            HasSettingsApi = true;
        }
        catch (Exception ex)
        {
            HasSettingsApi = false;
            DebugLog.Warn("Native", "SettingsApi probe failed", ex);
        }

        // Probe AppControl (source switching, app launching)
        try
        {
            IntPtr handle;
            int res = NativeMethods.app_control_create(out handle);
            if (res == 0 && handle != IntPtr.Zero)
            {
                NativeMethods.app_control_destroy(handle);
                HasAppControl = true;
            }
            else
            {
                DebugLog.Warn("Native", $"AppControl create probe returned {res}");
            }
        }
        catch (Exception ex)
        {
            HasAppControl = false;
            DebugLog.Warn("Native", "AppControl probe failed", ex);
        }

        // Probe Vconf (system source type query)
        try
        {
            int val;
            int res = NativeMethods.vconf_get_int("memory/system/source_type", out val);
            HasVconf = (res == 0);
            if (!HasVconf) DebugLog.Warn("Native", $"Vconf probe returned {res}");
        }
        catch (Exception ex)
        {
            HasVconf = false;
            DebugLog.Warn("Native", "Vconf probe failed", ex);
        }

        // Probe KeySound
        try
        {
            NativeMethods.mm_sound_play_keysound("/usr/share/feedback/sound/operation/move.wav", 0);
            HasKeySound = true;
        }
        catch (Exception ex)
        {
            HasKeySound = false;
            DebugLog.Warn("Native", "KeySound probe failed", ex);
        }

        // Probe SourceApi
        try
        {
            NativeMethods.connect_source(-1);
            HasSourceApi = true;
        }
        catch (Exception ex)
        {
            HasSourceApi = false;
            DebugLog.Warn("Native", "SourceApi probe failed", ex);
        }

        // Probe System command
        try
        {
            HasSystemCommand = true;
        }
        catch (Exception ex)
        {
            HasSystemCommand = false;
            DebugLog.Warn("Native", "System command probe failed", ex);
        }

        DebugLog.Info("Native", $"Probed capabilities: Sound={HasSoundManager}, Settings={HasSettingsApi}, AppControl={HasAppControl}, Vconf={HasVconf}");
    }

    // Volume & Mute Wrappers
    public static bool GetMasterVolume(out int volume)
    {
        volume = -1;
        if (!HasSoundManager) return false;
        try
        {
            int v;
            if (NativeMethods.sound_manager_get_master_volume(out v) == 0)
            {
                volume = v;
                return true;
            }
        }
        catch (Exception ex)
        {
            DebugLog.Warn("Native", "GetMasterVolume failed", ex);
        }
        return false;
    }

    public static bool SetMasterVolume(int volume)
    {
        if (!HasSoundManager) return false;
        try
        {
            return NativeMethods.sound_manager_set_master_volume(volume) == 0;
        }
        catch (Exception ex)
        {
            DebugLog.Error("Native", "SetMasterVolume failed", ex);
            return false;
        }
    }

    public static bool GetMasterMute(out int mute)
    {
        mute = -1;
        if (!HasSoundManager) return false;
        try
        {
            int m;
            if (NativeMethods.sound_manager_get_master_mute(out m) == 0)
            {
                mute = m;
                return true;
            }
        }
        catch (Exception ex)
        {
            DebugLog.Warn("Native", "GetMasterMute failed", ex);
        }
        return false;
    }

    public static bool SetMasterMute(int mute)
    {
        if (!HasSoundManager) return false;
        try
        {
            return NativeMethods.sound_manager_set_master_mute(mute) == 0;
        }
        catch (Exception ex)
        {
            DebugLog.Error("Native", "SetMasterMute failed", ex);
            return false;
        }
    }

    // Settings API Wrappers
    public static bool GetItemValue(string id, out string name, out string value)
    {
        name = null;
        value = null;
        if (!HasSettingsApi) return false;
        try
        {
            return NativeMethods.CSharpGetItemValue(id, out name, out value);
        }
        catch (Exception ex)
        {
            DebugLog.Warn("Native", "GetItemValue failed for " + id, ex);
            return false;
        }
    }

    public static bool SetItemValue(string id, string value)
    {
        if (!HasSettingsApi) return false;
        try
        {
            return NativeMethods.CSharpSetItemValue(id, value);
        }
        catch (Exception ex)
        {
            DebugLog.Error("Native", "SetItemValue failed for " + id, ex);
            return false;
        }
    }

    public static bool GetItemEnable(string id, out bool enabled)
    {
        enabled = false;
        if (!HasSettingsApi) return false;
        try
        {
            return NativeMethods.CSharpGetItemEnable(id, out enabled);
        }
        catch (Exception ex)
        {
            DebugLog.Warn("Native", "GetItemEnable failed for " + id, ex);
            return false;
        }
    }

    public static bool GetItemMinMaxStepValue(string id, out double min, out double max, out double step)
    {
        min = 0; max = 50; step = 1;
        if (!HasSettingsApi) return false;
        try
        {
            return NativeMethods.CSharpGetItemMinMaxStepValue(id, out min, out max, out step);
        }
        catch (Exception ex)
        {
            DebugLog.Warn("Native", "GetItemMinMaxStepValue failed for " + id, ex);
            return false;
        }
    }

    // Vconf Wrapper
    public static bool GetVconfInt(string key, out int value)
    {
        value = 0;
        if (!HasVconf) return false;
        try
        {
            return NativeMethods.vconf_get_int(key, out value) == 0;
        }
        catch (Exception ex)
        {
            DebugLog.Warn("Native", "GetVconfInt failed for " + key, ex);
            return false;
        }
    }

    // KeySound Wrapper
    public static void PlayKeySound(string filename, int type = 0)
    {
        if (!HasKeySound) return;
        try
        {
            NativeMethods.mm_sound_play_keysound(filename, type);
        }
        catch (Exception ex)
        {
            DebugLog.Warn("Native", "PlayKeySound failed for " + filename, ex);
        }
    }

    // App Control & Source Switching Wrappers
    public static int SwitchHdmiSource(int source)
    {
        if (!HasAppControl)
        {
            DebugLog.Error("Native", "Cannot switch HDMI source: AppControl unavailable");
            return -1;
        }
        IntPtr c;
        int r = NativeMethods.app_control_create(out c);
        if (r != 0)
        {
            DebugLog.Error("Native", $"app_control_create returned {r}");
            return r;
        }
        try
        {
            NativeMethods.app_control_set_app_id(c, "org.tizen.tv-viewer");
            NativeMethods.app_control_set_operation(c, "http://tizen.org/appcontrol/operation/default");
            NativeMethods.app_control_add_extra_data(c, "CALLER", "org.tizen.source-list");
            NativeMethods.app_control_add_extra_data(c, "change_source", source.ToString());
            NativeMethods.app_control_add_extra_data(c, "mbrIndex", "-2");
            NativeMethods.app_control_add_extra_data(c, "sourceswitch_banner", "disable");
            NativeMethods.app_control_add_extra_data(c, "__K_EX_FORCE_BOOST", "enable");
            int sendRes = NativeMethods.app_control_send_launch_request(c, IntPtr.Zero, IntPtr.Zero);
            if (sendRes != 0) DebugLog.Error("Native", $"app_control_send_launch_request returned {sendRes} for source {source}");
            return sendRes;
        }
        catch (Exception ex)
        {
            DebugLog.Error("Native", "SwitchHdmiSource exception for " + source, ex);
            return -1;
        }
        finally
        {
            try { NativeMethods.app_control_destroy(c); } catch {}
        }
    }

    public static int LaunchAppControl(string id, string menuItem = null)
    {
        if (!HasAppControl)
        {
            DebugLog.Error("Native", "Cannot launch app: AppControl unavailable");
            return -1;
        }
        IntPtr c;
        int r = NativeMethods.app_control_create(out c);
        if (r != 0)
        {
            DebugLog.Error("Native", $"app_control_create returned {r} for {id}");
            return r;
        }
        try
        {
            NativeMethods.app_control_set_app_id(c, id);
            NativeMethods.app_control_set_operation(c, "http://tizen.org/appcontrol/operation/default");
            if (id == "org.tizen.menu" && menuItem != null)
            {
                NativeMethods.app_control_add_extra_data(c, "menuitem", menuItem);
                NativeMethods.app_control_add_extra_data(c, "launchoption", "pressed");
            }
            NativeMethods.app_control_add_extra_data(c, "__K_EX_FORCE_BOOST", "enable");
            NativeMethods.app_control_add_extra_data(c, "__K_SPLASH_SCREEN", "LA_UI_VR_TRANSPARENT");
            int sendRes = NativeMethods.app_control_send_launch_request(c, IntPtr.Zero, IntPtr.Zero);
            if (sendRes != 0) DebugLog.Warn("Native", $"Launch request returned {sendRes} for {id}");
            return sendRes;
        }
        catch (Exception ex)
        {
            DebugLog.Error("Native", "LaunchAppControl exception for " + id, ex);
            return -1;
        }
        finally
        {
            try { NativeMethods.app_control_destroy(c); } catch {}
        }
    }

    public static int ExecuteSystem(string command)
    {
        if (!HasSystemCommand) return -1;
        try
        {
            return NativeMethods.system(command);
        }
        catch (Exception ex)
        {
            DebugLog.Error("Native", "ExecuteSystem exception for " + command, ex);
            return -1;
        }
    }
}

import zipfile, os, shutil
from pathlib import Path

ROOT = Path("C:/Users/Jorge/quiet-tizen")
pkg_root = ROOT / ".local/tpk_quiethome"
if pkg_root.exists():
    shutil.rmtree(pkg_root)
pkg_root.mkdir(parents=True)

(pkg_root / "bin").mkdir()
(pkg_root / "res").mkdir()
(pkg_root / "shared" / "res").mkdir(parents=True)

# Copy bin files
shutil.copy(ROOT / ".local/package/CinemaGraphApp.dll", pkg_root / "bin/CinemaGraphApp.dll")
shutil.copy(ROOT / ".local/package/CinemaGraphApp.runtimeconfig.json", pkg_root / "bin/CinemaGraphApp.runtimeconfig.json")
shutil.copy(ROOT / ".local/package/Probe.dll", pkg_root / "bin/Probe.dll")
shutil.copy(ROOT / ".local/package/QuickLaunch.dll", pkg_root / "bin/QuickLaunch.dll")

# Copy res
shutil.copytree(ROOT / ".local/package/quiet-home", pkg_root / "res/quiet-home")

# Create standard non-reserved manifest
manifest = """<?xml version="1.0" encoding="utf-8"?>
<manifest package="org.quiethome.launcher" version="1.0.0" api-version="6.5" xmlns="http://tizen.org/ns/packages">
    <profile name="tv" />
    <ui-application appid="org.quiethome.launcher" exec="CinemaGraphApp.dll" multiple="false" nodisplay="false" taskmanage="true" type="dotnet" launch_mode="single">
        <label>Quiet Home</label>
        <icon>CinemaGraphApp.png</icon>
    </ui-application>
    <privileges>
        <privilege>http://tizen.org/privilege/network.get</privilege>
        <privilege>http://tizen.org/privilege/internet</privilege>
    </privileges>
    <feature name="http://tizen.org/feature/screen.size.normal.1080.1920">true</feature>
</manifest>
"""
(pkg_root / "tizen-manifest.xml").write_text(manifest, encoding="utf-8")

# Create default icon
from PIL import Image
icon = Image.new("RGBA", (117, 117), (20, 24, 38, 255))
icon.save(pkg_root / "shared/res/CinemaGraphApp.png")

# Pack TPK
out_tpk = Path("C:/Users/Jorge/Downloads/QuietHome-TV-1.0.0.tpk")
with zipfile.ZipFile(out_tpk, "w", zipfile.ZIP_DEFLATED) as z:
    for root, dirs, files in os.walk(pkg_root):
        for f in files:
            p = Path(root) / f
            arcname = p.relative_to(pkg_root)
            z.write(p, arcname)

print("Generated clean package:", out_tpk, "size:", out_tpk.stat().st_size)

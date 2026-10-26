#!/usr/bin/env python3
"""Packages the unprivileged lite variant into opt-quiet-tizen-lite.tpk without touching the full tpk."""
import os
import shutil
import zipfile
from pathlib import Path

ROOT = Path("C:/Users/Jorge/quiet-tizen")
source_dir = ROOT / ".local/package-lite"

if not source_dir.exists():
    raise SystemExit("Error: .local/package-lite does not exist. Run 'python tools/build.py launcher --variant lite --framework .local/tv-framework' first.")

pkg_root = ROOT / ".local/tpk_quiethome_lite"
if pkg_root.exists():
    shutil.rmtree(pkg_root)
pkg_root.mkdir(parents=True)

(pkg_root / "bin").mkdir()
(pkg_root / "res").mkdir()
(pkg_root / "shared" / "res").mkdir(parents=True)

# Copy bin files
shutil.copy(source_dir / "CinemaGraphApp.dll", pkg_root / "bin/CinemaGraphApp.dll")
shutil.copy(source_dir / "CinemaGraphApp.runtimeconfig.json", pkg_root / "bin/CinemaGraphApp.runtimeconfig.json")
shutil.copy(source_dir / "Probe.dll", pkg_root / "bin/Probe.dll")
shutil.copy(source_dir / "QuickLaunch.dll", pkg_root / "bin/QuickLaunch.dll")

# Copy res
shutil.copytree(source_dir / "quiet-home", pkg_root / "res/quiet-home")

# Create unprivileged manifest
manifest = """<?xml version="1.0" encoding="utf-8"?>
<manifest package="org.quiethome.lite" version="1.0.0" api-version="6.5" xmlns="http://tizen.org/ns/packages">
    <profile name="tv" />
    <ui-application appid="org.quiethome.lite" exec="CinemaGraphApp.dll" multiple="false" nodisplay="false" taskmanage="true" type="dotnet" launch_mode="single">
        <label>Quiet Home Lite</label>
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

# Create icon
try:
    from PIL import Image
    icon = Image.new("RGBA", (117, 117), (20, 35, 60, 255))
    icon.save(pkg_root / "shared/res/CinemaGraphApp.png")
except ImportError:
    pass

# Pack TPK
out_tpk = Path("C:/Users/Jorge/Downloads/opt-quiet-tizen-lite.tpk")
with zipfile.ZipFile(out_tpk, "w", zipfile.ZIP_DEFLATED) as z:
    for root, dirs, files in os.walk(pkg_root):
        for f in files:
            p = Path(root) / f
            arcname = p.relative_to(pkg_root)
            z.write(p, arcname)

print("Generated clean lite package:", out_tpk, "size:", out_tpk.stat().st_size)

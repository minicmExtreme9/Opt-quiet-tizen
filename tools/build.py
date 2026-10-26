#!/usr/bin/env python3
"""Build locally; generates unsigned TPK packages ready for device-specific signing."""
import argparse
import os
from pathlib import Path
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def create_manifest(variant: str) -> str:
    pkg_id = "org.quiethome.lite" if variant == "lite" else "org.quiethome.launcher"
    label = "Quiet Home Lite" if variant == "lite" else "Quiet Home"
    return f"""<?xml version="1.0" encoding="utf-8"?>
<manifest package="{pkg_id}" version="1.0.0" api-version="6.5" xmlns="http://tizen.org/ns/packages">
    <profile name="tv" />
    <ui-application appid="{pkg_id}" exec="CinemaGraphApp.dll" multiple="false" nodisplay="false" taskmanage="true" type="dotnet" launch_mode="single">
        <label>{label}</label>
        <icon>CinemaGraphApp.png</icon>
    </ui-application>
    <privileges>
        <privilege>http://tizen.org/privilege/network.get</privilege>
        <privilege>http://tizen.org/privilege/internet</privilege>
    </privileges>
    <feature name="http://tizen.org/feature/screen.size.normal.1080.1920">true</feature>
</manifest>
"""


def package_unsigned_tpk(source_dir: Path, out_tpk: Path, variant: str, container: str):
    stage_dir = ROOT / f".local/tpk_stage_{variant}"
    if stage_dir.exists():
        shutil.rmtree(stage_dir)
    stage_dir.mkdir(parents=True)

    bin_dir = stage_dir / "bin"
    res_dir = stage_dir / "res"
    shared_res_dir = stage_dir / "shared" / "res"
    bin_dir.mkdir()
    res_dir.mkdir()
    shared_res_dir.mkdir(parents=True)

    # Binaries
    shutil.copyfile(source_dir / f"{container}.dll", bin_dir / f"{container}.dll")
    runtime_config = source_dir / f"{container}.runtimeconfig.json"
    if runtime_config.is_file():
        shutil.copyfile(runtime_config, bin_dir / f"{container}.runtimeconfig.json")
    if (source_dir / "Probe.dll").is_file():
        shutil.copyfile(source_dir / "Probe.dll", bin_dir / "Probe.dll")
    if (source_dir / "QuickLaunch.dll").is_file():
        shutil.copyfile(source_dir / "QuickLaunch.dll", bin_dir / "QuickLaunch.dll")

    # Resources
    shutil.copytree(source_dir / "quiet-home", res_dir / "quiet-home")

    # Manifest
    (stage_dir / "tizen-manifest.xml").write_text(create_manifest(variant), encoding="utf-8")

    # Icon
    icon_src = ROOT / "launcher/assets/icon.png"
    if icon_src.is_file():
        shutil.copyfile(icon_src, shared_res_dir / "CinemaGraphApp.png")
    else:
        # Fallback 117x117 blank icon if Pillow is available or none
        try:
            from PIL import Image
            img = Image.new("RGBA", (117, 117), (20, 35, 60, 255))
            img.save(shared_res_dir / "CinemaGraphApp.png")
        except ImportError:
            pass

    # Create unsigned TPK zip archive
    out_tpk.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(out_tpk, "w", zipfile.ZIP_DEFLATED) as z:
        for root, dirs, files in os.walk(stage_dir):
            for f in files:
                p = Path(root) / f
                arcname = p.relative_to(stage_dir)
                z.write(p, arcname)

    return out_tpk


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("target", choices=["payloads", "launcher", "all"])
    default_dotnet = os.environ.get("DOTNET") or (str(Path.home() / ".dotnet/dotnet.exe") if (Path.home() / ".dotnet/dotnet.exe").is_file() else "dotnet")
    parser.add_argument("--dotnet", default=default_dotnet, help="Path or command for dotnet executable (default: from PATH or DOTNET env)")
    default_framework = os.environ.get("TIZEN_FRAMEWORK") or os.environ.get("TV_FRAMEWORK_DIR") or (ROOT / ".local/tv-framework")
    parser.add_argument("--framework", type=Path, default=Path(default_framework), help="Path to Tizen reference assemblies directory")
    parser.add_argument("--container", default="CinemaGraphApp", choices=["CinemaGraphApp", "BixbyPromotion"])
    parser.add_argument("--variant", default="full", choices=["full", "lite"], help="Build standard full variant or unprivileged lite variant")
    parser.add_argument("--no-tpk", action="store_true", help="Skip packaging the unsigned TPK archive")
    parser.add_argument("--output-tpk", type=Path, default=None, help="Custom output path for the unsigned TPK archive")
    args = parser.parse_args()

    jobs = []
    if args.target in ("payloads", "all"):
        for name in ("SamsungTvArchiveRoot", "SamsungTvRootAgent"):
            jobs.append((ROOT / f"vendor/payload-source/{name}.csproj", "payloads", name, []))
    if args.target in ("launcher", "all"):
        for name in ("Tizen.NUI", "Tizen.Applications.Common", "Tizen.System", "Tizen"):
            if not (args.framework / f"{name}.dll").is_file():
                parser.error(f"Missing {name}.dll in {args.framework}. Specify --framework <dir> or set TIZEN_FRAMEWORK env var.")
        for name in ("Home", "QuickLaunch"):
            extra = [f"-p:TV_FRAMEWORK_DIR={args.framework.resolve()}", f"-p:LAUNCHER_ASSEMBLY_NAME={args.container}"]
            if args.variant == "lite":
                extra.append("-p:VARIANT=Lite")
            jobs.append((ROOT / f"launcher/src/{name}.csproj", name, name, extra))
        jobs.append((ROOT / "launcher/probe/Probe.csproj", "Probe", "Probe", []))

    build_dir = ROOT / (".local/build-lite" if args.variant == "lite" else ".local/build")
    obj_dir = ROOT / (".local/obj-lite" if args.variant == "lite" else ".local/obj")

    for project, output, intermediate, extra in jobs:
        subprocess.run([
            args.dotnet, "build", str(project), "-c", "Release", "--nologo",
            "-o", str(build_dir / output),
            f"-p:BaseIntermediateOutputPath={obj_dir / intermediate}/",
            "-p:Deterministic=true",
            f"-p:PathMap={ROOT}=/_/quiet-tizen", *extra,
        ], check=True, env={**os.environ, "DOTNET_CLI_TELEMETRY_OPTOUT": "1"})

    if args.target in ("launcher", "all"):
        destination = ROOT / (".local/package-lite" if args.variant == "lite" else ".local/package")
        destination.mkdir(parents=True, exist_ok=True)
        launcher_dll = build_dir / f"Home/{args.container}.dll"
        shutil.copyfile(launcher_dll, destination / f"{args.container}.dll")
        runtime_config = build_dir / f"Home/{args.container}.runtimeconfig.json"
        if runtime_config.is_file():
            shutil.copyfile(runtime_config, destination / f"{args.container}.runtimeconfig.json")
        if args.container != "BixbyPromotion" and (build_dir / "Home/BixbyPromotion.dll").is_file():
            shutil.copyfile(build_dir / "Home/BixbyPromotion.dll", destination / "BixbyPromotion.dll")
        shutil.copyfile(build_dir / "QuickLaunch/QuickLaunch.dll", destination / "QuickLaunch.dll")
        shutil.copyfile(build_dir / "Probe/Probe.dll", destination / "Probe.dll")
        assets = destination / "quiet-home"
        if assets.exists():
            shutil.rmtree(assets)
        shutil.copytree(ROOT / "launcher/assets", assets)
        local_catalog = ROOT / "launcher/assets/apps.local.json"
        if local_catalog.exists():
            shutil.copyfile(local_catalog, assets / "apps.json")
        (assets / "apps.local.json").unlink(missing_ok=True)
        print(f"Deployable files staged at: {destination}")

        if not args.no_tpk:
            out_tpk = args.output_tpk or (ROOT / f"opt-quiet-tizen-{args.variant}-unsigned.tpk")
            package_unsigned_tpk(destination, out_tpk, args.variant, args.container)
            print("\n" + "=" * 80)
            print("BUILD COMPLETED: Unsigned package created successfully!")
            print(f"Artifact: {out_tpk} ({out_tpk.stat().st_size:,} bytes)")
            print("\nNEXT STEPS (SIGNING WITH YOUR SAMSUNG DUID):")
            print("  Samsung TV firmware enforces that packages are signed with a distributor")
            print("  certificate registered specifically to your TV's unique DUID.")
            print("\n  1. Create your Samsung Certificate Profile in Tizen Studio:")
            print("     Tools > Certificate Manager > + > Samsung (Author + Distributor)")
            print("  2. Sign this package with your profile:")
            print(f"     tizen package -t tpk -s <YOUR_PROFILE_NAME> -- {out_tpk}")
            print("  3. Install onto your TV:")
            print(f"     sdb install {out_tpk}")
            print("=" * 80 + "\n")


if __name__ == "__main__":
    main()

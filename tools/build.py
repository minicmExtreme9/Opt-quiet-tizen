#!/usr/bin/env python3
"""Build locally; never redistribute Samsung reference assemblies."""
import argparse
import os
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("target", choices=["payloads", "launcher", "all"])
    default_dotnet = os.environ.get("DOTNET") or (str(Path.home() / ".dotnet/dotnet.exe") if (Path.home() / ".dotnet/dotnet.exe").is_file() else "dotnet")
    parser.add_argument("--dotnet", default=default_dotnet)
    parser.add_argument("--framework", type=Path, default=ROOT / ".local/tv-framework")
    parser.add_argument("--container", default="CinemaGraphApp", choices=["CinemaGraphApp", "BixbyPromotion"])
    parser.add_argument("--variant", default="full", choices=["full", "lite"], help="Build standard full variant or unprivileged lite variant")
    args = parser.parse_args()
    jobs = []
    if args.target in ("payloads", "all"):
        for name in ("SamsungTvArchiveRoot", "SamsungTvRootAgent"):
            jobs.append((ROOT / f"vendor/payload-source/{name}.csproj", "payloads", name, []))
    if args.target in ("launcher", "all"):
        for name in ("Tizen.NUI", "Tizen.Applications.Common", "Tizen.System", "Tizen"):
            if not (args.framework / f"{name}.dll").is_file():
                parser.error(f"Missing {name}.dll. Read docs/04-launcher.md; these are not bundled.")
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
        print(f"Deployable files: {destination}. Review the catalog and input mapping before use.")


if __name__ == "__main__":
    main()

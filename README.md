# Opt-quiet-tizen v1.0

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Original Author](https://img.shields.io/badge/Original%20Author-jss8649-informational.svg)](https://github.com/jss8649/quiet-tizen)
[![Platform](https://img.shields.io/badge/Platform-Tizen%206.5%20%7C%207.0%20%7C%208.0%20%7C%209.0-0080FF.svg)](https://tizen.org)
[![Runtime](https://img.shields.io/badge/.NET%20Runtime-6.0-512BD4.svg)](https://dotnet.microsoft.com)
[![Target](https://img.shields.io/badge/Target-Samsung%20Smart%20TV-1428A0.svg)](https://samsung.com)
[![UI Performance](https://img.shields.io/badge/UI%20Performance-60%20FPS%20%7C%20104ms-brightgreen.svg)](#performance-benchmarks)

A lightweight, ad-free, hardware-accelerated native launcher and optimization suite for **Samsung Tizen Smart TVs**. 

> [!NOTE]
> This repository is an **enhanced fork** of the foundational work [Quiet Tizen](https://github.com/jss8649/quiet-tizen) created by **jss8649**. It preserves the original author's research and agentic workflows while adding native D-pad icon reordering, tactile hardware audio, Apple TV-styled fluid focus animations, high-definition console artwork, real-world retail QLED benchmarks, and automated 1-click deployment tools.

---

## 📸 Preview

![Quiet Home Preview](docs/quiet-home-preview.png)

*Quiet Home running live on a Samsung QLED TV: distraction-free layout with direct HDMI inputs, your favorite streaming apps, quick settings, and zero sponsored bloatware (And to my liking, no recommended content).*

---

## 🚀 Why this Fork? (Key Enhancements)

While the original upstream repository documented a targeted case study on a single U8000 model, this fork transforms Quiet Tizen into **Opt-quiet-tizen v1.0**, a **practical, feature-complete daily driver**:

1. **📱 Native Remote-Control Icon Reordering**:
   - Long-press **OK / Enter** for 500 ms on any app icon to enter interactive edit mode.
   - Distinct amber/gold focus ring with elevated card z-index.
   - Move apps fluidly across grid slots and multiple pages using the D-pad arrows.
   - Atomic instant persistence directly to local storage (`apps.json`).
2. **🍏 Fluid Apple TV-Styled Focus Animations**:
   - Natural, organic scale, depth, and elevation transitions styled after **Apple TV (tvOS)**.
   - Cards smoothly zoom and elevate with hardware-accelerated cubic easing when navigating via the remote.
   - Driven directly by the native Tizen DALi GPU compositor at a solid **60 FPS** without web DOM layout shifts.
3. **⚡ 100% Native Tactile Audio Feedback**:
   - Integrated with Samsung's native `libmmfkeysound.so.0` (`mm_sound_play_keysound`), delivering instant tactile remote clicks without spawning child processes or risking kernel thread crashes.
4. **🎨 Crisp 1:1 Vector & Hi-Res Graphics Pipeline**:
   - Custom high-definition assets for modern consoles (**PlayStation 4 / PC**, **Nintendo Switch**, **HDMI 3**) and streaming services (**Netflix**, **YouTube**, **Crunchyroll**, **HBO Max**, **Disney+**, **Apple Music**, and more).
   - Filtered with hardware Lanczos sampling (`BoxThenLanczos`) to eliminate GPU blurriness on 4K/QLED panels.
5. **📊 Tested & Verified on Modern QLED Hardware**:
   - Verified on physical retail hardware: **Samsung QLED QN55Q7FAAPXPA** running firmware `T-RSLFUABC-0090-1300.9` (Tizen 9.0 / RoseL platform).
6. **📦 Pre-Built 1-Click TV Deployment Package**:
   - Ready-to-run release bundle under **10 MB** — no .NET compiler or SDK installation required on your host PC.
   - Includes `deploy-to-tv.bat` for hands-off automated TV deployment, plus a standalone `.tpk` package for Tizen Studio.

---

## ⚡ Performance Benchmarks

Smart TV SoCs have limited CPU horsepower. Architectural choices dictate whether your home screen feels like a sluggish web page or an instant gaming console.

Here is the real-world performance measured live on a physical **Samsung QN55Q7FAAPXPA**:

| Metric | Stock Samsung Smart Hub | Web Launchers (Free My TV / TizenBrew) | Opt-quiet-tizen v1.0 |
|---|---|---|---|
| **Underlying Engine** | Heterogeneous (native shell + web widget embeds + ad network) | Chromium/WebKit Web Engine inside container | **Native .NET 6.0 with Tizen NUI / DALi GPU pipeline** |
| **First UI Render Time** | 2,400 – 4,500 ms (ad network delay) | 1,200 – 2,200 ms (DOM parse & layout) | **104 – 114 ms** (measured live) |
| **D-Pad Navigation Latency** | 180 – 350 ms | 220 – 450 ms (event loop queue) | **< 16 ms** (direct hardware event loop) |
| **Display Refresh Rate** | 25 – 45 FPS (stutter during ad refreshes) | 15 – 25 FPS (CPU throttling on CSS blur) | **Solid 60 FPS** (hardware compositing) |
| **Focus Animation Feel** | Clunky step animations | CSS transition drops frames on blur | **Smooth Apple TV-styled hardware zoom & easing** |
| **Tactile Audio Latency** | ~120 ms | > 200 ms (Web Audio context) | **< 5 ms** (`libmmfkeysound.so.0`) |
| **Network Traffic on Launch** | Heavy (telemetry & dynamic ad banners) | Moderate/Heavy (TMDB API, weather, remote images) | **0 bytes** (100% offline & autonomous) |
| **Available RAM (`MemAvailable`)** | ~500 – 540 MiB | ~520 – 580 MiB | **~900 – 922 MiB** available for gaming & apps |
| **App Reordering Speed** | Slow carousel editing with cloud sync | DOM re-render with layout shift | **< 5 ms** ($O(1)$ memory swap + atomic save) |

> [!TIP]
> For a detailed technical breakdown of memory graphs and graphics pipelines, read [docs/07-performance.md](docs/07-performance.md).

---

## 🎮 Features

- **Direct Hardware Inputs**: Instant 1-click switching to HDMI 1, HDMI 2, or HDMI 3 without traversing deep vendor menus.
- **Apple TV (tvOS) Motion & Aesthetics**: Silky-smooth focus transitions, subtle depth elevation, and reactive cards designed for tactile remote navigation.
- **Native Quick Settings**: Adjust Volume, Brightness, Game Mode, Sleep Timer, and Mute directly on screen without opening Samsung's full-screen menu.
- **Dark & Light Mode**: Built-in SVG-rendered themes switchable with one click from the toolbar.
- **DualSense / Bluetooth Pairing**: Direct shortcut to Samsung's Bluetooth controller pairing subsystem.
- **Zero Sponsored Ads**: No sponsored tiles, no TV Plus autostart, and no background tracking daemons.
- **Automatic State Persistence**: Preserves your custom app order, selected theme, and last-used input across full cold restarts.

---

## 🕹️ How to Reorder Apps on your TV

App reordering works natively using only your standard TV remote:

```
[ Focus on App ] ──( Hold OK for 500ms )──> [ Ring turns Gold / Elevated ]
                                                         │
                                        ( Use Arrow Keys: ◄ ▲ ▼ ► )
                                                         │
                                                         ▼
[ Position Saved ] ◄──( Press OK or BACK )── [ App Swaps Position ]
```

1. Navigate to the **Apps** tab.
2. Focus on any app you want to reposition.
3. **Hold the OK / Enter button** on your remote for ~500 ms.
   - The card elevates and the focus ring turns **bright gold**.
   - The status bar displays: `Mover '<App>' con flechas · OK para fijar`.
4. Use the **Arrow keys** (Up, Down, Left, Right) to move the card across slots and pages.
5. Press **OK / Enter** (or **BACK**) to drop the card and save your changes permanently.

---

## 🚀 Getting Started

### 🤖 Using with an AI Coding Agent

This repository is completely structured to be agent-readable. If you are using Codex, Claude, ChatGPT, or Antigravity, clone the repository, open it in your assistant workspace, and hand it the original author's prompt:

> *Read README.md and AGENTS.md, then inspect the documentation before touching my TV. Help me reproduce Quiet Tizen on a TV I own. First identify its exact model, firmware and runtime, compare them with the tested profile, and show me any compatibility gaps. Keep HDMI, sound, networking, Bluetooth and accessibility working. Prefer HDMI and minimize smart features; keep the streaming apps I choose. Back up my actual settings and prepare rollback before each change. Install and verify Quiet Home before assigning it to Home/startup or replacing the slow settings sidebar. Use the measured findings and known limitations rather than assuming every package is spyware or promising a particular speed improvement.*

---

### 📋 Prerequisites (Must-Haves)

Before deploying to your TV, ensure your PC and TV have the following requirements ready:

#### 1. TV Requirements
* **Samsung Smart TV** running Tizen 6.5, 7.0, 8.0, or 9.0.
* Connected to the **same local Wi-Fi or Ethernet network** as your computer.
* **Developer Mode enabled** (instructions in Step 1 below).

#### 2. PC Requirements (Must-Haves)
* **Python 3.10+**: Ensure you tick the checkbox *"Add Python to PATH"* during installation ([python.org](https://www.python.org/downloads/)).
* **Samsung Smart Development Bridge (`sdb.exe`)**: Required to establish the authenticated communication channel with the TV over port `26101`. Due to Samsung's proprietary license, SDB cannot be redistributed inside open-source repositories. Choose one of two options to obtain it:

  * **Option A: Tizen CLI Tools Only (Recommended — Lightweight ~100 MB)**
    1. Download the standalone **Tizen Studio CLI** installer from the [Tizen Developer Download Page](https://developer.tizen.org/development/tizen-studio/download).
    2. Run the installer and add the `tizen-studio/tools` directory to your Windows system `PATH` (or place `sdb.exe` inside the project folder).
    3. Verify in your terminal: `sdb version`.
  
  * **Option B: Tizen Studio with IDE (Full Development Suite — ~5 GB)**
    1. Download and install **Tizen Studio with IDE** from the [Tizen Developer Download Page](https://developer.tizen.org/development/tizen-studio/download).
    2. Includes the full graphical suite: **Tizen Device Manager**, Certificate Manager, and official emulators.
    3. The deployment script automatically discovers `sdb.exe` at `C:\tizen-studio\tools\sdb.exe`.

---

### 📥 Installation & Deployment

#### Option 1: Direct 1-Click TV Install (Recommended)

Once the prerequisites are ready, deployment is fully automated with no manual assembly compilation:

1. **Enable Developer Mode on your Samsung TV**:
   - Open the **Apps** panel on your TV.
   - On your physical remote, press **`1` `2` `3` `4` `5`** in sequence.
   - When the Developer Mode dialog appears, toggle the switch to **ON**.
   - Enter your computer's local network IP (e.g., `192.168.1.X`) in the **Host PC IP** field.
   - **Perform a cold reboot**: Press and hold the **Power** button on your remote for ~5 seconds until the TV turns off and reboots displaying the Samsung / QLED startup logo.

2. **Deploy with 1 Click**:
   - Download the latest **[Opt-quiet-tizen v1.0 Package (`Opt-quiet-tizen-v1.0.zip`)](https://github.com/minicmExtreme9/Opt-quiet-tizen/releases/latest)** from the [Releases page](https://github.com/minicmExtreme9/Opt-quiet-tizen/releases).
   - Extract the `.zip` archive on your computer.
   - Double-click and run `tv-direct-install/deploy-to-tv.bat`.
   - Enter your TV's local IP address when prompted (e.g., `192.168.1.32`).
   - The script automatically verifies access, deploys the optimized assembly, syncs artwork, and launches Quiet Home directly on your screen.

#### Option 2: Standalone TPK Package (Tizen Studio / SDB CLI)

For developers using Tizen Device Manager, emulators, or SDB CLI, the release archive includes a self-contained `.tpk` installer inside the `tizen-tpk-installer/` folder:

```sh
# Connect to your TV or emulator
sdb connect <TV-IP>

# Install the standalone TPK
sdb install tizen-tpk-installer/Opt-quiet-tizen-v1.0.tpk
```

#### Option 3: Build from Source

Requirements:
- Python 3.12+
- .NET 6.0 SDK (`dotnet`)
- Samsung Tizen Studio SDB tools

```sh
# Clone this enhanced fork:
git clone https://github.com/minicmExtreme9/Opt-quiet-tizen.git
cd Opt-quiet-tizen

# Original upstream repository:
# https://github.com/jss8649/quiet-tizen

# Run tests:
python3 -m unittest discover -s tests -v

# Obtain matching Tizen assemblies from your TV:
# tv pull /usr/share/dotnet.tizen/netcoreapp/Tizen.NUI.dll .local/tv-framework/Tizen.NUI.dll
# (repeat for Tizen.Applications.Common, Tizen.System, Tizen)

# Build the native launcher:
python tools/build.py launcher --framework .local/tv-framework
```

---

## 📚 Technical Documentation & Walkthrough

Inherited from the original project and updated for modern platforms:

1. [Understand compatibility and inventory the TV](docs/01-inventory.md).
2. [Enable Developer Mode and check temporary access](docs/02-access.md).
3. [Take snapshots and apply selected cleanup](docs/03-cleanup.md).
4. [Build, install and assign Quiet Home](docs/04-launcher.md).
5. [Pair a DualSense using the TV's Bluetooth support](docs/05-controller.md).
6. [Customize apps, fetch artwork and keep it preloaded](docs/06-artwork.md).
7. [Diagnose slow streaming apps and view benchmarks](docs/07-performance.md).
8. [Understand privacy and telemetry findings](docs/08-privacy.md).
9. [Restore the original experience safely](docs/09-recovery.md).

---

## 🔬 Tested Hardware Comparison

| Item | Original Upstream Baseline | Opt-quiet-tizen v1.0 |
|---|---|---|
| **Model** | `UN75U8000FFXZA` (2025 U8000F) | `QN55Q7FAAPXPA` (QLED 55") |
| **Firmware** | `T-KSU2EFAKUC-0090-1300.9` | `T-RSLFUABC-0090-1300.9` |
| **Platform** | Tizen 9.0, Linux 5.4.261, ARMv7 | Tizen 9.0, Linux 5.4.261, ARMv7 (RoseL) |
| **Runtime** | .NET Runtime 6.0.9 | .NET Runtime 6.0.9 |
| **Animations** | Static snap navigation | **Fluid Apple TV-styled focus zoom & depth easing** |
| **Icon Reordering** | Fixed static JSON order | **Interactive D-pad 500ms long-press reorder** |
| **Navigation Click** | System default / silent | **Native `libmmfkeysound.so.0` tactile sound** |
| **Deployment** | Multi-step SDB shell scripts | **1-click automated script (`deploy-to-tv.bat`) + TPK** |

---

## 🛠️ Project Structure

```
quiet-tizen/
├── launcher/
│   ├── src/                 # Native C# NUI Application (Home.cs, AppLibrary.cs)
│   ├── assets/              # Hi-res artwork, vector SVGs, custom click audio
│   └── probe/               # Hardware diagnostic tool (Probe.cs)
├── profiles/                # Tested TV hardware signatures (QN55Q7F, U8000F)
├── recipes/                 # Shell scripts for snapshot, launcher, and recovery
├── tools/                   # Build scripts, TV access bridge (tv.py), packaging
└── docs/                    # Technical documentation
    ├── 01-inventory.md      # Hardware and firmware compatibility inspection
    ├── 02-access.md         # SDB authentication and session security
    ├── 03-cleanup.md        # Registry snapshots and safe daemon disabling
    ├── 04-launcher.md       # Launcher integration and assembly replacement
    ├── 05-controller.md     # DualSense Bluetooth controller pairing
    ├── 06-artwork.md        # Texture caching and pixel-perfect rendering
    ├── 07-performance.md    # Real-world benchmark data & architecture analysis
    ├── 08-privacy.md        # Network telemetry findings
    └── 09-recovery.md       # Safe uninstallation and stock restoration
```

---

## 🛡️ Open Source Compliance, Attribution & Credits

This project strictly adheres to open source principles, licensing agreements, and utmost respect for the original authors:

- **Original Project & Research**: Created by **jss8649** ([@jss8649](https://github.com/jss8649)) at [quiet-tizen](https://github.com/jss8649/quiet-tizen). All credit for the reverse engineering methodology, architecture design, and original case study belongs to the original author (jss8649).
- **Visual Design Concept**: Adapted from the open-source [Free My TV](https://github.com/simbasjungles/free-my-tv) project by Simba's Jungles. This repository is a native Tizen C#/NUI rewrite of the visual approach, not an Android port.
- **Temporary Access Route**: The archive-launcher access mechanism is based on [samsung-tv-root](https://github.com/chris-ritsen/samsung-tv-root) by Chris Ritsen.
- **License**: All original and modified code is licensed under the [MIT License](LICENSE).
- **Third-Party Assets**:
  - Fonts: Inter Font Family (SIL Open Font License 1.1).
  - Material Symbols: Apache 2.0 license.
  - See [NOTICE.md](NOTICE.md) and [docs/provenance.md](docs/provenance.md) for full license texts and commit provenance.
  - **No proprietary Samsung firmware, stock application binaries, or protected television backups are included in this repository.**

---

## ⚠️ Disclaimer

This is an independent open-source project. Samsung, Tizen, QLED, PlayStation, Nintendo Switch, and other brand names are trademarks of their respective owners. This project is not affiliated with, endorsed by, or sponsored by Samsung Electronics Co., Ltd. Use at your own discretion.

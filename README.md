# Opt-quiet-tizen v1.0.1

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Original Author](https://img.shields.io/badge/Original%20Author-jss8649-informational.svg)](https://github.com/jss8649/quiet-tizen)
[![Status](https://img.shields.io/badge/Status-Beta%20%7C%20Daily%20Driver%20Ready-brightgreen.svg)](#)
[![Platform](https://img.shields.io/badge/Platform-Tizen%206.5%20%7C%207.0%20%7C%208.0%20%7C%209.0-0080FF.svg)](https://tizen.org)
[![Runtime](https://img.shields.io/badge/.NET%20Runtime-6.0-512BD4.svg)](https://dotnet.microsoft.com)
[![Target](https://img.shields.io/badge/Target-Samsung%20Smart%20TV-1428A0.svg)](https://samsung.com)
[![UI Performance](https://img.shields.io/badge/UI%20Performance-60%20FPS%20%7C%2092ms-brightgreen.svg)](#performance-benchmarks)
[![Changelog](https://img.shields.io/badge/Changelog-v1.0.1-orange.svg)](CHANGELOG-v1.0.1.md)

A lightweight, ad-free, hardware-accelerated native launcher and optimization suite for **Samsung Tizen Smart TVs**. 

> [!NOTE]
> This repository is an **enhanced fork** of the foundational work [Quiet Tizen](https://github.com/jss8649/quiet-tizen) created by **jss8649**. It preserves the original author's research and agentic workflows while adding native D-pad icon reordering, tactile hardware audio, Apple TV-styled fluid focus animations, dynamic console artwork, real-world retail QLED benchmarks, and automated 1-click deployment tools.

> [!IMPORTANT]
> **Release Status:** Opt-quiet-tizen is currently in **Beta, but rock-solid and stable enough for daily use** as your TV's primary home screen. All core flows (HDMI port switching, streaming apps launch, D-pad navigation, volume/brightness controls, and sleep timers) are tested live on retail hardware and run 100% autonomously without external servers or cloud dependencies.

---

## 📸 Preview

![Quiet Home Preview](docs/quiet-home-preview.png)

*Quiet Home running live on a Samsung QLED TV: distraction-free layout with direct HDMI inputs, your favorite streaming apps, quick settings, and zero sponsored bloatware (And to my liking, no recommended content).*

---

## 🚀 Why this Fork? (Key Enhancements)

While the original upstream repository documented a targeted case study on a single U8000 model, this fork transforms Quiet Tizen into **Opt-quiet-tizen v1.0.1**, a **practical, feature-complete daily driver**:

1. **📱 Native Remote-Control Icon Reordering**:
   - Long-press **OK / Enter** for 500 ms on any app icon to enter interactive edit mode.
   - Distinct amber/gold focus ring with elevated card z-index.
   - Move apps fluidly across grid slots and multiple pages using the D-pad arrows.
   - Atomic instant persistence directly to local storage (`apps.json`).
2. **🍏 Fluid Apple TV-Styled Focus Animations**:
   - Natural, organic scale, depth, and elevation transitions styled after **Apple TV (tvOS)**.
   - Cards smoothly zoom and elevate with hardware-accelerated cubic easing when navigating via the remote.
   - Driven directly by the native Tizen DALi GPU compositor at a solid **60 FPS** without web DOM layout shifts.
3. **⚡ Native TV Audio Feedback & Toggle**:
   - Integrated with Samsung's native system audio feedback (`/usr/share/feedback/sound/operation/move.wav` via `libmmfkeysound.so.0`), delivering the authentic TV remote navigation click with zero latency and an on/off sound toggle.
4. **🔧 Dedicated Launcher Preferences Modal ("Preferencias del launcher")**:
   - Access via the dedicated **Wrench icon (`🔧`)** in the top-right toolbar.
   - Clean UI without bracket clutter: directional arrows (`◀ Option ▶`, `OK`) for effortless remote control.
   - Customize HDMI 1, 2, and 3 names and descriptions on the fly, with quick presets for consoles, PCs, and streaming boxes.
   - Integrated **10-Language Switcher**: Español, English, Deutsch, Português, Français, 中文 (Mandarin), 日本語, 한국어, हिन्दी, and العربية.
   - **Sound & Animation Toggles**: Customize sound effects and switch between fluid Apple TV animations or instantaneous (0 ms) snap navigation.
5. **🖼️ Dynamic HDMI 1 Hero Artwork**:
   - Custom-crafted landscape vector illustrations for **Xbox Series X / S**, **Nintendo Switch**, **Apple TV 4K**, **Gaming PC**, **Blu-ray / DVD**, **Cable TV / Box**, and **PlayStation 4 / 5**.
   - Dynamically updates the main Hero card in real time as you adjust your presets, with dimension-isolated caching preventing portrait/landscape distortion.
6. **📏 Dynamic Tab Pill Sizing**:
   - Real-time calculation of tab header widths (*Home*, *Apps*, *TV Settings*) based on translated string length and script density.
   - Completely eliminates ellipsis truncation (e.g. `Paramètr...`, `Configura...`, `TV-Einste...`).
7. **📄 Smart Pagination Dots for Apps**:
   - Dynamic page indicator (● ○ ○) in the Apps library when apps span multiple pages, deactivated when single-page to conserve memory.
8. **🎨 Crisp 1:1 Vector & Hi-Res Graphics Pipeline**:
   - Custom high-definition assets for modern consoles and streaming services (**Netflix**, **YouTube**, **Crunchyroll**, **HBO Max**, **Disney+**, **Apple Music**, and more).
   - Seamless return from third-party apps with zero multi-frame resize glitch.
9. **📊 Tested & Verified on Modern QLED Hardware**:
   - Verified on physical retail hardware: **Samsung QLED QN55Q7FAAPXPA** running firmware T-RSLFUABC-0090-1300.9 (Tizen 9.0 / RoseL platform).
10. **📦 Reproducible Builds & Device-Bound Signing**:
   - Open-source structured codebase allowing any user to compile locally with .NET 6 and sign using their own Samsung Certificate Profile tied to their TV's specific DUID.
   - Dual-target support for both Full and unprivileged Lite variants. See the [Build & Install on Your Own TV](#-build--install-on-your-own-tv) guide.

---

## ⚡ Performance Benchmarks

Smart TV SoCs have limited CPU horsepower. Architectural choices dictate whether your home screen feels like a sluggish web page or an instant gaming console.

Here is the real-world performance measured live on a physical **Samsung QN55Q7FAAPXPA**:

| Metric | Stock Samsung Smart Hub | Web Launchers (Free My TV / TizenBrew) | Opt-quiet-tizen v1.0.1 (Beta) |
|---|---|---|---|
| **Underlying Engine** | Heterogeneous (native shell + web widget embeds + ad network) | Chromium/WebKit Web Engine inside container | **Native .NET 6.0 with Tizen NUI / DALi GPU pipeline** |
| **First UI Render Time** | 2,400 – 4,500 ms (ad network delay) | 1,200 – 2,200 ms (DOM parse & layout) | **92 – 104 ms** (measured live) |
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
   - The status bar displays: `Arrows: move · OK: set · Back: exit`.
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

## 🔨 Build & Install on Your Own TV

Because Samsung TV firmware enforces that every `.tpk` package must be signed with a distributor certificate tied to the **DUID (Device Unique ID)** of a specific TV, **it is impossible to distribute a generic, pre-signed binary** that works across different users' devices. Every user must compile and sign the package using their own Samsung Certificate Profile.

Follow these 8 steps in order to build and install on your TV:

### Step 1: PC Prerequisites
- **.NET 6.0 SDK**: Installed and available in your terminal (`dotnet --version`). If installed in a custom location, set the `DOTNET` environment variable.
- **Python 3.10+**: With the *"Add Python to PATH"* option enabled.
- **Tizen Studio with TV SDK & Tools**:
  1. Download and install **Tizen Studio with IDE** from the [official Tizen Developer site](https://developer.tizen.org/development/tizen-studio/download).
  2. Open the **Tizen Package Manager** (`Tools > Package Manager`).
  3. Under the **Main SDK** tab, install `Tizen SDK tools`.
  4. Under the **Extension SDK** tab, install `TV Extensions-6.5` (or the version matching your TV) and **Samsung Certificate Extension** (mandatory for Step 3).
  5. Ensure that `tizen-studio/tools` and `tizen-studio/tools/ide/bin` are added to your system `PATH` so that the `sdb` and `tizen` CLI commands are accessible.

### Step 2: Connect the TV (Developer Mode & Device Manager)
1. **Enable Developer Mode on your TV**:
   - Open the **Apps** panel on your television.
   - On your physical remote control (or remote app with a numeric keypad), press **`1` `2` `3` `4` `5`** in sequence.
   - In the *Developer Mode* modal dialog, toggle the switch to **ON**.
   - In the **Host PC IP** field, enter your PC's local IP address (e.g., `192.168.1.50`).
   - Press and hold the **Power** button on the physical remote for ~5 seconds until the TV shuts off and restarts displaying the Samsung/QLED boot logo (a cold reboot is required to activate Developer Mode).
2. **Register and connect in Device Manager**:
   - Open **Tizen Device Manager** from Tizen Studio (`Tools > Device Manager`).
   - Click the **Remote Device Manager** icon (top right network icon).
   - Click **Scan** or click **+** to manually add your TV's local IP (port `26101` by default).
   - Connect to the device. Once connected, your TV will appear in the main device list.
   - Select the TV and note down its **DUID** displayed in the *Properties* panel on the right.

### Step 3: Enable the Samsung Certificate Extension
1. Open **Tizen Package Manager** (`Tools > Package Manager`).
2. Select the **Extension SDK** tab.
3. Locate **Samsung Certificate Extension** and click **Install**.
4. Restart Tizen Studio to ensure the extension is fully registered.

### Step 4: Create Your Samsung Certificate Profile (Author + Distributor)
> [!CAUTION]
> **Critical Warning Regarding Error -12 (`Check certificate error [-12]`):**  
> Your profile MUST contain certificates issued by **Samsung**. NEVER mix a generic Tizen developer certificate with a Samsung certificate. Both Author AND Distributor must be of type Samsung, and the distributor certificate must explicitly include your TV's DUID. Mixing certificate types or signing with a distributor that lacks your TV's DUID will cause installation to immediately abort with certificate error -12.

1. In Tizen Studio, open `Tools > Certificate Manager`.
2. Click the **+** button (New Certificate Profile).
3. Select **Samsung** (NOT Tizen!) and click *Next*.
4. Select **TV** and click *Next*.
5. Enter a profile name (e.g., `SamsungTVProfile`).
6. **Author Certificate**:
   - Select *Create a new author certificate*.
   - Sign in with your free **Samsung Developers** account.
   - Complete the wizard to generate your Samsung author key and certificate.
7. **Distributor Certificate**:
   - Select *Create a new distributor certificate*.
   - Select the privilege level (*Public* or *Partner*).
   - In the device list (**DUID**), ensure your TV's DUID obtained in Step 2 is checked or enter it manually.
   - Complete the wizard.
8. Verify that the newly created profile shows a blue checkmark indicating it is the active default profile.

### Step 5: Choose Which Variant to Build (Full vs Lite)
The project provides two variants tailored to your access level and setup:

| Feature | Lite Variant (`--variant lite`) | Full Variant (`--variant full`) |
|---|---|---|
| **Installation Requirements** | **None special**; installs directly as a standard TV app | Requires vendor/ temporary access process ([docs/02-access.md](docs/02-access.md)) |
| **App Launching (Dock & Library)** | ✅ Fully supported (public APIs) | ✅ Fully supported |
| **HDMI Inputs (Direct source switching)** | ✅ Via public Tizen AppControl | ✅ Via public Tizen AppControl |
| **Interactive Remote Icon Reordering** | ✅ Yes (golden ring, atomic storage) | ✅ Yes (golden ring, atomic storage) |
| **DALi GPU UI / Dark & Light Themes** | ✅ Fluid 60 FPS | ✅ Fluid 60 FPS |
| **On-Screen Volume & Mute Controls** | ❌ Omitted (requires private Samsung API) | ✅ Supported (`libcapi-media-sound-manager-tv`) |
| **On-Screen Brightness & Game Mode** | ❌ Omitted (requires private Samsung API) | ✅ Supported (`libsettings-api`) |

*For users seeking a straightforward, hassle-free installation without modifying internal system permissions, the **Lite Variant** is recommended.*

### Step 6: Build with `tools/build.py`
In your terminal at the root of the repository:

- **To build the Lite Variant (Recommended starting point)**:
  ```sh
  python tools/build.py launcher --variant lite
  ```
  *Output: `opt-quiet-tizen-lite-unsigned.tpk`*

- **To build the Full Variant**:
  ```sh
  python tools/build.py launcher --variant full
  ```
  *Output: `opt-quiet-tizen-full-unsigned.tpk`*

*(Note: If your Tizen reference assemblies are not in `.local/tv-framework`, specify their path via `--framework <dir>` or by setting the `TIZEN_FRAMEWORK` environment variable).*

### Step 7: Sign the Package with Your Samsung Profile
Use the Tizen Studio CLI tool specifying the name of your profile created in Step 4:

```sh
# For Lite Variant:
tizen package -t tpk -s <YOUR_SAMSUNG_PROFILE_NAME> -- opt-quiet-tizen-lite-unsigned.tpk

# For Full Variant:
tizen package -t tpk -s <YOUR_SAMSUNG_PROFILE_NAME> -- opt-quiet-tizen-full-unsigned.tpk
```

Alternatively, you can use the Tizen Studio GUI via `Tools > Package > Sign .tpk`. The tool will output the signed package ready for deployment.

### Step 8: Install on Your TV Using SDB
Ensure you are connected to your TV and deploy the signed package:

```sh
# Connect to your TV if not already connected:
sdb connect <YOUR_TV_IP>

# Install the signed package on the TV:
sdb install opt-quiet-tizen-lite.tpk
```

Done! The launcher is now installed on your TV and ready to launch from the apps menu.

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

| Item | Original Upstream Baseline | Opt-quiet-tizen v1.0.1 (Beta) |
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

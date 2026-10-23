# Opt-quiet-tizen v1.0.1 — Release Notes & Changelog

**Release Date:** October 23, 2026  
**Target Hardware:** Samsung Smart TVs (Tizen 6.5, 7.0, 8.0, 9.0 / QLED & Crystal UHD)  
**Binary App ID:** `com.samsung.tv.CinemaGraphApp`  
**License:** MIT (Enhanced fork of Quiet Tizen by jss8649)

---

## 🌟 What's New & Enhancements

### 1. 🖼️ Dynamic HDMI 1 Hero Artwork
- **Console & Device Custom Vectors**: Added custom landscape illustrations for **Xbox Series X / S**, **Nintendo Switch**, **Apple TV 4K**, **Gaming PC**, **Blu-ray / DVD**, **Cable TV / Box**, and **PlayStation 4 / 5**.
- **Real-Time Dynamic Swapping**: Selecting an HDMI preset inside the Wrench preferences automatically updates the main hero card in real time upon closing the menu.
- **Dimension-Isolated Cache**: Images are indexed with resolution keys (`asset@WxH`), eliminating cross-slot aspect ratio conflicts.

### 2. 🔧 Wrench Preferences Menu ("Preferencias del launcher")
- **Top-Right Quick Access**: Replaced the redundant inputs icon with a clean wrench (`🔧`) settings button in the top-right toolbar.
- **Cleaner Interface**: Removed the outer square brackets (`[ ]`) around values in the modal, keeping elegant directional arrows (`◀ PlayStation 4 / PC ▶`, `OK`, etc.).
- **Updated UI Hints**: 
  - Title: *"Preferencias del launcher"* (localized across all 10 languages).
  - Subtitle: *"Flechas para cambiar | Botón de atrás o \"OK\" para guardar"* (localized across all 10 languages).
- **Instant Persistence**: All preferences (HDMI presets, language, sound toggle, animation toggle) are immediately saved to `/tmp/quiet-home-config.json` upon exit.

### 2. 🌍 10-Language Internationalization & CJK/Devanagari/Arabic Font Support
- Expanded language support from 5 to **10 languages**:
  - 🇪🇸 **Español**
  - 🇬🇧 **English**
  - 🇩🇪 **Deutsch**
  - 🇵🇹 **Português**
  - 🇫🇷 **Français**
  - 🇨🇳 **中文 (Mandarin)**
  - 🇯🇵 **日本語 (Japanese)**
  - 🇰🇷 **한국어 (Korean)**
  - 🇮🇳 **हिन्दी (Hindi)**
  - 🇸🇦 **العربية (Arabic)**
- **Native Samsung Font Loading**: Automatically mounts `/usr/share/fonts` alongside bundled fonts in `OnCreate()` to ensure CJK, Arabic, and Devanagari glyphs render crisply without missing character blocks (`tofu`).
- **Dynamic System Date/Time Localization**: Sourced through `GetCurrentCulture()`, updating the top-right date and clock formatting in the active language.

### 3. 📏 Dynamic Tab Pill Sizing
- Eliminated text truncation and ellipsis issues (such as `Paramètr...`, `Configura...`, or `TV-Einste...`).
- Computes tab widths on the fly based on character count and script density (`isWide` detection for CJK, Arabic, and Devanagari) with fluid sequential tab positioning.

### 4. ⚡ Animations Toggle (Zero Latency Mode)
- Added an **Animaciones** toggle inside the Wrench modal (`Activado` / `Desactivado`).
- When disabled, transitions bypass animations and snap instantly with 0 ms latency for users who prefer maximum responsiveness.

### 5. 🔊 Native Sound Rollback & Sound Toggle
- Restored authentic Samsung TV feedback sound (`/usr/share/feedback/sound/operation/move.wav` via native `libmmfkeysound.so.0`).
- Added on/off sound toggle in launcher preferences.

### 6. 📄 Smart Pagination Indicator for Apps
- Dynamic slider dots (● ○ ○) appear automatically on the top right of the Apps tab when installed apps exceed one page.
- Stays completely disabled when apps fit on a single page, conserving GPU/CPU overhead.

---

## 🐛 Bug Fixes

- **HDMI 3 Portrait Card Distortion**: Fixed an issue where HDMI 3 picked up the 1040x442 hero image from cache, causing a cropped/stretched connector graphic. Introduced dimension-aware caching and dedicated `feature-hdmi-hero.svg`.
- **Top-Right Toolbar Clock Collision**: Removed intrusive floating tooltips when focusing top-right toolbar icons, preventing overlap with the clock and date.
- **Settings Card Background Glitch**: Fixed missing-icon background artifact in the TV Settings tab by removing the obsolete `shadow-settings.svg` reference.
- **TV Settings Localization**: Localized all TV Settings cards, descriptions, and HDMI descriptors via the unified `i18n` dictionary.
- **App Icons Visibility**: Restored complete high-resolution SVG and PNG artwork mapping in `apps.json`.
- **Smooth App Resume**: Eliminated the 2-frame resize/zoom glitch when exiting third-party applications back to Quiet Home.

---

## 📦 Verified Hardware Environment

- **Model**: Samsung QLED `QN55Q7FAAPXPA`
- **Firmware**: `T-RSLFUABC-0090-1300.9` (Tizen 9.0 / RoseL platform)
- **Runtime**: .NET Runtime 6.0.9 / Tizen NUI
- **Measured Render Time**: 92 – 104 ms
- **Navigation Latency**: < 16 ms (60 FPS solid)

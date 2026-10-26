# Changelog

All notable changes to **Opt-quiet-tizen** are documented in this file.

---

## v2.0.0 — Architecture & Distribution Overhaul

### Breaking Changes
- **Distribution model changed.** Pre-signed `.tpk` downloads are no longer
  provided. Samsung TV firmware requires packages to be signed with a
  distributor certificate tied to the specific TV's DUID, so a single
  pre-signed binary cannot work across devices. You now build and sign
  locally with your own Samsung certificate profile. See the
  "Build & Install on Your Own TV" section in [README.md](README.md).

### Added
- `SettingsStore.cs`: typed configuration via `System.Text.Json`, with
  automatic migration from the legacy hand-parsed JSON format.
- `Localization.cs`: key-based nested dictionary for i18n (10 languages),
  replacing positionally-indexed string arrays.
- `NativeCapabilities.cs`: centralizes all P/Invoke calls to Samsung/Tizen
  native libraries, with runtime capability probing and graceful UI
  degradation when a given library isn't available.
- `DebugLog.cs`: in-memory ring buffer logging with an on-screen viewer
  (Play/Pause key), replacing silent exception swallowing and per-event
  disk writes.
- `AtomicFileWriter.cs`: atomic, fsync-backed writes (temp file + rename)
  for all persistent data, verified to survive a real power-loss event on
  physical hardware.
- `opt-quiet-tizen-lite` build variant: public-API-only build requiring no
  elevated access, installable via standard Developer Mode.

### Fixed
- Dynamic resolution-based layout (derived from `Window.Default.WindowSize`)
  replacing hardcoded pixel positions, across the main UI, both modals, and
  the app library grid.
- Dark/light theme preference now persists through `SettingsStore` instead
  of a `tmpfs`-backed file that was lost on reboot.
- App reordering save path verified to survive both a soft reboot and a
  real power-cut, using atomic writes with a single source of truth
  (`/opt/usr/home/owner/data/`), removing two redundant/no-op write paths
  that existed in earlier iterations.

### Known Limitations
- Validated only on Samsung QN55Q7FAAPXPA, firmware
  T-RSLFUABC-0090-1300.9 (Tizen 9.0). Not verified on other models, SoCs,
  native resolutions besides 1080p, or other Tizen firmware versions.
- Power-cut-mid-write for the reordering save path is covered by design
  (orphaned `.tmp` cleanup) but not directly verified on hardware.
- Behavior after a Samsung OTA firmware update has not been tested.

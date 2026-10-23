# Changelog

All notable changes to the **Opt-quiet-tizen** project will be documented in this file.

---

## [v1.0.1] - 2026-10-23

See [CHANGELOG-v1.0.1.md](CHANGELOG-v1.0.1.md) for full details.

### Added
- **Preferencias del Launcher**: Dedicated modal accessed via the top-right Wrench icon (`🔧`) to configure HDMI preset names, language, sound, and animations.
- **Clean Selection UI**: Removed outer brackets `[ ]` in modal values, presenting clean directional navigation arrows (`◀ ▶`).
- **10 Languages Support**: Added full localized strings and date formats for Spanish, English, German, Portuguese, French, Mandarin Chinese, Japanese, Korean, Hindi, and Arabic.
- **Native Font Directory Integration**: Automatically registers `/usr/share/fonts` to ensure glyph rendering for CJK, Arabic, and Devanagari.
- **Dynamic Tab Header Sizing**: Sizing tabs dynamically according to translated string length and script width, eliminating text truncation ellipses.
- **Animations Toggle**: User preference to disable UI transitions for instantaneous (0 ms) snap navigation.
- **Sound Toggle & Native TV Feedback Audio**: Restored authentic TV hardware feedback sound (`/usr/share/feedback/sound/operation/move.wav` via `libmmfkeysound.so.0`) with on/off switch.
- **Apps Pagination Dots**: Adaptive indicator showing page count and current page in the apps grid, dormant when single-page.

### Fixed
- Fixed top-right toolbar labels overlapping the date/clock widget.
- Fixed gray missing-image artifact behind TV Settings tiles by removing broken `shadow-settings.svg` references.
- Fixed unlocalized text in TV Settings tiles and HDMI port descriptions.
- Restored missing app icon artwork in `apps.json`.
- Smoothed 2-frame zoom/flicker artifact when returning to Quiet Home from background applications.

---

## [v1.0.0] - 2026-10-22

### Added
- Interactive D-pad 500 ms long-press icon reordering.
- Apple TV-styled fluid focus zoom animations at 60 FPS.
- Native TV audio feedback.
- Custom high-definition assets for PS4, Nintendo Switch, HDMI 3, and streaming services.
- 1-click automated deployment tools (`deploy-to-tv.bat`) and standalone TPK for retail TVs.
- Retail QLED benchmarks on model `QN55Q7FAAPXPA`.

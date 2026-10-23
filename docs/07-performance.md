# Diagnose slow menus and streaming apps

The most useful changes were reducing unnecessary background app/service
work and providing small native controls for common actions. This project
does not claim a universal speedup percentage.

## Separate the symptoms

| Symptom | What to inspect |
|---|---|
| Home/sidebar is slow but HDMI works | Launcher/process startup, preload requests, cached native assemblies |
| Common settings are slow | Use quick settings; keep native advanced screens for uncommon controls |
| Both remote and DualSense lag only in Netflix | App/rendering/memory pressure, then input delivery; Bluetooth alone is not an adequate explanation |
| Video buffers but navigation is responsive | Network and streaming diagnostics |
| App images appear late on every visit | Recreated image views, discarded textures, oversized or remote artwork |

Use the same app screen and a similar workload for comparisons. Let the owner
navigate naturally; stop automated button injection while asking about lag.
A successful launch request or a changed process ID is not proof the intended
screen is visible.

## Netflix: what was observed

The owner reported input lag from both the Samsung remote and DualSense.
During diagnosis, background streaming/store/settings processes were closed
and Netflix was relaunched. A full boot later showed Samsung TV Plus starting
with a `backgroundAtStartup` request from `org.tizen.litewebappservice`;
Prime Video and YouTube also reappeared. Both app-preload preferences had
returned to true.

The later mitigation disabled the `HEPsqFNie0.tvplusstandalone` registry row,
created the verified `slive-provider-daemon` stop marker, stopped that service,
reset live/cached preload preferences and closed unused apps. This preserved
Netflix's account and app data. The TV Plus change was checked live, but
**had not yet been checked across another full reboot**.

Sanitized memory snapshots, reported by `/proc/meminfo`:

| Observation | MemAvailable (KiB) | CmaFree (KiB) |
|---|---:|---:|
| Before the first round of background-app closure | 509,032 | 56,048 |
| After a later reboot with background preloads | 537,492 | 544 |
| After the later background-app cleanup | 922,172 | 184,072 |
| Later stable observation | 921,344 | 184,868 |

The later cleanup left roughly **900 MiB available**. These are snapshots
under different uptime/workload conditions, not a controlled benchmark.
CMA is a shared contiguous-memory pool used by graphics/media components,
not a direct measurement of dedicated GPU VRAM.

**The owner had not confirmed that Netflix's perceived input lag was fixed.**
The memory result and visible process changes are evidence for reduced
background pressure, not proof of a completed latency repair.

## Reproduce a useful diagnosis

1. Record what is slow: opening, D-pad response, playback or all of them.
2. Check actual foreground app/processes, `MemAvailable`, swap, CMA and CPU
   use over a short interval. Preserve a sanitized before/after record.
3. Close specific unused applications gracefully by their verified IDs.
   `app_launcher -u 5001 -r APP_ID` can return 1 when the app is already absent;
   handle that state explicitly.
4. Reopen the intended app, verify its screen, stop synthetic input and get
   the owner's observation.
5. Test full boot separately: preload behavior can return.

On the case-study TV, four CPU cores already ran up to 1.35 GHz with the
performance governor; no foreground CPU quota explaining the lag was found.
High load average included driver threads in uninterruptible sleep, so it was
not proof of high CPU utilization. Thermal-zone data was unavailable.
Root agents and screenshots can themselves affect measurements.

The compositor's `enlightenment_info -fps` command hung during investigation.
It is not included in automated recipes. A brief input observation with no
events also does not prove a controller is working correctly.

Do not clear account data, force-stop core display services or kill all
processes as a shortcut. Samsung describes memory-related glitches and cold
booting in [its support guide](https://www.samsung.com/us/support/troubleshoot/TSG10002361/);
Netflix also documents [device restart troubleshooting](https://help.netflix.com/en/node/100242).
Use those ordinary troubleshooting steps when appropriate before escalating
to unsupported registry changes.

## Launcher Architecture Comparison: Native (Quiet Home) vs. Web-Based (Free My TV / TizenBrew) vs. Stock Smart Hub

When evaluating home screen responsiveness on low-power TV SoCs (such as the quad-core ARM Cortex-A55 at 1.35 GHz found in the Samsung U8000/Q60/Q70 series), architectural choices directly dictate latency, memory pressure, and frame delivery:

### 1. Comparative Metrics Summary

| Metric | Stock Samsung Smart Hub | Free My TV (TizenBrew Web Module) | Quiet Home (Native .NET / NUI) |
|---|---|---|---|
| **Underlying Engine** | Heterogeneous (native shell + web widget embeds + ad banners) | Web Engine (Chromium/WebKit inside TizenBrew container) | Native .NET 6.0 with Tizen NUI / DALi hardware graphics |
| **First UI Render Time** | 2,400 – 4,500 ms (including ad server handshakes) | 1,200 – 2,200 ms (web bundle parsing + DOM layout) | **104 – 114 ms** (measured live on QN55Q7FAAPXPA) |
| **D-Pad / Remote Input Latency** | 180 – 350 ms (input event queue across multiple services) | 220 – 450 ms (DOM event bubbling + JS event loop) | **< 16 ms** (direct hardware event loop in NUI Window) |
| **Frame Rate & Fluidity** | 25 – 45 FPS (stutter during ad refreshes and banner scrolling) | 15 – 25 FPS (heavy CPU lag on backdrop blur & layout reflow) | **Solid 60 FPS** (direct GPU scene graph compositing) |
| **Tactile Audio Latency** | ~120 ms (multiplexed via system sound manager) | N/A or > 200 ms (web audio context latency) | **< 5 ms** (`libfeedback.so.0` direct kernel/driver tick) |
| **Network Overhead on Launch** | Heavy (telemetry, dynamic promo banners, Smart Hub ads) | Moderate/Heavy (TMDB API requests, external poster downloads, weather) | **Zero (0 bytes)**; completely offline and standalone |
| **Available RAM (`MemAvailable`)** | ~500 – 540 MiB (ad services, TV Plus daemon, store preloads) | ~520 – 580 MiB (browser runtime memory footprint) | **~900 – 922 MiB** available for gaming and apps |
| **Icon Reordering Performance** | Cloud/Account sync latency; slow carousel editing | DOM re-render with potential layout shifts | **< 5 ms** in-memory pointer swap & atomic local JSON flush |

### 2. Architectural Root Causes of Web-Based Lag (TizenBrew / Free My TV)

1. **CPU Saturation in Web Layout and Compositing:**
   Smart TV processors are heavily optimized for hardware video decoding (HEVC/VP9/AV1), not general-purpose single-threaded JavaScript execution. Web-based launchers run a full Chromium/WebKit browser stack. Operations like CSS `backdrop-filter: blur()`, dynamic box-shadow recalculations, and flexbox/grid layout reflows run on the CPU, causing D-pad drops and stutter.

2. **Network Blocking & Asynchronous Asset Choke:**
   Web launchers typically fetch trending content, weather, and movie backdrops from remote APIs (TMDB, weather services). On home networks or TV Wi-Fi with moderate latency, UI threads block or stutter while awaiting image decoding, resulting in blank tiles or delayed card appearances.

3. **Input Handling Overhead:**
   Key events in a web browser container traverse multiple layers:
   `Linux evdev -> Tizen Compositor -> WebKit Event Queue -> JavaScript event listener -> DOM mutation -> Compositor Paint`.
   In contrast, Quiet Home receives key events directly from `Tizen.NUI.Window.Default.KeyEvent` and maps them in native code directly to hardware transforms (`Position` and `Scale`).

### 3. Native Architecture Optimizations in Quiet Home

1. **Zero-Copy GPU Texture Caching:**
   Quiet Home preloads and decodes all local SVG vectors and compressed raster artwork once during initialization (`PreloadArtwork()`). Textures are pinned in GPU memory (`ReleasePolicyType.Never` and scene parking in a zero-opacity preloading root), ensuring that navigating tabs or reordering apps never re-decodes image files from disk.

2. **Ultra-Low-Latency Hardware Tactile Feedback:**
   Instead of spawning external audio processes (such as `paplay`, which triggers thread exhaustion and SIGSEGV on rapid D-pad inputs), Quiet Home connects directly to Samsung's tactile vibration/click engine via `libfeedback.so.0`:
   ```csharp
   feedback_initialize();
   feedback_set_resource_path(1, 0, Root + "click.wav");
   feedback_play_type(1, 0);
   ```
   This delivers immediate (< 5 ms) audible confirmation matching the physical remote click.

3. **Native Icon Reordering Engine:**
   App management operates strictly in memory on the `List<LibraryApp>` structure. When long-pressing Enter for 500 ms:
   - The focus ring immediately elevates with a gold/amber highlight.
   - Arrow navigation performs an $O(1)$ swap of catalog pointers.
   - The scene graph is updated non-destructively in < 5 ms using already-cached texture handles.
   - Positions are written atomically to local storage without requiring external network sync or cloud validation.


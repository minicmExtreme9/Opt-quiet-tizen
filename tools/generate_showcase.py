import os, base64, json
from pathlib import Path

def get_b64(path):
    if not os.path.exists(path):
        return ''
    ext = path.split('.')[-1].lower()
    mime = 'image/svg+xml' if ext == 'svg' else 'image/png' if ext == 'png' else 'image/jpeg'
    with open(path, 'rb') as f:
        return f'data:{mime};base64,' + base64.b64encode(f.read()).decode()

ROOT = Path('C:/Users/Jorge/quiet-tizen')
ps4_b64 = get_b64(str(ROOT / 'launcher/assets/feature-ps4.png'))
switch_b64 = get_b64(str(ROOT / 'launcher/assets/feature-switch.png'))
hdmi3_b64 = get_b64(str(ROOT / 'launcher/assets/feature-hdmi.svg'))

dock_items = [
    ('Netflix', get_b64(str(ROOT / 'launcher/assets/free-dock-netflix.svg'))),
    ('YouTube', get_b64(str(ROOT / 'launcher/assets/free-dock-youtube.svg'))),
    ('Crunchyroll', get_b64(str(ROOT / 'launcher/assets/free-dock-crunchyroll.svg'))),
    ('HBO Max', get_b64(str(ROOT / 'launcher/assets/free-dock-hbomax.png'))),
    ('Disney+', get_b64(str(ROOT / 'launcher/assets/free-dock-disney.svg'))),
    ('Apple Music', get_b64(str(ROOT / 'launcher/assets/free-dock-apple-music.png'))),
]

with open(ROOT / 'launcher/assets/apps.json', 'r', encoding='utf-8') as f:
    apps_data = json.load(f)

apps_list = []
for a in apps_data[:15]:
    asset = a.get('asset')
    b64_val = ''
    if asset:
        p = ROOT / 'launcher/assets' / asset
        if p.exists():
            b64_val = get_b64(str(p))
    apps_list.append({
        'id': a.get('id', ''),
        'name': a.get('name', ''),
        'bg': a.get('background', '#1c2237'),
        'b64': b64_val
    })

html_content = f'''<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="utf-8">
<title>Quiet Home TV - Interactive Showcase & Simulation</title>
<script src="https://www.gstatic.com/antigravity/web/dev/tailwindcss.min.js"></script>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');
  * {{ box-sizing: border-box; font-family: 'Plus Jakarta Sans', sans-serif; }}
  body {{ margin: 0; background: #090a10; color: #fff; overflow-x: hidden; }}
  
  /* TV Screen Container */
  .tv-frame {{
    width: 100%;
    max-width: 1200px;
    aspect-ratio: 16 / 9;
    border-radius: 20px;
    position: relative;
    overflow: hidden;
    box-shadow: 0 25px 60px -15px rgba(0, 0, 0, 0.9), 0 0 0 1px rgba(255, 255, 255, 0.1);
    transition: background 0.4s ease;
  }}

  .theme-dark {{
    background: radial-gradient(circle at 15% 15%, #18203c 0%, #0d101d 50%, #06070c 100%);
    --text-primary: #ffffff;
    --text-muted: #8e9bb5;
    --card-bg: rgba(255, 255, 255, 0.05);
    --dock-bg: rgba(20, 24, 40, 0.65);
    --focus-ring: #ffffff;
  }}

  .theme-light {{
    background: radial-gradient(circle at 15% 15%, #e2e8f6 0%, #cbd8f0 50%, #b8c7e6 100%);
    --text-primary: #121826;
    --text-muted: #4b5874;
    --card-bg: rgba(255, 255, 255, 0.7);
    --dock-bg: rgba(255, 255, 255, 0.55);
    --focus-ring: #1a56db;
  }}

  .focus-active {{
    transform: scale(1.035);
    box-shadow: 0 0 0 4px var(--focus-ring), 0 18px 36px -10px rgba(0,0,0,0.5);
    z-index: 20;
  }}

  .smooth-trans {{
    transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  }}
</style>
</head>
<body class="p-6 flex flex-col items-center min-h-screen justify-center">

  <!-- Header Bar -->
  <div class="w-full max-w-[1200px] mb-4 flex items-center justify-between">
    <div class="flex items-center gap-3">
      <div class="w-3.5 h-3.5 rounded-full bg-emerald-500 animate-pulse"></div>
      <h1 class="text-xl font-bold tracking-tight">Quiet Home Tizen · Showcase Interactivo</h1>
      <span class="text-xs px-2.5 py-0.5 rounded-full bg-blue-500/20 text-blue-400 font-semibold border border-blue-500/30">Samsung QN55Q7F</span>
    </div>
    <div class="flex items-center gap-2">
      <button onclick="toggleTheme()" class="px-3.5 py-1.5 rounded-lg bg-white/10 hover:bg-white/20 text-xs font-semibold backdrop-blur border border-white/10 transition">
        🌓 Alternar Tema (<span id="theme-label">Oscuro</span>)
      </button>
      <button onclick="autoTour()" id="tour-btn" class="px-3.5 py-1.5 rounded-lg bg-blue-600 hover:bg-blue-500 text-xs font-semibold shadow-lg shadow-blue-600/30 transition flex items-center gap-1.5">
        ▶ Showcase Automático
      </button>
    </div>
  </div>

  <!-- TV Frame Mockup -->
  <div id="tv-viewport" class="tv-frame theme-dark p-10 flex flex-col justify-between">
    
    <!-- Top Bar -->
    <div class="flex items-center justify-between z-10">
      <div>
        <h2 id="page-title" class="text-3xl font-extrabold tracking-tight text-[var(--text-primary)] transition-all duration-300">Home</h2>
        <div class="flex gap-6 mt-3">
          <button onclick="setPage(0)" id="tab-0" class="tab-btn text-sm font-semibold transition pb-1 border-b-2 border-blue-500 text-[var(--text-primary)]">Inputs</button>
          <button onclick="setPage(1)" id="tab-1" class="tab-btn text-sm font-semibold transition pb-1 border-b-2 border-transparent text-[var(--text-muted)] hover:text-[var(--text-primary)]">Apps</button>
          <button onclick="setPage(2)" id="tab-2" class="tab-btn text-sm font-semibold transition pb-1 border-b-2 border-transparent text-[var(--text-muted)] hover:text-[var(--text-primary)]">Quick Settings</button>
        </div>
      </div>
      <div class="flex items-center gap-4 text-xs font-semibold text-[var(--text-muted)]">
        <span class="flex items-center gap-1.5"><span>📶</span> Wi-Fi</span>
        <span>18:25</span>
      </div>
    </div>

    <!-- PAGE 0: INPUTS -->
    <div id="page-0-content" class="page-content flex flex-col justify-between flex-1 mt-6">
      <!-- Feature Cards Row -->
      <div class="flex gap-5 items-stretch h-[240px]">
        <!-- PS4 Hero Card (1040x442 aspect) -->
        <div id="card-0" onclick="focusElement(0)" class="nav-item smooth-trans relative flex-1 rounded-2xl overflow-hidden cursor-pointer focus-active shadow-xl">
          <img src="{ps4_b64}" class="w-full h-full object-cover" />
          <div class="absolute inset-0 p-6 flex flex-col justify-between bg-gradient-to-r from-blue-950/70 via-transparent to-transparent">
            <div>
              <span class="text-[11px] font-bold tracking-wider text-blue-200 uppercase">PLAYSTATION 4 / PC</span>
              <h3 class="text-2xl font-black tracking-tight text-white mt-1">Let's play.</h3>
            </div>
            <span class="text-xs font-semibold text-blue-100 flex items-center gap-1">Switch to HDMI 1 →</span>
          </div>
        </div>

        <!-- Switch Card (320x442 aspect) -->
        <div id="card-1" onclick="focusElement(1)" class="nav-item smooth-trans relative w-[170px] rounded-2xl overflow-hidden cursor-pointer shadow-xl">
          <img src="{switch_b64}" class="w-full h-full object-cover" />
          <div class="absolute bottom-0 inset-x-0 p-3 bg-gradient-to-t from-black/80 to-transparent">
            <span class="text-[10px] font-bold text-yellow-300 block">HDMI 2</span>
            <span class="text-xs font-bold text-white block truncate">Nintendo Switch</span>
          </div>
        </div>

        <!-- HDMI 3 Card (320x442 aspect) -->
        <div id="card-2" onclick="focusElement(2)" class="nav-item smooth-trans relative w-[170px] rounded-2xl overflow-hidden cursor-pointer shadow-xl">
          <img src="{hdmi3_b64}" class="w-full h-full object-cover" />
          <div class="absolute bottom-0 inset-x-0 p-3 bg-gradient-to-t from-black/80 to-transparent">
            <span class="text-[10px] font-bold text-emerald-300 block">HDMI 3</span>
            <span class="text-xs font-bold text-white block truncate">External device</span>
          </div>
        </div>
      </div>

      <!-- Dock Row -->
      <div class="p-3 rounded-2xl backdrop-blur-xl border border-white/10 flex gap-4 items-center justify-between shadow-2xl bg-[var(--dock-bg)] mt-4">
        {"".join([f'''
        <div id="card-{3+i}" onclick="focusElement({3+i})" class="nav-item smooth-trans flex-1 h-[68px] rounded-xl overflow-hidden cursor-pointer flex items-center justify-center p-2 bg-white/5 hover:bg-white/10">
          <img src="{item[1]}" class="max-h-full max-w-full object-contain filter drop-shadow" />
        </div>''' for i, item in enumerate(dock_items)])}
      </div>
    </div>

    <!-- PAGE 1: APPS (Clean, no text tags as requested) -->
    <div id="page-1-content" class="page-content hidden flex-1 mt-6">
      <div class="grid grid-cols-5 gap-4 h-full">
        {"".join([f'''
        <div class="smooth-trans rounded-2xl p-4 flex items-center justify-center cursor-pointer shadow-md hover:scale-105 border border-white/5" style="background: {app['bg']}">
          {f'<img src="{app["b64"]}" class="max-h-16 max-w-full object-contain filter drop-shadow" />' if app['b64'] else f'<span class="text-sm font-bold text-white">{app["name"]}</span>'}
        </div>''' for app in apps_list])}
      </div>
    </div>

    <!-- PAGE 2: QUICK SETTINGS -->
    <div id="page-2-content" class="page-content hidden flex-1 mt-6">
      <div class="grid grid-cols-4 gap-4 h-full">
        <div class="p-5 rounded-2xl bg-white/5 border border-white/10 flex flex-col justify-between">
          <span class="text-2xl">🔊</span>
          <div>
            <h4 class="font-bold text-white">Volumen</h4>
            <p class="text-xs text-[var(--text-muted)] mt-1">Nivel actual: 12</p>
          </div>
        </div>
        <div class="p-5 rounded-2xl bg-white/5 border border-white/10 flex flex-col justify-between">
          <span class="text-2xl">☀️</span>
          <div>
            <h4 class="font-bold text-white">Brillo</h4>
            <p class="text-xs text-[var(--text-muted)] mt-1">Panel: 45 / 50</p>
          </div>
        </div>
        <div class="p-5 rounded-2xl bg-white/5 border border-white/10 flex flex-col justify-between">
          <span class="text-2xl">🎮</span>
          <div>
            <h4 class="font-bold text-white">Modo Juego</h4>
            <p class="text-xs text-emerald-400 mt-1">Activado (Baja Latencia)</p>
          </div>
        </div>
        <div class="p-5 rounded-2xl bg-white/5 border border-white/10 flex flex-col justify-between">
          <span class="text-2xl">⏱️</span>
          <div>
            <h4 class="font-bold text-white">Temporizador</h4>
            <p class="text-xs text-[var(--text-muted)] mt-1">Apagado automático</p>
          </div>
        </div>
        <div class="p-5 rounded-2xl bg-white/5 border border-white/10 flex flex-col justify-between">
          <span class="text-2xl">🔇</span>
          <div>
            <h4 class="font-bold text-white">Silencio</h4>
            <p class="text-xs text-[var(--text-muted)] mt-1">Desactivado</p>
          </div>
        </div>
        <div class="p-5 rounded-2xl bg-white/5 border border-white/10 flex flex-col justify-between">
          <span class="text-2xl">🎮</span>
          <div>
            <h4 class="font-bold text-white">Controles</h4>
            <p class="text-xs text-[var(--text-muted)] mt-1">DualShock 4 emparejado</p>
          </div>
        </div>
        <div class="p-5 rounded-2xl bg-white/5 border border-white/10 flex flex-col justify-between">
          <span class="text-2xl">🌐</span>
          <div>
            <h4 class="font-bold text-white">Red</h4>
            <p class="text-xs text-blue-400 mt-1">5 GHz Conectado</p>
          </div>
        </div>
        <div class="p-5 rounded-2xl bg-white/5 border border-white/10 flex flex-col justify-between">
          <span class="text-2xl">⚙️</span>
          <div>
            <h4 class="font-bold text-white">Configuración TV</h4>
            <p class="text-xs text-[var(--text-muted)] mt-1">Menú nativo Samsung</p>
          </div>
        </div>
      </div>
    </div>

  </div>

  <!-- Interactive Remote Control Bar -->
  <div class="mt-6 flex items-center gap-4 bg-white/5 px-6 py-3 rounded-2xl border border-white/10 backdrop-blur">
    <span class="text-xs font-semibold text-zinc-400">Control / Teclado:</span>
    <kbd class="px-2 py-1 rounded bg-zinc-800 text-xs font-mono text-zinc-300 border border-zinc-700">←</kbd>
    <kbd class="px-2 py-1 rounded bg-zinc-800 text-xs font-mono text-zinc-300 border border-zinc-700">→</kbd>
    <span class="text-xs text-zinc-400">Navegar tarjetas</span>
    <span class="text-zinc-600">|</span>
    <kbd class="px-2 py-1 rounded bg-zinc-800 text-xs font-mono text-zinc-300 border border-zinc-700">1</kbd>
    <kbd class="px-2 py-1 rounded bg-zinc-800 text-xs font-mono text-zinc-300 border border-zinc-700">2</kbd>
    <kbd class="px-2 py-1 rounded bg-zinc-800 text-xs font-mono text-zinc-300 border border-zinc-700">3</kbd>
    <span class="text-xs text-zinc-400">Cambiar pestañas</span>
  </div>

<script>
  let currentPage = 0;
  let focusedIndex = 0;
  let isDark = true;
  let tourInterval = null;

  function toggleTheme() {{
    isDark = !isDark;
    const vp = document.getElementById('tv-viewport');
    vp.classList.toggle('theme-dark', isDark);
    vp.classList.toggle('theme-light', !isDark);
    document.getElementById('theme-label').innerText = isDark ? 'Oscuro' : 'Claro';
  }}

  function setPage(n) {{
    currentPage = n;
    const titles = ['Home', 'Your Apps', 'Quick Settings'];
    document.getElementById('page-title').innerText = titles[n];
    for (let i = 0; i < 3; i++) {{
      const tab = document.getElementById('tab-' + i);
      const content = document.getElementById('page-' + i + '-content');
      if (i === n) {{
        tab.classList.add('border-blue-500', 'text-[var(--text-primary)]');
        tab.classList.remove('border-transparent', 'text-[var(--text-muted)]');
        content.classList.remove('hidden');
      }} else {{
        tab.classList.remove('border-blue-500', 'text-[var(--text-primary)]');
        tab.classList.add('border-transparent', 'text-[var(--text-muted)]');
        content.classList.add('hidden');
      }}
    }}
  }}

  function focusElement(idx) {{
    focusedIndex = idx;
    document.querySelectorAll('.nav-item').forEach((el, i) => {{
      if (i === idx) el.classList.add('focus-active');
      else el.classList.remove('focus-active');
    }});
  }}

  window.addEventListener('keydown', (e) => {{
    if (e.key === 'ArrowRight') {{
      focusedIndex = (focusedIndex + 1) % 9;
      focusElement(focusedIndex);
    }} else if (e.key === 'ArrowLeft') {{
      focusedIndex = (focusedIndex - 1 + 9) % 9;
      focusElement(focusedIndex);
    }} else if (e.key === '1') setPage(0);
    else if (e.key === '2') setPage(1);
    else if (e.key === '3') setPage(2);
  }});

  function autoTour() {{
    const btn = document.getElementById('tour-btn');
    if (tourInterval) {{
      clearInterval(tourInterval);
      tourInterval = null;
      btn.innerText = '▶ Showcase Automático';
      btn.classList.remove('bg-rose-600');
      btn.classList.add('bg-blue-600');
      return;
    }}
    btn.innerText = '⏹ Detener Showcase';
    btn.classList.remove('bg-blue-600');
    btn.classList.add('bg-rose-600');
    let step = 0;
    tourInterval = setInterval(() => {{
      step++;
      if (step % 12 === 0) setPage(0);
      else if (step % 12 === 4) setPage(1);
      else if (step % 12 === 8) setPage(2);
      
      if (currentPage === 0) {{
        focusElement((step) % 9);
      }}
    }}, 1200);
  }}
</script>
</body>
</html>
'''

out_path = Path('C:/Users/Jorge/.gemini/antigravity/brain/f4cfd31d-6467-4b3f-994f-17774cf4a50b/quiet-home-showcase.html')
out_path.write_text(html_content, encoding='utf-8')
print('Showcase written to:', out_path)

downloads_path = Path('C:/Users/Jorge/Downloads/quiet-home-showcase.html')
downloads_path.write_text(html_content, encoding='utf-8')
print('Showcase copied to:', downloads_path)

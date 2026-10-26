# CHANGELOG - Refactor Launcher Portable

Refactorización incremental y segura del launcher Opt-quiet-tizen (.NET 6 Tizen NUI).
Cada paso es probado en hardware real antes de avanzar al siguiente.

---

## PASO 1 — Persistencia de configuración (SettingsStore)
- **Archivos creados / modificados**:
  - `launcher/src/SettingsStore.cs`: Nueva clase y modelo tipado `LauncherSettings` con serialización moderna `System.Text.Json`.
  - `launcher/src/Home.cs`: Extracción de `LoadConfig`, `SaveConfig`, `ExtractJsonValue`, `EscapeJson`. Reemplazado el parseo manual por llamadas a `SettingsStore.Load` y `SettingsStore.Save`.
  - `launcher/src/Home.csproj`: Inclusión de `SettingsStore.cs` en los ítems de compilación.
- **Migración de legado**:
  - `MigrateLegacyJson()` detecta formatos antiguos o valores textuales existentes en `/tmp/quiet-home-config.json` y los convierte al nuevo esquema tipado reescribiendo el archivo sin pérdida de configuración.
- **Validación previa**:
  - Compilación .NET 6 limpia (0 errores, 0 advertencias).
  - Tests unitarios verificados: Guardado y carga tipada, deserialización, migración de legado y reescritura de formato.
  - Paquetes generados: `QuietHome-TV-1.0.0.tpk` y `Opt-quiet-tizen-v1.0.1.zip` en Downloads.
- **Resultado en TV del dueño (Punto de control 1)**:
  - Verificado y confirmado como funcional en Samsung Smart TV real (QLED). La configuración persiste correctamente tras editar preferencias y reiniciar la aplicación.

---

## PASO 2 — Internacionalización (Localization)
- **Archivos creados / modificados**:
  - `launcher/src/Localization.cs`: Nueva clase estática con diccionario anidado `Dictionary<string, Dictionary<string, string>>` por clave e idioma en vez de arrays posicionales frágiles. Funciones centralizadas `T(key, lang)`, `GetCulture(code)` y `FormatDate(dt, code)`.
  - `launcher/src/Home.cs`: Eliminado el array monolítico `I18N` y métodos redundantes de fecha/cultura; reemplazados por llamadas directas a `Localization`.
  - `launcher/src/Home.csproj`: Inclusión de `Localization.cs` en los ítems de compilación.
- **Validación previa**:
  - Tests unitarios verificados: Búsqueda exacta en los 10 idiomas ("es", "en", "de", "pt", "fr", "zh", "ja", "ko", "hi", "ar"), manejo de fallbacks ante claves o códigos desconocidos y formateo de fechas.
  - Compilación .NET 6 limpia (0 errores, 0 advertencias).
  - Paquetes actualizados: `QuietHome-TV-1.0.0.tpk` y `Opt-quiet-tizen-v1.0.1.zip` en Downloads.
- **Resultado en TV del dueño (Punto de control 2)**:
  - Verificado y confirmado como funcional en Samsung Smart TV real (QLED). Cambio de idiomas fluido, todas las cadenas coinciden exactamente con el idioma seleccionado sin textos cruzados.


---

## PASO 3 — Capacidades nativas con detección en runtime (NativeCapabilities)
- **Archivos creados / modificados**:
  - `launcher/src/NativeCapabilities.cs`: Nueva clase estática que encapsula todas las 18 firmas `DllImport` del sistema Tizen (`libcapi-media-sound-manager-tv.so.0`, `libsettings-api.so`, `libcapi-appfw-app-control.so.0`, `libvconf.so.0`, `libmmfkeysound.so.0`, `libc.so.6`, `libsource-api.so`). Provee detección segura en runtime (`Initialize()`) probando cada subsistema con manejo de excepciones y cacheando flags de capacidad (`HasSoundManager`, `HasSettingsApi`, `HasAppControl`, `HasVconf`, `HasKeySound`, `HasVolume`, `HasBrightness`, etc.). Ofrece métodos wrapper de alto nivel que evitan exponer `IntPtr` o llamadas desprotegidas al launcher.
  - `launcher/src/Home.cs`: Eliminados todos los `DllImport`s directos. Integración con `NativeCapabilities.Initialize()` al arrancar. La cuadrícula de Ajustes Rápidos (Página 2) ahora se construye dinámicamente según las capacidades detectadas, omitiendo controles no disponibles en lugar de fallar al seleccionarlos. La navegación por teclado (Arriba/Abajo) y la actualización de textos fueron protegidas contra desbordamientos de índice ante controles dinámicos.
  - `launcher/src/Home.csproj`: Inclusión de `NativeCapabilities.cs` en los ítems de compilación.
- **Validación previa**:
  - Compilación .NET 6 limpia (0 errores, 0 advertencias).
  - Tests de sondeo ejecutados: verificado el comportamiento de fallback seguro en entornos donde las bibliotecas nativas de Tizen no están presentes.
  - Paquetes actualizados: `QuietHome-TV-1.0.0.tpk` y `Opt-quiet-tizen-v1.0.1.zip` en Downloads.
- **Resultado en TV del dueño (Punto de control 3)**:
  - Verificado y confirmado como funcional en Samsung Smart TV real (QLED). Todos los controles respondieron correctamente (volumen, brillo, modo juego, temporizador de apagado y cambio de fuente HDMI).


---

## PASO 4 — Eliminar los catch vacíos (DebugLog & Ring Buffer)
- **Archivos creados / modificados**:
  - `launcher/src/DebugLog.cs`: Nueva clase y estructura `LogEntry` que implementa un ring buffer circular de 50 entradas en memoria. Soporta niveles (`INFO`, `WARN`, `ERROR`, `STATE`), categorías, seguimiento de excepciones y evento de notificación `OnErrorLogged`. Permite volcado manual a `/tmp/quiet-home-debuglog.txt` sin realizar escrituras continuas en disco.
  - `launcher/src/Home.cs`: Reemplazados todos los bloques `catch{}` vacíos por llamadas a `DebugLog.Warn` y `DebugLog.Error`. Conectado el evento `DebugLog.OnErrorLogged` con `ShowError` para degradación visual inmediata en la UI ante errores. Reemplazado `WriteState(activity)` (que antes escribía a `/tmp/quiet-home-state.txt` en cada pulsación del mando) por `DebugLog.RecordState`, eliminando el desgaste de memoria flash. Añadido visor en pantalla `ToggleDebugLogView()` accesible mediante las teclas `Info`, `d`, o `0`.
  - `launcher/src/NativeCapabilities.cs`: Actualizado el manejo de excepciones de todos los sondeos y llamadas P/Invoke para registrar advertencias y errores en `DebugLog`.
  - `launcher/src/Home.csproj`: Inclusión de `DebugLog.cs` en los ítems de compilación.
- **Validación previa**:
  - Compilación .NET 6 limpia (0 errores, 0 advertencias).
  - Tests unitarios en `tests/DebugLogTest.cs`: verificado desborde circular a 50 entradas, preservación FIFO de los registros más recientes, conteo de errores y advertencias, y volcado a archivo.
  - Paquetes actualizados: `QuietHome-TV-1.0.0.tpk` y `Opt-quiet-tizen-v1.0.1.zip` en Downloads.
- **Resultado en TV del dueño (Punto de control 4)**:
  - Verificado y confirmado como funcional en Samsung Smart TV real (QLED). El visor del Debug Log abrió perfectamente en pantalla mostrando "0 errores, 0 avisos". Se comprobó además que la tarjeta no funcional de Modo Juego fue ocultada limpiamente y que "Vincular mando" abrió directamente el emparejamiento Bluetooth.

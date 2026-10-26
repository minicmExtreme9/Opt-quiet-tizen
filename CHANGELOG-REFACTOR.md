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


---

## PASO 5 — Layout dinámico por resolución
- **Archivos creados / modificados**:
  - `launcher/src/Home.cs`: Reemplazadas todas las dimensiones fijas y coordenadas absolutas por propiedades dinámicas calculadas a partir de `Window.Default.WindowSize` (`ScreenWidth`, `ScreenHeight`, `ScaleX`, `ScaleY`, `ScaleFont`, `SclX`, `SclY`, `SclF`). Aplicado escalado en fondos, contenedores animados, tarjetas hero/portrait de HDMI, estante y dock de apps, cuadrícula de biblioteca (Página 1), tarjetas de configuración rápida (Página 2), botones de la barra de herramientas y textos del pie de página.
- **Validación previa**:
  - Compilación .NET 6 limpia (0 errores, 0 advertencias).
  - Paquetes actualizados: `QuietHome-TV-1.0.0.tpk` y `Opt-quiet-tizen-v1.0.1.zip` en Downloads.
  - Desplegado y lanzado en el televisor (PID: 16257).
- **Resultado en TV del dueño (Punto de control 5)**:
  - Verificado y confirmado en Samsung Smart TV real (QLED). El layout dinámico escala y se visualiza correctamente en la pantalla sin desalineaciones ni recortes.
  - Se restauró la visibilidad de la tarjeta de "Modo Juego" en la fila de Ajustes Rápidos (`HasGameMode => HasSettingsApi`) a petición del dueño, reempaquetado y desplegado (PID: 17669).


---

## PASO 6 — Build separado para variante "sin privilegios" (Lite)
- **Archivos creados / modificados**:
  - `launcher/src/NativeCapabilities.cs`: Introducida directiva condicional `UNPRIVILEGED_LITE` que desactiva y excluye totalmente las llamadas P/Invoke a librerías propietarias y privilegiadas de Samsung (`libsettings-api.so`, `libcapi-media-sound-manager-tv.so.0`, `libvconf.so.0`, `libmmfkeysound.so.0`, `libsource-api.so`, y `libc.so.6: system`). Expone `IsLiteVariant`. Únicamente mantiene activa la API pública de Tizen `libcapi-appfw-app-control.so.0` para lanzamiento de aplicaciones y cambio de fuentes HDMI.
  - `launcher/src/Home.csproj`: Añadida condición de compilación `<DefineConstants Condition="'$(VARIANT)' == 'Lite'">$(DefineConstants);UNPRIVILEGED_LITE</DefineConstants>`.
  - `launcher/src/Home.cs`: Adaptada la página de Ajustes Rápidos para reorganizar las opciones disponibles en la Fila 0 cuando los ajustes de volumen/brillo/pantalla no están presentes. Añadida indicación visual `(Lite)` en la cabecera. Protección contra división por cero en navegación D-Pad si alguna fila está vacía. Corrección del ciclo de vida (`OnAppControlReceived`, `OnResume`, `OnPause`): protección contra `NullReferenceException` cuando `ExtraData` es nulo al reanudar desde apps externas del mando físico, re-elevación de foco con `Window.Default.Raise()`, reseteo de `area = 1` y limpieza de flags de pulsación larga (`waitingForLongPress`, `longPressTriggered`) para evitar bloqueos del D-Pad y tecla Return.
  - `tools/build.py`: Incorporado argumento `--variant [full|lite]` para compilar de forma aislada en `.local/build-lite/` y generar el paquete intermedio en `.local/package-lite/`.
  - `tools/make_lite_tpk.py`: Nuevo empaquetador que genera `opt-quiet-tizen-lite.tpk` (Package/App ID: `org.quiethome.lite`, con privilegios estándar exclusivamente) en la carpeta Downloads sin sobreescribir el TPK completo (`QuietHome-TV-1.0.0.tpk`).
- **Funciones omitidas / limitadas en la versión Lite**:
  - *Omitidas*: Ajuste directo de volumen y mute maestro (requieren `libcapi-media-sound-manager-tv.so.0`).
  - *Omitidas*: Ajuste de brillo de imagen, alternador de Modo Juego y temporizador de apagado (requieren `libsettings-api.so`).
  - *Omitidas*: Efectos de sonido de teclas nativos (`libmmfkeysound.so.0`) y comandos de sistema (`system()`).
  - *Disponibles y funcionales*: Cambio de entradas HDMI (`org.tizen.tv-viewer` + `change_source` vía AppControl público), apertura de todas las aplicaciones del Dock y Biblioteca, acceso a Ajustes de Bluetooth/Mandos y Redes vía AppControl, navegación completa de la interfaz con escalado dinámico, temas claro/oscuro y visor de diagnóstico interno en memoria.
- **Validación previa**:
  - Compilación .NET 6 limpia tanto en variante Full como en variante Lite (0 errores, 0 advertencias).
  - Empaquetado independiente generado exitosamente: `C:\Users\Jorge\Downloads\opt-quiet-tizen-lite.tpk` (3,293,525 bytes) coexistiendo con `QuietHome-TV-1.0.0.tpk` (3,294,637 bytes).
- **Resultado en TV del dueño (Punto de control 6)**:
  - Verificado y confirmado en Samsung Smart TV real (QLED). La variante Lite responde fluidamente sin errores (0 errores, 0 avisos en el visor de diagnóstico).
  - Confirmado el cambio de fuentes HDMI (`source_switch` exitoso mediante `app_control` público), apertura y retorno rápido de apps sin bloqueos ni congelamientos, reorganización limpia de los Ajustes Rápidos disponibles y coexistencia independiente del paquete `opt-quiet-tizen-lite.tpk`.


---

## AUDITORÍA Y SIMPLIFICACIÓN — Eliminación de redundancia de triple escritura (Persistencia única en /opt/usr/home/owner/data/)
- **Análisis técnico y evidencia de rutas**:
  - `Root + "apps.json"` (`/opt/usr/apps/com.samsung.tv.CinemaGraphApp/res/quiet-home/apps.json`): Pertenece a UID `201:root`, permisos `644`, etiqueta SMACK `User::Pkg::com.samsung.tv.CinemaGraphApp::RO`. El launcher corre como UID `5001` (`owner`), por lo que cualquier intento de escritura en runtime siempre falla arrojando `UnauthorizedAccessException`. Es estrictamente un recurso semilla (seed) de solo lectura instalado con el paquete.
  - `/tmp/quiet-home-apps.json`: Montaje `tmpfs` en RAM volátil. Se borra tras un reinicio o ciclo de energía del televisor, por lo que no aporta persistencia real entre sesiones y constituía un remanente innecesario.
  - `/opt/usr/home/owner/data/quiet-home-apps.json`: Directorio estándar de datos de usuario en Tizen OS (`5001:users`, permisos `777` en flash persistente). Sobrevive a reinicios del televisor, suspensión y actualizaciones de binarios.
- **Decisión y cambios aplicados**:
  - **Eliminación de la triple escritura**: Se suprimieron las escrituras redundantes a `/tmp/quiet-home-apps.json` y al bundle local.
  - **Fuente única de verdad**: `SaveLibrary()` en `Home.cs` escribe únicamente a `/opt/usr/home/owner/data/quiet-home-apps.json`.
  - **Carga limpia en startup**: Al iniciar, `Home.cs` comprueba si existe `/opt/usr/home/owner/data/quiet-home-apps.json` y lo carga. Si no existe (primera ejecución tras instalación limpia), carga el catálogo semilla por defecto desde `Root + "apps.json"`.
  - **Alineación con SettingsStore**: `SettingsStore.cs` y la persistencia de configuración general (`quiet-home-config.json`) fueron unificados bajo el mismo esquema de fuente única en `/opt/usr/home/owner/data/quiet-home-config.json` con plantilla fallback de solo lectura en el bundle, eliminando también escrituras paralelas a `/tmp`.

---

## PERSISTENCIA SEGURA ANTE CORTE DE ENERGÍA — AtomicFileWriter (fsync explícito + rename atómico)
- **Problema identificado en hardware**:
  - `File.WriteAllText()` solo confirma datos en el *page cache* de Linux en RAM. Tras un soft-reboot, el kernel tiene tiempo de volcar las páginas sucias a flash; sin embargo, ante una desconexión abrupta del cable de alimentación (corte de energía real), las escrituras pendientes en RAM se pierden por completo.
- **Implementación (`launcher/src/AtomicFileWriter.cs`)**:
  - **Mecanismo de fsync**: Se combinaron dos niveles de garantía:
    1. A nivel de .NET 6: Apertura del stream con `FileOptions.WriteThrough` y llamada explícita a `FileStream.Flush(flushToDisk: true)` (que en el runtime de Unix invoca internamente `fsync` / `fdatasync` sobre el descriptor de archivo).
    2. A nivel de SO / libc: P/Invoke directo a `libc.so.6: fsync(int fd)` sobre el `SafeFileHandle` para forzar la sincronización física del bloque de datos en flash antes de cerrar el descriptor.
  - **Reemplazo atómico (`File.Move`)**: El contenido se escribe en `path.tmp` en el mismo directorio (`/opt/usr/home/owner/data/`), garantizando que la operación se realice dentro del mismo sistema de archivos. Una vez confirmado el `fsync`, se ejecuta `File.Move(tmpPath, path, overwrite: true)` (llamada a `rename(2)` de POSIX), lo que garantiza que nunca exista un archivo corrupto o a medio escribir.
  - **Sincronización de metadatos de directorio**: P/Invoke directo a `libc.so.6: sync()` inmediatamente después de `File.Move` para asegurar que la nueva entrada de directorio y el inodo queden grabados en el diario de la memoria flash ante un apagón instantáneo.
  - **Protección contra temporales huérfanos**: `AtomicFileWriter.CleanOrphanTmp(path)` verifica en `AppLibrary.Load()` y `SettingsStore.Load()` si quedó algún `.tmp` huérfano producto de un corte de energía a mitad del proceso de guardado y lo limpia de forma segura sin degradar la lectura del archivo definitivo.
  - **Adopción completa**:
    - `AppLibrary.Save()` utiliza `AtomicFileWriter.WriteAllTextSafe()` para `quiet-home-apps.json`.
    - `SettingsStore.Save()` utiliza `AtomicFileWriter.WriteAllTextSafe()` para `quiet-home-config.json`.
    - Incluido en `Home.csproj` y verificado en compilaciones Full y Lite (0 errores, 0 avisos).
- **Verificación técnica de robustez**:
  - *Comportamiento ante `.tmp` huérfano / corrupto*: `CleanOrphanTmp()` ejecuta únicamente `File.Exists(tmpPath)` y `File.Delete(tmpPath)` dentro de un bloque `try/catch` que registra advertencias en `DebugLog`. En ningún momento se abre, lee o deserializa el archivo `.tmp`. Si un corte de energía ocurre a mitad de la escritura y deja un `.tmp` de 0 bytes o con JSON incompleto/corrupto, el método lo elimina de forma silenciosa e inmediata, permitiendo que la carga continúe con el archivo final íntegro existente sin fallos.
  - *Auditoría de escalado en AppLibrary y Home*:
    - Grep de `Size2D(` y `Position2D(` en `AppLibrary.cs`: 0 ocurrencias (la clase contiene 74 líneas de lógica pura de modelo y navegación sin referencias a `Tizen.NUI`).
    - La cuadrícula de apps correspondiente a la mención original reside en `Home.cs:BuildLibrary()` (~450-490), donde todos los cálculos (`Position2D`, `Size2D`, `CornerRadius`, `BorderlineWidth`, textos y paginadores) se encuentran gobernados por `SclX()`, `SclY()`, y `SclF()`.

---

## CORRECCIÓN 4 — Estrategia de distribución: Compilación reproducible sin binarios pre-firmados
- **Cambio de paradigma en la distribución**:
  - Se retiró la promesa de distribuir un `.tpk` binario pre-firmado listo para instalar.
  - **Motivo técnico**: En Samsung Smart TV (firmware comercial), la instalación mediante `sdb install` o `pkgcmd -i -t tpk` exige obligatoriamente un certificado distribuidor de Samsung emitido contra el **DUID (Device Unique ID)** del televisor objetivo. Un `.tpk` firmado con el DUID de un desarrollador fallará de forma irrecuperable en el televisor de otro usuario arrojando el error `Check certificate error [-12]`. Esta es una restricción no negociable de la plataforma Tizen en TVs Samsung.
  - **Nueva estrategia**: El artefacto distribuible oficial es el **código fuente completo + el script de build automatizado**. Cada usuario compila localmente con .NET 6 y firma el paquete resultante con su propio perfil de certificado Samsung vinculado al DUID de su TV.
- **Auditoría y parametrización de `tools/build.py`**:
  - Verificado que `tools/build.py` **no contiene** certificados hardcodeados (`.p12`), DUIDs, IPs ni rutas locales del autor.
  - Se confirmó que el script compila y empaqueta `.tpk`s **sin firmar** (`opt-quiet-tizen-lite-unsigned.tpk` y `opt-quiet-tizen-full-unsigned.tpk`), dejando la firma como un paso manual explícito para el usuario.
  - Se parametrizaron las variables de entorno (`DOTNET`, `TIZEN_FRAMEWORK`, `TV_FRAMEWORK_DIR`) y argumentos CLI (`--variant full|lite`, `--container`, `--output-tpk`, `--no-tpk`) con valores por defecto portables.
- **Limpieza de artefactos pre-firmados**:
  - Se confirmó que ningún archivo `.tpk` se encuentra bajo control de versiones en git.
  - La regla `*.tpk` permanece activa en `.gitignore`.
- **Actualización de documentación (`README.md`)**:
  - Se reemplazaron las secciones obsoletas de "1-Click TV Install" y binarios pre-firmados por una guía exhaustiva de 8 pasos:
    1. Requisitos previos (.NET 6 SDK, Python 3.10+, Tizen Studio con TV SDK).
    2. Conexión del televisor (Developer Mode en el TV y registro en Device Manager).
    3. Habilitación de la extensión Samsung Certificate Extension en Package Manager.
    4. Creación del perfil de certificado Samsung (Author y Distributor ambos de Samsung, vinculando el DUID del TV).
    5. Selección de variante: Lite (APIs públicas sin privilegios) vs Full (ajustes nativos de volumen/brillo).
    6. Compilación mediante `tools/build.py`.
    7. Firma del paquete con `tizen package -t tpk -s <perfil>`.
    8. Instalación en el televisor mediante `sdb install`.

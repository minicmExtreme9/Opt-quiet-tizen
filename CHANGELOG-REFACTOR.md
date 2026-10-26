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
  - *Pendiente de verificación en TV por el dueño.*

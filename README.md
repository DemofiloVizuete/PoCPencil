# Prompt Market

PoC de un marketplace de prompts diseñado en Pencil y convertido en una aplicación de escritorio Windows con C#.

## Arquitectura actual

- `PromptMarket.Desktop`: WPF + WebView2.
- Backend local embebido: ASP.NET Core/Kestrel dentro del mismo proceso.
- Persistencia: Entity Framework Core + SQLite.
- Interfaz: `index.html`, `styles.css` y `app.js`, incrustados como recursos dentro del EXE y servidos por el backend local.
- Base de datos: `%LocalAppData%\PromptMarket\prompt-market.db`.

La aplicación arranca el backend en un puerto localhost dinámico y abre la interfaz dentro de WebView2. El endpoint inicial es `GET /api/prompts`; `GET /api/health` sirve como comprobación de disponibilidad.

## Ejecutar en desarrollo

```powershell
dotnet run --project PromptMarket.Desktop/PromptMarket.Desktop.csproj
```

Requiere el WebView2 Runtime instalado en Windows.

## Publicar el ejecutable

```powershell
dotnet publish PromptMarket.Desktop/PromptMarket.Desktop.csproj `
	-c Release -r win-x64 --self-contained true `
	-p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
	-p:DebugType=None -p:DebugSymbols=false `
	-p:CopyOutputSymbolsToPublishDirectory=false -o publish/one-file
```

El resultado se genera en `publish/one-file/PromptMarket.Desktop.exe`. Los ficheros de la interfaz no se distribuyen aparte. La base de datos y los logs permanecen fuera del EXE para evitar perder datos al actualizar la aplicación.

La única condición de Windows es tener instalado el **Microsoft Edge WebView2 Runtime**. WebView2 puede crear una carpeta de datos de usuario junto al ejecutable o en la ubicación configurada al arrancar; esa carpeta no contiene el código de la aplicación ni es necesaria para copiar junto al EXE.

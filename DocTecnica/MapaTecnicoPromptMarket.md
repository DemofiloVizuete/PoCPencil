# Mapa técnico de Prompt Market

## 1. Visión general
Prompt Market es una prueba de concepto (PoC) de un marketplace de prompts implementado como aplicación de escritorio Windows con una capa frontend web incrustada en WebView2 y un backend local ejecutado dentro del mismo proceso.

La arquitectura principal es:
- WPF como shell de escritorio
- WebView2 para renderizar la UI web
- Kestrel + ASP.NET Core como backend local embebido
- SQLite + EF Core como almacenamiento persistente
- HTML/CSS/JS para la interfaz del marketplace

## 2. Componentes del sistema

### 2.1 Frontend desktop
Ubicación relevante:
- PromptMarket.Desktop/MainWindow.xaml
- PromptMarket.Desktop/MainWindow.xaml.cs

Responsabilidades:
- Crear la ventana principal de la aplicación
- Arrancar el backend local al cargar la ventana
- Esperar a que WebView2 esté listo
- Navegar a la URL de inicio del backend

Flujo:
1. La app se inicia en `MainWindow`.
2. En `Loaded`, se ejecuta `OnLoadedAsync`.
3. Se llama a `apiHost.StartAsync()`.
4. Se habilita `Browser.EnsureCoreWebView2Async()`.
5. Se asigna `Browser.Source = apiHost.BaseAddress`.
6. La interfaz se muestra dentro del navegador embebido.

### 2.2 Backend local embebido
Ubicación:
- PromptMarket.Desktop/Api/LocalApiHost.cs

Responsabilidades:
- Crear el host web ASP.NET Core con Kestrel
- Configurar la persistencia con SQLite
- Servir archivos estáticos embebidos
- Exponer endpoints HTTP para la app
- Cargar datos iniciales de prueba si la base de datos está vacía

Tecnologías:
- ASP.NET Core
- Kestrel
- EF Core
- SQLite

Configuración relevante:
- `UseKestrel(options => options.Listen(IPAddress.Loopback, 0))`
- Puerto dinámico en localhost
- `ContentRootPath = AppContext.BaseDirectory`

### 2.3 Persistencia
Ubicación:
- PromptMarket.Desktop/Api/PromptDbContext.cs
- PromptMarket.Desktop/Api/Prompt.cs

Responsabilidades:
- Definir el modelo `Prompt`
- Exponer el DbSet `Prompts`
- Crear la base de datos SQLite automáticamente si no existe

Modelo principal:
- Id
- Title
- Category
- Description
- Creator
- Price
- Sales

Ruta física de la base de datos:
- `%LocalAppData%\PromptMarket\prompt-market.db`

### 2.4 Frontend web
Ubicación:
- app.js
- index.html
- styles.css

Responsabilidades:
- Mostrar la UI del marketplace
- Consultar la API local
- Renderizar tarjetas o listados de prompts
- Mostrar datos de precio, ventas y creador

Aunque el proyecto usa WebView2, la parte visual se implementa como una mini aplicación web servida por Kestrel desde recursos embebidos del ensamblado.

## 3. Proceso de arranque

### Secuencia real
1. El usuario ejecuta la aplicación WPF.
2. `MainWindow` se inicializa.
3. El host API local se crea y arranca.
4. El servidor Kestrel abre un puerto de localhost aleatorio.
5. Se crea la BD SQLite si no existe.
6. Se ejecuta `SeedAsync()` para insertar prompts iniciales.
7. Se obtiene la URL base del host, por ejemplo `http://127.0.0.1:xxxxx/`.
8. WebView2 navega a la URL base.
9. El frontend carga `index.html`, `styles.css` y `app.js`.
10. La UI consulta `GET /api/prompts`.

## 4. Endpoints expuestos

### GET /api/health
Respuesta:
- status: ok

Uso:
- Validar que la API local está disponible.

### GET /api/prompts
Respuesta:
- Lista de prompts ordenados por número de ventas descendente.

Consulta:
- `db.Prompts.AsNoTracking().OrderByDescending(prompt => prompt.Sales).ToListAsync()`

## 5. Mecanismo de archivos embebidos
El proyecto define una ruta web con recursos que se leen desde el ensamblado del proyecto:
- `PromptMarket.Desktop.Web.index.html`
- `PromptMarket.Desktop.Web.styles.css`
- `PromptMarket.Desktop.Web.app.js`

Se sirven con el método `MapEmbeddedFile()`, que:
- localiza el recurso por nombre
- valida que exista
- establece el tipo MIME adecuado
- copia el contenido al cuerpo HTTP de la respuesta

Esto permite que la app se ejecute sin depender de un bundle externo visible en disco.

## 6. Datos iniciales
En `SeedAsync()` se insertan 3 prompts de ejemplo:
- The Brand Voice Architect
- The Fast Research Partner
- Your Weekly Chief of Staff

Estos datos sirven como base de prueba para que la UI muestre contenido al arrancar.

## 7. Flujo de datos

### 7.1 Datos de prompts
- El usuario abre la app.
- El backend inicia la DB.
- La base de datos se crea automáticamente.
- Si no existe contenido, se sembrarán prompts de ejemplo.
- La API devuelve la colección en JSON.
- El frontend renderiza los prompts en pantalla.

### 7.2 Relación entre capas
- WPF (shell) -> inicia backend
- Backend ASP.NET -> sirve UI + API
- EF Core -> persiste en SQLite
- WebView2 -> consume la UI web

## 8. Diagrama conceptual

```text
[Usuario]
    |
    v
[WPF / MainWindow]
    |
    | 1. StartAsync()
    v
[LocalApiHost]
    |-- crea Kestrel
    |-- crea SQLite
    |-- sirve archivos embebidos
    |-- expone /api/health y /api/prompts
    v
[SQLite / prompt-market.db]
    |
    |  data access via EF Core
    v
[Frontend Web (index.html + app.js + styles.css)]
    |
    | consume API JSON
    v
[WebView2]
    |
    v
[UI renderizada en escritorio]
```

## 9. Dependencias técnicas clave
- .NET 10 WPF
- Microsoft.Web.WebView2
- ASP.NET Core
- Entity Framework Core
- Microsoft.EntityFrameworkCore.Sqlite

## 10. Consideraciones de diseño
- La aplicación mantiene un backend local embebido para simplificar despliegue y uso en un escritorio Windows.
- El puerto es dinámico, evitando conflictos con otros servicios locales.
- La base de datos vive en el perfil de usuario, lo que facilita la persistencia sin empaquetar la BD dentro del EXE.
- La UI se sirve desde recursos embebidos para que el ejecutable sea más autónomo.

## 11. Resumen operativo
Prompt Market es un concepto de aplicación híbrida donde una ventana WPF lanza un backend local, sirve una interfaz web embebida y persiste datos en SQLite. La interacción entre escritorio y web se realiza a través de una URL local `localhost` y una API REST muy pequeña. 

## 12. Puntos de extensión futuros
- Agregar CRUD completo de prompts
- Añadir autenticación de creadores
- Añadir filtro y búsqueda
- Mejorar la seguridad del backend local
- Preparar exportación e importación de prompts
- Mejorar la arquitectura separando API, dominio y acceso a datos

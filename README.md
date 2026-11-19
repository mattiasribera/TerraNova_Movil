# TerraNova Bienes Raíces — Versión móvil (.NET MAUI)

Versión móvil y de escritorio de [TerraNova Bienes Raíces](https://github.com/LeonardoAvilaB/TerraNova_Bienes_Raices) (originalmente en Blazor).
Gestiona **propiedades, clientes, trabajadores, ventas, visitas y usuarios**, con **mapas**, fotos y roles (Admin / Agente).

| Capa | Tecnología |
|---|---|
| App (Android, iOS, Windows) | .NET MAUI 10 + CommunityToolkit.Maui + CommunityToolkit.Mvvm |
| Mapas | Mapsui (OpenStreetMap, sin API Key) |
| API | ASP.NET Core 10 Web API + Entity Framework Core |
| Base de datos | SQL Server |
| Seguridad | Contraseñas con BCrypt |

---

## 1. Requisitos

- **Visual Studio 2022/2026** con la carga de trabajo **"Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET (MAUI)"**
- **.NET SDK 10** (el repo trae `global.json`)
- **SQL Server** (Developer, Express o LocalDB) — y opcionalmente SSMS

## 2. Puesta en marcha (primera vez)

```bash
git clone <url-de-este-repo>
cd TerraNova
```

### 2.1 Configurar SU base de datos (cada uno usa la suya)

1. Vaya a `TerraNova.Api/`
2. Copie `appsettings.Local.example.json` y renómbrelo a **`appsettings.Local.json`**
3. Deje la cadena de conexión que corresponda a su SQL Server:

| Su SQL Server | `CadenaSql` |
|---|---|
| Instalación normal (instancia por defecto) | `Server=localhost;Database=TerraNovaDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;` |
| SQL Server **Express** | `Server=localhost\\SQLEXPRESS;Database=TerraNovaDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;` |
| Usuario `sa` / Docker | `Server=localhost,1433;Database=TerraNovaDB;User Id=sa;Password=SU_CLAVE;Encrypt=False;TrustServerCertificate=True;` |

> `appsettings.Local.json` **no se sube a GitHub**, así nadie pisa la configuración de otro.
> Si su SQL es la instalación normal, puede saltarse este paso: la API ya trae esa cadena por defecto.

### 2.2 Ejecutar

1. Abra **`TerraNova.slnx`** en Visual Studio
2. Clic derecho en la solución → **Configurar proyectos de inicio** → **Varios proyectos de inicio**:
   - `TerraNova.Api` → **Iniciar**
   - `TerraNova` → **Iniciar** (elija *Windows Machine* o un emulador Android)
3. Presione **F5**

**La primera vez, la API crea sola la base de datos** `TerraNovaDB` con datos de prueba (ejecuta `BaseDatos/ScriptCompleto.sql`).
No hace falta abrir SSMS. Si la base ya existe, no la toca.

### 2.3 Ingresar

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123!` | Admin |
| `lucia`, `carlos`, `valeria` | `Agente123!` | Agente |

Desde **Usuarios** puede crear más agentes vinculados a un trabajador.

### 2.4 Datos de demostración

Al crear la base, la API también genera **datos realistas de Santa Cruz** (todos obtienen los mismos):
~65 propiedades con **foto y ubicación en el mapa**, ~65 clientes, 13 trabajadores, ventas con su comisión
y ~150 visitas (historial + agenda de los próximos días).

Si su base ya existía, puede cargarlos desde Swagger (http://localhost:5150/swagger) → `POST /api/Demo/Generar`.
Solo se cargan una vez (no se duplican) y solo funciona en modo desarrollo.

---

## 3. Estructura de la solución

```
TerraNova.slnx
├─ BaseDatos/            ScriptCompleto.sql (tablas + datos de prueba)
├─ TerraNova.Shared/     DTOs compartidos entre la API y la app
├─ TerraNova.Api/        API REST (controladores, EF Core, imágenes, creación de la BD)
└─ TerraNova/            App MAUI (MVVM)
   ├─ Constantes/        Iconos (Fluent UI), Rutas de navegación
   ├─ Helpers/           Números (acepta coma y punto decimal)
   ├─ Models/            Modelos de pantalla (dashboard, filas)
   ├─ Services/          Consumo de la API, autenticación, imágenes, diálogos
   ├─ ViewModels/        Un ViewModel por pantalla, en carpetas por módulo
   ├─ Views/             Páginas XAML, en carpetas por módulo
   ├─ Converters/  Mapas/
   └─ Resources/Styles/TerraNova.xaml   ← colores y estilos de toda la app
```

La app **nunca se conecta directo a la base de datos**: todo pasa por la API (`App → API → SQL Server`).

## 4. Imágenes

Las fotos se **comprimen en el dispositivo** (máx. 1280 px, JPEG 80 %), se suben a `api/Imagen/Subir` y se guardan como
archivo en `TerraNova.Api/wwwroot/imagenes/`. En la base de datos solo queda la ruta (`imagenes/trabajadores/xxx.jpg`).

## 5. Problemas frecuentes

| Problema | Solución |
|---|---|
| "No se pudo conectar con la API" | Verifique que `TerraNova.Api` esté corriendo (http://localhost:5150/swagger) |
| La API dice "No se pudo conectar/crear la base de datos" | Revise `CadenaSql` en `appsettings.Local.json` y que el servicio SQL Server esté iniciado |
| Android Emulator no conecta | La app usa `10.0.2.2:5150` (ya configurado). En un **celular físico** cambie la IP en `TerraNova/Services/Configuracion.cs` por la de su PC |
| Quiero empezar con la base limpia | Borre `TerraNovaDB` en SSMS y vuelva a ejecutar la API: se crea de nuevo |
| Error `MSB3030 ... MsixContent ... no se encontró` al compilar en Windows | La ruta de la carpeta es demasiado larga (límite de 260 caracteres de Windows). Clone el repo en una ruta corta, por ejemplo `C:\Proyectos\TerraNova_Movil` |

## 6. Documentación

- [`docs/GUIA_ESTUDIO.md`](docs/GUIA_ESTUDIO.md) — guía de estudio: arquitectura, recorridos paso a paso, preguntas difíciles y guión de demo
- [`docs/MAPAS.md`](docs/MAPAS.md) — cómo funcionan y cómo se arman los mapas (Mapsui)
- [`docs/GLOSARIO.md`](docs/GLOSARIO.md) — 20 preguntas y respuestas para la presentación
- [`TerraNova/INVESTIGACION.md`](TerraNova/INVESTIGACION.md) — investigación de CommunityToolkit.Maui

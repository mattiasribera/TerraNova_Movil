# Mapas en TerraNova MAUI — cómo funcionan y cómo se arman

Todo lo que hay que saber de los mapas de la app, con el código real del proyecto.

---

## 1. Qué librería usamos y por qué

Usamos **Mapsui** (paquete NuGet `Mapsui.Maui`, versión 5.2.0), con los mapas de **OpenStreetMap**.

| Opción | ¿Necesita API Key? | ¿Windows? | Comentario |
|---|---|---|---|
| **Mapsui + OpenStreetMap** ✅ | No | **Sí** | Gratis, código abierto, dibuja con SkiaSharp |
| Microsoft.Maui.Controls.Maps | Sí (Google, en Android) | **No** | Usa el mapa nativo (Google Maps / Apple Maps) |
| Leaflet (la versión web) | No | — | Es JavaScript; lo usaba Blazor con `mapaInterop.js` |

**Por qué Mapsui:** la web original usaba **Leaflet con OpenStreetMap**. Mapsui muestra **los mismos mapas**, no
necesita registrarse en Google ni tarjeta de crédito, y funciona en **Windows** (donde presentamos) y en **Android**.

---

## 2. Conceptos básicos (lo que hay que entender para explicarlo)

### 2.1 Teselas (tiles)
El mapa **no es una imagen gigante**: está cortado en cuadritos de 256×256 píxeles llamados **teselas**.
A medida que mueves el mapa, Mapsui pide a los servidores de OpenStreetMap solo las teselas que se ven en pantalla.
Por eso el mapa necesita **internet**.

### 2.2 Niveles de zoom y resolución
Cada **nivel de zoom** tiene el doble de detalle que el anterior. Mapsui trabaja con la **resolución**
(cuántos metros del mundo real ocupa un píxel):

| Zoom | Resolución aprox. | Se ve... | Dónde lo usamos |
|---|---|---|---|
| 13 | ~19 m/píxel | toda la ciudad | Mapa general y formulario (`CrearMapa`, zoom por defecto) |
| 15 | ~4,8 m/píxel | un barrio | Desde aquí aparecen los nombres (`MaxVisible = 5`) |
| 16 | ~2,4 m/píxel | calles | Detalle de propiedad y al editar |

### 2.3 Dos sistemas de coordenadas (¡pregunta típica!)
- **Latitud/Longitud (WGS84, EPSG:4326)**: grados. Es lo que guardamos en la base de datos.
  Santa Cruz centro: **lat -17.7833, lng -63.1821**.
- **Spherical Mercator (EPSG:3857)**: **metros** sobre un plano. Es lo que usan los mapas web para dibujar
  (Google, OpenStreetMap, Leaflet, Mapsui).

Por eso siempre **convertimos** antes de dibujar y después de tocar el mapa:

```csharp
// Mapas/MapaHelper.cs
// Lat/Lng (grados) -> coordenadas del mapa (metros)
public static MPoint APunto(double Lat, double Lng)
{
    var (X, Y) = SphericalMercator.FromLonLat(Lng, Lat);   // ¡ojo! primero Longitud, después Latitud
    return new MPoint(X, Y);
}

// Coordenadas del mapa -> Lat/Lng (para guardar en la base)
public static (double Lat, double Lng) ALatLng(MPoint Punto)
{
    var (Lng, Lat) = SphericalMercator.ToLonLat(Punto.X, Punto.Y);
    return (Lat, Lng);
}
```

### 2.4 Capas, elementos y estilos
Un mapa de Mapsui es una **pila de capas** (como hojas transparentes una encima de otra):

```
 ┌─────────────────────────────┐
 │ Capa "Propiedades"           │  ← MemoryLayer: nuestros marcadores (PointFeature)
 ├─────────────────────────────┤
 │ Capa OpenStreetMap           │  ← TileLayer: las calles (teselas descargadas)
 └─────────────────────────────┘
```

| Término | Qué es | En el código |
|---|---|---|
| `Map` | El mapa (capas + navegador) | `new Mapsui.Map()` |
| `MapControl` | El control visual que se pone en el XAML | `<mapsui:MapControl x:Name="Mapa"/>` |
| `TileLayer` | Capa de calles hecha de teselas | `OpenStreetMap.CreateTileLayer(...)` |
| `MemoryLayer` | Capa con nuestros datos, en memoria | `new MemoryLayer("Propiedades")` |
| `PointFeature` | Un elemento del mapa (un punto) | un marcador = una propiedad |
| `SymbolStyle` | Cómo se dibuja el punto (círculo, color) | color según el estado |
| `LabelStyle` | Texto junto al punto | el título de la propiedad |
| `Navigator` | Mueve y hace zoom | `CenterOnAndZoomTo(...)` |

---

## 3. Cómo se arma un mapa, paso a paso

### Paso 1 — Instalar y activar
```xml
<!-- TerraNova.csproj -->
<PackageReference Include="Mapsui.Maui" Version="5.2.0" />
```
```csharp
// MauiProgram.cs — Mapsui dibuja con SkiaSharp, hay que activarlo
builder.UseMauiApp<App>()
       .UseSkiaSharp()
       ...
```

### Paso 2 — Poner el control en la pantalla
```xml
<!-- Views/Propiedades/MapaPropiedadesPage.xaml -->
xmlns:mapsui="clr-namespace:Mapsui.UI.Maui;assembly=Mapsui.UI.Maui"
...
<mapsui:MapControl x:Name="Mapa"/>
```

### Paso 3 — Crear el mapa con las calles y centrarlo
```csharp
// Mapas/MapaHelper.cs
public static Mapsui.Map CrearMapa(double Lat = LatInicial, double Lng = LngInicial, int Zoom = 13)
{
    var Mapa = new Mapsui.Map();
    Mapa.Layers.Add(OpenStreetMap.CreateTileLayer("TerraNova-MAUI"));   // 1) capa de calles
    var Centro = APunto(Lat, Lng);                                       // 2) lat/lng -> metros
    Mapa.Navigator.CenterOnAndZoomTo(Centro, Mapa.Navigator.Resolutions[Zoom]); // 3) centrar
    return Mapa;
}
```
`"TerraNova-MAUI"` es el **User-Agent**: OpenStreetMap pide que cada aplicación se identifique al pedir teselas
(es parte de su política de uso).

### Paso 4 — Crear un marcador
```csharp
// Mapas/MapaHelper.cs
public static PointFeature CrearMarcador(double Lat, double Lng, string? Texto = null, string? Estado = null)
{
    var C = EstadoAColorConverter.ColorDe(Estado ?? "Disponible");   // mismo color que en las listas
    var Marcador = new PointFeature(APunto(Lat, Lng));
    Marcador.Styles.Add(new SymbolStyle                               // el círculo de color
    {
        SymbolScale = 0.7,
        Fill = new Brush(new Color(...C...)),
        Outline = new Pen(Color.White, 2)
    });
    if (!string.IsNullOrWhiteSpace(Texto))
        Marcador.Styles.Add(new LabelStyle                             // el nombre
        {
            Text = Texto,
            Offset = new Offset(0, -24),     // encima del punto
            MaxVisible = 5                   // solo con zoom de calle (con 100 propiedades se encimaban)
        });
    return Marcador;
}
```

### Paso 5 — Juntar los marcadores en una capa
```csharp
// Mapas/MapaHelper.cs
public static MemoryLayer CrearCapaPropiedades(IEnumerable<PropiedadDTO> Propiedades)
{
    var Marcadores = new List<IFeature>();
    foreach (var P in Propiedades.Where(P => P.Latitud.HasValue && P.Longitud.HasValue))  // solo las que tienen ubicación
    {
        var M = CrearMarcador(P.Latitud!.Value, P.Longitud!.Value, P.Titulo, P.Estado);
        M["Id"] = P.Id;          // guardamos el Id dentro del marcador para saber cuál se tocó
        Marcadores.Add(M);
    }
    return new MemoryLayer("Propiedades") { Features = Marcadores, Style = null };
}
```
`Style = null` en la capa significa: *"no uses un estilo general, cada marcador trae el suyo"* (por eso cada uno tiene su color).

---

## 4. Los 3 mapas de la app

### 4.1 Mapa general — `Views/Propiedades/MapaPropiedadesPage.xaml.cs`
Muestra **todas** las propiedades con un color según su estado.

```csharp
public MapaPropiedadesPage(ServicioPropiedades servicio)
{
    InitializeComponent();
    Mapa.Map = MapaHelper.CrearMapa();          // calles + centro en Santa Cruz
    Mapa.Map.Tapped += AlTocarMapa;             // escuchar los toques
}

protected override async void OnAppearing()
{
    var Lista = await Servicio.Lista();                       // GET api/Propiedad/Lista
    if (CapaPropiedades != null) Mapa.Map.Layers.Remove(CapaPropiedades);  // quitar la capa vieja
    CapaPropiedades = MapaHelper.CrearCapaPropiedades(Lista);
    Mapa.Map.Layers.Add(CapaPropiedades);                     // agregar la nueva
}

// Tocar un marcador -> abrir su detalle
private void AlTocarMapa(object? sender, Mapsui.MapEventArgs e)
{
    var Info = e.GetMapInfo(new[] { CapaPropiedades });       // ¿qué hay donde se tocó?
    if (Info?.Feature?["Id"] is int Id)                       // ¿es un marcador? leemos su Id
    {
        e.Handled = true;
        Shell.Current.GoToAsync(Rutas.ConId(Rutas.DetallePropiedad, Id));
    }
}
```
La **leyenda** de colores (Disponible verde, Reservada amarillo, Vendida rojo, Alquilada azul) está en el XAML,
encima del mapa.

### 4.2 Mapa del detalle — `Views/Propiedades/DetallePropiedadPage.xaml.cs`
Muestra **una** propiedad, con zoom de calle (16). Si no tiene ubicación, el mapa se oculta y aparece
*"Esta propiedad no tiene ubicación registrada"*.

```csharp
var M = MapaHelper.CrearMapa(P.Latitud.Value, P.Longitud.Value, 16);   // centrado EN la propiedad
M.Layers.Add(new MemoryLayer("Marcador")
{
    Features = new[] { MapaHelper.CrearMarcador(P.Latitud.Value, P.Longitud.Value, P.Titulo, P.Estado) },
    Style = null
});
Mapa.Map = M;
```

### 4.3 Mapa del formulario (elegir la ubicación) — `Views/Propiedades/PropiedadFormPage.xaml.cs`
Es igual que en la web: **tocas el mapa y ahí queda la propiedad**.

```csharp
private void AlTocarMapa(object? sender, Mapsui.MapEventArgs e)
{
    var (Lat, Lng) = MapaHelper.ALatLng(e.WorldPosition);   // punto tocado (metros) -> lat/lng
    Vm.Latitud = Lat;                                       // al ViewModel (se muestra "Lat: ... Lng: ...")
    Vm.Longitud = Lng;
    CapaMarcador.Features = new[] { MapaHelper.CrearMarcador(Lat, Lng) };   // mover el marcador
    CapaMarcador.DataHasChanged();                          // avisar a Mapsui que redibuje
    e.Handled = true;
}
```
Al **editar**, el ViewModel dispara `UbicacionCargada` y la página centra el mapa en la ubicación guardada con zoom 16.
Al **guardar**, `Latitud` y `Longitud` viajan en el `PropiedadDTO` a la API y quedan en la base de datos.

---

## 5. El recorrido completo de una ubicación

```
 Usuario toca el mapa
        │  e.WorldPosition (metros, Spherical Mercator)
        ▼
 MapaHelper.ALatLng  ──►  (-17.7650, -63.1975)   grados
        │
        ▼
 PropiedadFormViewModel.Latitud / Longitud
        │  Guardar  →  PropiedadDTO
        ▼
 POST api/Propiedad/Guardar   ──►   SQL Server: Propiedades.Latitud / Longitud  (float, pueden ser NULL)
        │
        ▼  (cuando alguien abre el mapa)
 GET api/Propiedad/Lista  ──►  MapaHelper.CrearCapaPropiedades  ──►  APunto (grados → metros)  ──►  marcador
```

---

## 6. Preguntas que te pueden hacer sobre los mapas

**¿Por qué no usaron Google Maps?**
Necesita una API Key con cuenta de facturación, y el control de mapas de MAUI no funciona en Windows.
Mapsui con OpenStreetMap es gratis, no necesita registro y funciona en las dos plataformas.

**¿El mapa funciona sin internet?**
No: las teselas se descargan de OpenStreetMap. Los marcadores sí son nuestros. Como mejora se podría guardar
un caché de teselas en el dispositivo.

**¿Por qué hay que convertir las coordenadas?**
La base guarda **grados** (lat/lng), pero los mapas web dibujan en **metros** (Spherical Mercator, EPSG:3857).
`SphericalMercator.FromLonLat` convierte para dibujar y `ToLonLat` convierte el punto tocado para guardarlo.

**¿Cómo saben qué propiedad tocaste?**
Cada marcador lleva su `Id` guardado (`M["Id"] = P.Id`). Al tocar, `GetMapInfo` devuelve el elemento que está
debajo del dedo, leemos su `Id` y navegamos al detalle.

**¿Por qué los nombres solo aparecen al acercar?**
Con más de 100 propiedades, los textos se encimaban. `LabelStyle.MaxVisible = 5` hace que el nombre solo se dibuje
cuando la resolución es de 5 m/píxel o menos (zoom de barrio). De lejos solo se ven los puntos de colores.

**¿Por qué Latitud y Longitud pueden ser NULL?**
Porque una propiedad se puede registrar sin marcar la ubicación. En ese caso no aparece en el mapa general
(`Where(P => P.Latitud.HasValue && ...)`) y el detalle muestra un mensaje en lugar del mapa.

**¿Qué es el User-Agent "TerraNova-MAUI"?**
Es cómo nuestra app se identifica ante los servidores de OpenStreetMap. Su política de uso pide que cada
aplicación se identifique; si no, pueden bloquear las peticiones.

**¿Qué es SkiaSharp y qué tiene que ver?**
Es la librería de gráficos 2D (de Google, la misma de Chrome y Android) que Mapsui usa para dibujar las calles,
los círculos y los textos. Por eso en `MauiProgram.cs` se activa con `.UseSkiaSharp()`.
También la usamos para comprimir las fotos.

**¿Qué mejoras le harían a los mapas?**
- Filtros en el mapa (por tipo, estado y precio).
- Agrupar marcadores cercanos (*clustering*) cuando hay muchos.
- Botón **"Cómo llegar"**, que abre Google Maps o Waze con `Map.Default.OpenAsync(lat, lng)` (API de MAUI).
- Mostrar "mi ubicación" con `Geolocation` de MAUI.
- Caché de teselas para usarlo sin internet.

# Investigación: CommunityToolkit.Maui
**Proyecto:** TerraNova Bienes Raíces, requerimiento *Registrar propiedad*

## 1. ¿Qué es?
**CommunityToolkit.Maui** es una librería gratuita y de código abierto que mantiene la comunidad de .NET, con respaldo de Microsoft (forma parte de la .NET Foundation). Agrega a .NET MAUI controles, alertas, validaciones y conversores que MAUI no trae de serie.

- Paquete NuGet: `CommunityToolkit.Maui`, versión usada **16.0.0**
- Documentación oficial: https://learn.microsoft.com/dotnet/communitytoolkit/maui/
- Código fuente: https://github.com/CommunityToolkit/Maui

## 2. ¿Por qué la elegí?
En la versión web de TerraNova (Blazor) tuvimos que **programar a mano** el sistema de notificaciones: `ToastService.cs` para la lógica y `ToastNotificacion.razor` para el diseño. En MAUI, CommunityToolkit.Maui resuelve lo mismo con **una línea de código**, y además valida los datos numéricos sin escribir validaciones a mano.

## 3. Instalación (3 pasos)
1. **Instalar el paquete:** en Visual Studio, clic derecho en el proyecto → *Administrar paquetes NuGet* → buscar `CommunityToolkit.Maui` → Instalar. También se puede desde la consola:
   ```
   dotnet add package CommunityToolkit.Maui
   ```
2. **Activarlo en `MauiProgram.cs`:**
   ```csharp
   using CommunityToolkit.Maui;

   builder
       .UseMauiApp<App>()
       .UseMauiCommunityToolkit(options => options.SetShouldEnableSnackbarOnWindows(true));
   ```
   > ⚠️ **Lo que descubrí probando:** en Windows el Toast viene **desactivado**. Sin `SetShouldEnableSnackbarOnWindows(true)`, la app se cierra con el error *"To enable Toast on Windows, call UseMauiCommunityToolkit(options => options.SetShouldEnableSnackbarOnWindows(true))"*.
3. **Agregar el espacio de nombres en el XAML:**
   ```xml
   xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
   ```

## 4. Función 1: Toast (notificaciones)
Un **Toast** es un mensaje corto que aparece unos segundos y **desaparece solo**, sin que el usuario tenga que presionar nada.

```csharp
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

var Aviso = Toast.Make($"Propiedad '{Titulo.Text}' registrada ({Vis + 1} en total)", ToastDuration.Short, 16);
await Aviso.Show();
```
| Parámetro | Significado |
|---|---|
| Texto | El mensaje a mostrar |
| `ToastDuration.Short` / `Long` | Cuánto tiempo se ve (corto o largo) |
| `16` | Tamaño de la letra |

**Cómo se ve en cada plataforma:**
- **Android:** el clásico mensaje gris en la parte inferior de la pantalla.
- **Windows:** una notificación del sistema en la esquina inferior derecha.

**Diferencia con `DisplayAlert`:**
| `DisplayAlert` | `Toast` |
|---|---|
| Bloquea la pantalla hasta que se presiona "OK" | No interrumpe; desaparece solo |
| Sirve para **errores** que el usuario debe atender | Sirve para **confirmaciones** ("se guardó") |

En el proyecto uso los dos: `DisplayAlert` para los campos obligatorios y `Toast` para confirmar el registro.

## 5. Función 2: NumericValidationBehavior (validación)
Un **Behavior** (comportamiento) es una funcionalidad que se "engancha" a un control existente **sin modificarlo**. `NumericValidationBehavior` revisa **mientras el usuario escribe** si el texto de un `Entry` es un número válido dentro de un rango.

```xml
<Entry x:Name="Precio" Keyboard="Numeric">
    <Entry.Behaviors>
        <toolkit:NumericValidationBehavior
            x:Name="ValidaPrecio"
            InvalidStyle="{StaticResource Invalido}"
            ValidStyle="{StaticResource Valido}"
            Flags="ValidateOnValueChanged"
            MinimumValue="1"
            MaximumValue="10000000"
            MaximumDecimalPlaces="0"/>
    </Entry.Behaviors>
</Entry>
```
| Propiedad | Significado |
|---|---|
| `InvalidStyle` / `ValidStyle` | Estilo cuando es inválido (texto rojo) o válido (texto negro) |
| `Flags="ValidateOnValueChanged"` | Valida en cada tecla que se presiona |
| `MinimumValue` / `MaximumValue` | Rango permitido |
| `MaximumDecimalPlaces="0"` | Solo números enteros |

Además, desde C# se consulta si el valor es válido antes de guardar:
```csharp
if (!ValidaPrecio.IsValid)
{
    await DisplayAlertAsync("Dato invalido", "El precio debe ser un numero entero mayor a 0", "OK");
    return;
}
```
Esto evita que `Convert.ToInt32` falle y cierre la app si el usuario escribe letras.

Lo uso en **Precio** (1 a 10.000.000) y **Área** (1 a 100.000 m²).

## 6. Otras funciones del Toolkit
| Función | Para qué sirve |
|---|---|
| `Snackbar` | Como el Toast, pero con un botón de acción (ej. "Deshacer") |
| `Popup` | Ventanas emergentes personalizadas |
| `EmailValidationBehavior` | Valida correos electrónicos |
| `TextValidationBehavior` | Valida longitud o formato de textos |
| `MaskedBehavior` | Aplica máscaras de entrada (ej. teléfono `XXX-XXXXXXX`) |
| `AvatarView`, `Expander`, `DrawingView` | Controles visuales adicionales |
| Converters | Transforman datos en los bindings (ej. `IsStringNullOrEmptyConverter`) |

## 7. Conclusión
CommunityToolkit.Maui se integra en pocos minutos y mejora la experiencia del usuario de TerraNova en dos aspectos:
1. **Avisos no invasivos (Toast):** confirman el registro sin interrumpir.
2. **Validación en tiempo real (NumericValidationBehavior):** el usuario ve el error mientras escribe, y la app no se cierra por datos mal ingresados.

Es el equivalente en MAUI a las librerías complementarias de Blazor, como `Blazored.LocalStorage` o el `ToastService` que programamos a mano en la versión web de TerraNova.

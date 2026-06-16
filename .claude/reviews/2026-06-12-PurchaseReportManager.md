# Code Review — PurchaseReportManager Feature — 2026-06-12

## Resumen Ejecutivo

La feature `PurchaseReportManager` implementa correctamente la arquitectura vertical slice con tres proyectos separados (Proxy, ViewModels, Views). El código sigue la mayoría de convenciones del proyecto, pero presenta **tres déficits arquitectónicos significativos** en la gestión de dependencias, patrones de constructor y ausencia de carpeta Models. La implementación de async/await, IDisposable y inyección de dependencias es correcta.

---

## 🔴 Bugs Críticos

### Primary Constructor — Redundancia de asignación en Proxy
- **Archivo:** `PurchaseReportManager.Proxy/PurchaseReportProxy.cs:1-20`
- **Descripción:** El constructor primario declara parámetros `(HttpClient httpClient, IOptions<PurchaseReportProxyOptions> options, ILogger<PurchaseReportProxy> logger)`, pero luego extrae manualmente `options` a campo privado: `private readonly PurchaseReportProxyOptions _options = options.Value;`. Esto viola el patrón primary constructor que busca eliminar campos privados redundantes. El parámetro ya está disponible, solo necesita accederse en los métodos.
- **Corrección sugerida:**
```csharp
internal sealed class PurchaseReportProxy(
    HttpClient httpClient,
    IOptions<PurchaseReportProxyOptions> options,
    ILogger<PurchaseReportProxy> logger) : IPurchaseReportProxy
{
    // NO necesita campo privado _options
    // Acceder directamente como: options.Value.BaseUrl
    
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<HandlerRequestResult<IReadOnlyList<TenantOption>>> GetTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        HandlerRequestResult<IReadOnlyList<TenantOption>> result;
        try
        {
            // Usar options directamente:
            var baseUrl = options.Value.BaseUrl;
            // ...resto del código
        }
        // ...
    }
}
```

### Headers dinámicos ausentes en Proxy
- **Archivo:** `PurchaseReportManager.Proxy/PurchaseReportProxy.cs:73-90` (métodos GetFamiliesAsync, GetSubfamiliesAsync, GetPurchaseReportAsync)
- **Descripción:** El skill `Client/Features/CLAUDE.md` especifica que los Proxies deben agregar headers dinámicos como `X-Tenant-Id` y `X-Application-Id` a cada solicitud HTTP. El código actual construye requests con `TenantRequest()` (que aparentemente no se ve en el archivo), pero no hay evidencia de que esté agregando estos headers. El `PurchaseReportProxyOptions` tiene `ApplicationId = "1"`, pero no se usa en ningún lugar visible.
- **Corrección sugerida:** Implementar un método privado que agregue headers:
```csharp
private HttpRequestMessage TenantRequest(HttpMethod method, string url, string tenantId)
{
    var request = new HttpRequestMessage(method, url);
    request.Headers.Add("X-Tenant-Id", tenantId);
    request.Headers.Add("X-Application-Id", options.Value.ApplicationId);
    return request;
}
```

### Falta carpeta Models en ViewModels
- **Archivo:** `PurchaseReportManager.ViewModels/` (estructura general)
- **Descripción:** El skill establece que ViewModels debe tener carpeta `Models/` con clases `[Entidad]Model.cs`. El proyecto actual no incluye esta carpeta, aunque usa `PurchaseReportLine` (que viene de Domain). Según convención, debería haber un Model propio en el ViewModel para exponer propiedades de UI como `HasSuggestion`, `IsCritical`, `DisplayStatus`.
- **Corrección sugerida:**
```csharp
// PurchaseReportManager.ViewModels/Models/PurchaseReportLineModel.cs
public class PurchaseReportLineModel
{
    public PurchaseReportLine Data { get; set; }
    public bool HasSuggestion => Data.SuggestedQuantity > 0 || Data.SuggestedPackages > 0;
    public bool IsCritical => Data.AlertLevel.Equals("CRITICO", StringComparison.OrdinalIgnoreCase);
    public string DisplayStatus => InventoryDisplayHelper.Label(Data.InventoryStatus);
}
```

### Adapters sin método plural en Proxy
- **Archivo:** `PurchaseReportManager.Proxy/Adapters/PurchaseReportLineAdapter.cs`
- **Descripción:** El adapter solo implementa `ToModel(PurchaseReportLineDto dto)`. El skill requiere también `ToModels(IEnumerable<PurchaseReportLineDto> dtos)` usando `.Select().ToList()` para mantener consistencia con el patrón.
- **Corrección sugerida:**
```csharp
internal static class PurchaseReportLineAdapter
{
    internal static PurchaseReportLine ToModel(PurchaseReportLineDto dto) => new()
    {
        // ... código existente
    };

    // AGREGAR:
    internal static List<PurchaseReportLine> ToModels(IEnumerable<PurchaseReportLineDto> dtos) =>
        dtos.Select(ToModel).ToList();
}
```

### DependencyContainer faltante en Views
- **Archivo:** `PurchaseReportManager.Views/` (no existe DependencyContainer.cs)
- **Descripción:** La carpeta Views no tiene `DependencyContainer.cs`. La convención del proyecto exige un contenedor en cada proyecto feature para mantener consistencia y facilitar testing/extensión futura.
- **Corrección sugerida:** Aunque actualmente la inyección se centraliza en `Program.cs`, crear `PurchaseReportManager.Views/DependencyContainer.cs`:
```csharp
using Microsoft.Extensions.DependencyInjection;

namespace PurchaseReportManager.Views;

public static class DependencyContainer
{
    public static IServiceCollection AddPurchaseReportManagerViews(
        this IServiceCollection services)
    {
        // Aquí se pueden registrar servicios específicos de Views si es necesario
        return services;
    }
}
```

---

## 🟡 Mejoras

### Convenciones de Código

#### Logging en español
- **Archivo:** `PurchaseReportManager.Proxy/PurchaseReportProxy.cs:40, 52, 66, 87, 119` y otros
- **Problema:** Los mensajes de log están en español: `"Error al obtener tenants"`, `"Error al obtener familias"`, etc. La convención del proyecto especifica **inglés obligatorio** en todo el código. Aunque el logging estructurado es correcto (se usa `logger.LogError(ex, "msg {Param}", valor)`), el idioma debe ser inglés.
- **Corrección:**
```csharp
// ❌ ANTES
logger.LogError(ex, "Error al obtener tenants");

// ✅ DESPUÉS
logger.LogError(ex, "Error fetching tenants");
```

#### Lenguaje oblicuo inconsistente en logging
- **Archivo:** `PurchaseReportManager.Proxy/PurchaseReportProxy.cs:119-120`
- **Problema:** El mensaje de log usa términos españoles en la interpolación: `"GetPurchaseReportAsync OK — items={Count} | SKU={Sku} | SalesPeriodQuantity={Ventas} | SuggestedQuantity={Sugerida}..."`. Las variables `Ventas` y `Sugerida` son nombres españoles; deben ser en inglés.
- **Corrección:**
```csharp
logger.LogInformation(
    "GetPurchaseReportAsync OK — items={Count} | SKU={Sku} | SalesPeriodQuantity={SalesQuantity} | SuggestedQuantity={SuggestedQty} | AlertLevel={AlertLevel}",
    items.Count, f.Sku, f.SalesPeriodQuantity, f.SuggestedQuantity, f.AlertLevel);
```

#### Logging con interpolación (parcial)
- **Archivo:** `PurchaseReportManager.Proxy/PurchaseReportProxy.cs:87, 119`
- **Problema:** Algunos logs usan interpolación directa dentro del mensaje (ej: `"Error al obtener reporte..."` y luego parámetros adicionales). La práctica debe ser consistente: todo estructurado sin concatenación en el mensaje.
- **Corrección:** Verificar que TODOS los logs usen `logger.LogXxx(ex, "Template {Param}", param)` nunca `logger.LogXxx($"Template {param}")`.

### Buenas Prácticas .NET/Blazor

#### Componente sin ciclo de vida en PurchaseReportDetailPanel
- **Archivo:** `PurchaseReportManager.Views/Components/PurchaseReportDetailPanel.razor.cs:1-30`
- **Problema:** El componente solo declara `[Parameter]` pero no implementa `OnInitializedAsync` ni ningún método de ciclo de vida. Para un componente que recibe Item como parámetro, esto puede ser aceptable si no hace nada con el parámetro (solo lo renderiza), pero según las convenciones del skill, debería tener un code-behind completo con ciclo de vida.
- **Corrección:** Aunque no es crítico en este caso (el componente es declarativo), considerar agregar:
```csharp
public partial class PurchaseReportDetailPanel : ComponentBase
{
    [Parameter] public PurchaseReportLine? Item { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public int WindowDays { get; set; } = 45;

    protected override void OnParametersSet()
    {
        // Si hay lógica que depende de cambios en Item
    }
}
```

#### StateChanged event con inconsistencia en invocación
- **Archivo:** `PurchaseReportManager.ViewModels/PurchaseReportViewModel.cs:86-90, 138`
- **Problema:** El ViewModel declara `event Action? StateChanged;` y lo invoca con `StateChanged?.Invoke();` en algunos métodos (como en `IsLoading` setter), pero en otros métodos no lo invoca. Por ejemplo, `SearchText`, `OnlyWithSuggestedPurchase`, etc. son simples properties sin notificación. Esto puede causar que la UI no se actualice en algunos casos.
- **Corrección:** O bien (a) hacer que todas las propiedades de estado invoquen `StateChanged`, o (b) usar un patrón más consistente como `StateHasChanged()` en la Vista (que ya se hace).

---

## �️ Clases a Eliminar

| Archivo | Razón |
|---------|-------|
| `PurchaseReportManager.Proxy/Class1.cs` | Plantilla vacía de proyecto, sin uso |
| `PurchaseReportManager.ViewModels/Class1.cs` | Plantilla vacía de proyecto, sin uso |
| `PurchaseReportManager.Views/Class1.cs` | Plantilla vacía de proyecto, sin uso |
| `Common.Views/Class1.cs` | Plantilla vacía de proyecto, sin uso |
| `Common.Domain/Class1.cs` | Plantilla vacía de proyecto, sin uso |

Estas clases son generadas automáticamente por el template de VS Code al crear proyectos. Deben eliminarse para mantener el código limpio.


*Generado con /blazor-review — Basado en PurchaseReportManager feature y convenciones del proyecto*


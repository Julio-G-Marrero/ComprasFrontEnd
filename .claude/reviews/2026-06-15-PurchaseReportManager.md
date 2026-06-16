# Code Review — PurchaseReportManager — 2026-06-15

## Resumen Ejecutivo

El feature `PurchaseReportManager` muestra una **mejora significativa respecto al review anterior**: se resolvieron todos los bugs críticos de la sesión del 12/06 (primary constructor, headers HTTP, adapter plural, DependencyContainer en Views, logs en inglés). El código compila limpio y la arquitectura vertical slice está en su lugar. Quedan **tres categorías de deuda**: identi­ficadores en español dispersos en Views, uso de `RenderTreeBuilder` imperativo en code-behind (anti-patrón Blazor), y ausencia de la capa `Models/` en ViewModels que coloca propiedades de UI en la entidad Domain.

---

## 🔴 Bugs Críticos

### `EventCallback.Factory.Create` con receiver incorrecto en `SortHeader`

- **Archivo:** `PurchaseReportManager.Views/Components/PurchaseReportTable.razor.cs:22`
- **Descripción:** `EventCallback.Factory.Create(__builder, ...)` pasa un `RenderTreeBuilder` como receiver. El primer parámetro debe ser el componente (`ComponentBase`) para que Blazor sepa a quién llamar `StateHasChanged` cuando el callback complete. Pasar `__builder` impide que Blazor cierre el ciclo de re-render correctamente para este callback. Funciona en la práctica solo porque `HandleSort` dispara `OnPageChanged` que llega al padre, pero el contrato de `EventCallback` queda roto.
- **Corrección sugerida:**
```csharp
private RenderFragment SortHeader(string label, string col, string thClass) => __builder =>
{
    // ...
    __builder.AddAttribute(2, "onclick",
        EventCallback.Factory.Create(this, async () => await HandleSort(col)));  // this, no __builder
    // ...
};
```

---

## 🟡 Mejoras

### Arquitectura

#### Capa `Models/` ausente en ViewModels — propiedades de UI en Domain

- **Archivo:** `PurchaseReportManager.ViewModels/` (falta carpeta `Models/`) y `Common/Domain/PurchaseReportLine.cs:46-47`
- **Problema:** `PurchaseReportLine.IsCritical` y `PurchaseReportLine.HasSuggestion` son propiedades de presentación que viven en la entidad Domain. Domain no debería saber que "CRITICO" es una regla de color de badge ni que `SuggestedQuantity > 0` implica mostrar un badge. La convención exige una clase `PurchaseReportLineModel` en `ViewModels/Models/` con estas computed properties, adaptada desde `PurchaseReportLine`.
- **Corrección:**
```csharp
// PurchaseReportManager.ViewModels/Models/PurchaseReportLineModel.cs
public sealed class PurchaseReportLineModel
{
    public PurchaseReportLine Data { get; init; } = default!;
    public bool HasSuggestion => Data.SuggestedQuantity > 0;
    public bool IsCritical => Data.AlertLevel.Equals("CRITICO", StringComparison.OrdinalIgnoreCase);
}
```
Y el adapter correspondiente en `ViewModels/Adapters/PurchaseReportLineModelAdapter.cs`.

#### `IsLoading` e `IsLoadingCatalog` expuestos en el ViewModel

- **Archivo:** `PurchaseReportManager.ViewModels/Abstractions/IPurchaseReportViewModel.cs:32-33`
- **Problema:** La convención establece que propiedades de estado de UI (`IsLoading`, `IsLoadingCatalog`) son responsabilidad de la Vista. Al exponerlos en la interfaz del ViewModel, la Vista delega al ViewModel la decisión de cuándo mostrar un spinner.
- **Corrección:** Mover las flags de loading a la Vista (`PurchaseReportPage.razor.cs`), suscribiéndose a eventos del ViewModel para activarlas, o alternativamente redefinir la convención del proyecto explícitamente en CLAUDE.md si el patrón actual es intencional.

---

### Convenciones de Código

#### Nombres de métodos en español en Views (múltiples archivos)

- **Archivos y líneas:**
  - `PurchaseReportFilters.razor.cs:22` — `HandleFamiliaChanged`
  - `PurchaseReportSummaryCards.razor.cs:12,18,24` — `HandleToggleSugerida`, `HandleToggleCriticos`, `HandleToggleRevision`
  - `PurchaseReportTable.razor.cs:77-78` — `InventarioBadgeClass`, `InventarioLabel`
  - `PurchaseReportDetailPanel.razor.cs:14` — `InventarioLabel`
- **Correcciones:**

| Actual | Correcto |
|--------|----------|
| `HandleFamiliaChanged` | `HandleFamilyChanged` |
| `HandleToggleSugerida` | `HandleToggleSuggested` |
| `HandleToggleCriticos` | `HandleToggleCritical` |
| `HandleToggleRevision` | `HandleToggleReview` |
| `InventarioBadgeClass` | `InventoryBadgeClass` |
| `InventarioLabel` | `InventoryLabel` |

#### Parámetros en español (múltiples archivos)

- **Archivos:**
  - `PurchaseReportViewModel.cs:174` — `AlertOrder(string nivel)` → `AlertOrder(string level)`
  - `PurchaseReportTable.razor.cs:62-63,77-78` — `AlertBadgeClass(string nivel)`, `AlertDisplayText(string nivel)`, parámetro `estado`
  - `PurchaseReportDetailPanel.razor.cs:14` — `InventarioLabel(string estado)`
  - `Common.Views/Helpers/InventoryDisplayHelper.cs:5,14` — `Label(string estado)`, `BadgeClass(string estado)`

Todos los `estado` → `status`, todos los `nivel` → `level`.

#### Campo `_fecha` en español

- **Archivo:** `PurchaseReportManager.Views/Components/PurchaseSuggestionPrintView.razor.cs:13`
- **Corrección:** `_fecha` → `_printDate`

#### Mensaje de excepción en español en DependencyContainer

- **Archivo:** `PurchaseReportManager.Proxy/DependencyContainer.cs:20`
- **Problema:** `"ApiSettings:BaseUrl no está configurado."` — mensaje para desarrollador, debe ser en inglés.
- **Corrección:** `"ApiSettings:BaseUrl is not configured."`

---

### Buenas Prácticas .NET/Blazor

#### `RenderFragment` con `RenderTreeBuilder` imperativo en code-behind

- **Archivos:**
  - `PurchaseReportTable.razor.cs:15-28` — `SortHeader` construye HTML con `__builder`
  - `PurchaseReportDetailPanel.razor.cs:16-29` — `Row` construye HTML con `__builder`
- **Problema:** La API `RenderTreeBuilder` está pensada para generación de código — el compilador de Razor la usa internamente. Usarla manualmente en code-behind mezcla lógica de presentación con lógica de interacción, es frágil ante cambios de la API, y dificulta la lectura.
- **Corrección para `Row` (caso sencillo):**
```razor
{{!-- En PurchaseReportDetailPanel.razor, en lugar de @Row(...): --}}
<div class="d-flex justify-content-between border-bottom py-1 small">
    <span class="text-muted">@label</span>
    <span class="fw-semibold text-end">@value</span>
</div>
```
- **Corrección para `SortHeader`:** Mover el markup al `.razor` y usar un método que solo devuelva las clases/iconos.

#### `try-catch` ausente en `OnInitializedAsync`

- **Archivo:** `PurchaseReportManager.Views/Pages/PurchaseReportPage.razor.cs:19-24`
- **Problema:** Si `InitializeViewModel()` lanzara una excepción no controlada, se propaga al framework y el componente no renderiza.
- **Corrección:**
```csharp
protected override async Task OnInitializedAsync()
{
    Vm.StateChanged += HandleStateChanged;
    Vm.OnFailure += HandleFailure;
    try
    {
        await Vm.InitializeViewModel();
    }
    catch (Exception ex)
    {
        _ = ex;
    }
}
```

#### `PurchaseReportPage` no usa primary constructor

- **Archivo:** `PurchaseReportManager.Views/Pages/PurchaseReportPage.razor.cs:8-11`
- **Problema:** Usa `[Inject]` con `default!`; la convención establece primary constructor para inyección en Views.
- **Corrección:**
```csharp
public sealed partial class PurchaseReportPage(
    IPurchaseReportViewModel vm,
    IJSRuntime js) : IDisposable
{
    // Sin [Inject], sin default!
}
```

#### `style=""` inline en componentes (excluye `PurchaseSuggestionPrintView`)

- **Archivos:**
  - `PurchaseReportSummaryCards.razor:14,32,46` — `style="cursor:pointer;..."` y colores de fondo dinámicos
  - `PurchaseReportTable.razor:57,59,26` — `style="cursor:pointer"`, `style="max-width:200px;overflow:hidden;text-overflow:ellipsis"`, `style="opacity:...;font-size:..."`
  - `PurchaseReportDetailPanel.razor:6` — `style="max-height:calc(100vh - 260px)"`
  - `PurchaseReportParamsPanel.razor:3` — `style="cursor:pointer"`
- **Corrección:** Mover a clases CSS en un archivo `.razor.css` de alcance del componente o en el stylesheet global.

#### SVG inline en lugar de Bootstrap Icons

- **Archivo:** `PurchaseReportManager.Views/Components/PurchaseReportTable.razor:2-7`
- **Corrección:** `<i class="bi bi-search"></i>` (requiere Bootstrap Icons referenciado en el proyecto).

#### `StateHasChanged()` redundante en `HandleLoad`

- **Archivo:** `PurchaseReportManager.Views/Pages/PurchaseReportPage.razor.cs:38`
- **Problema:** Después de `await Vm.LoadAsync()`, Blazor ya llama `StateHasChanged` automáticamente en event handlers. La llamada explícita es redundante.

---

## 🟢 Bien aplicado

- **Primary constructor en Proxy y ViewModel:** Sin campos privados redundantes; `options.Value.ApplicationId` accedido directamente en `TenantRequest()`.
- **Headers HTTP dinámicos:** `TenantRequest()` agrega `X-Tenant-Id` y `X-Application-Id` en todos los métodos que los requieren.
- **Logging 100% en inglés y estructurado:** Todos los `logger.LogError/LogInformation` usan template con `{Param}` sin interpolación de strings.
- **`HandlerRequestResult<T>` consistente:** Single return, nunca se propagan excepciones; todos los métodos del Proxy siguen el mismo patrón.
- **IDisposable correcto en PurchaseReportPage:** Suscripción en `OnInitializedAsync` y des-suscripción de `StateChanged` y `OnFailure` en `Dispose()`.
- **`ToModel` y `ToModels` en el adapter:** Patrón plural con `.Select().ToList()` correctamente implementado.
- **net10.0 en todos los .csproj** y paquetes alineados a la misma versión `10.0.8`.
- **`PurchaseReportParamsPanel` con estado local desacoplado:** Copia local de parámetros antes de aplicar al ViewModel, patrón correcto para paneles de edición.
- **`LegacyTextNormalizer`:** Documentado con comentario de por qué existe, sin lógica compleja, fácil de extender.

---

## Checklist de Conformidad

| Regla | Estado | Nota |
|---|---|---|
| Tres proyectos por feature | ✅ | Proxy, ViewModels, Views |
| DependencyContainer en Proxies y ViewModels | ✅ | Ambos presentes y funcionales |
| Interfaces en Abstractions/ | ✅ | `IPurchaseReportProxy` e `IPurchaseReportViewModel` |
| Primary constructors en Proxies | ✅ | Sin campos privados redundantes |
| try-catch en Proxies con LogError | ✅ | Todos los métodos públicos cubiertos |
| Retorno HandlerRequestResult<T> en Proxies | ✅ | Nunca se propagan excepciones |
| OnFailure event en ViewModels | ✅ | Declarado, invocado y suscrito en la Vista |
| InitializeViewModel() en ViewModels | ✅ | Presente y llamado en OnInitializedAsync |
| Sin DTOs expuestos en ViewModels | ✅ | Solo tipos de Domain expuestos |
| Code-behind obligatorio (sin @code{}) | ✅ | Todos los componentes con .razor.cs |
| IDisposable + des-suscripción OnFailure | ✅ | Solo en PurchaseReportPage (correcto) |
| Bootstrap exclusivamente en Views | ⚠️ | Inline styles en 4 componentes; SVG inline en tabla |
| Primary constructor en Views | ❌ | PurchaseReportPage usa [Inject] + default! |
| try-catch en OnInitializedAsync | ❌ | PurchaseReportPage sin try-catch |
| PascalCase / camelCase correcto | ⚠️ | Identificadores en español en Views y Common.Views |
| Single return por método | ✅ | Todos los métodos con single return |
| Adapters en ViewModels | ❌ | Adapter en Proxy, no en ViewModels |
| Models en ViewModels | ❌ | Sin carpeta Models/ |
| Sin IsLoading en ViewModel | ❌ | IsLoading/IsLoadingCatalog expuestos en interfaz |
| Logging estructurado (sin interpolación) | ✅ | Todo correcto en Proxy |
| net10.0 en todos los .csproj | ✅ | |
| Identificadores en inglés | ⚠️ | Varios métodos y parámetros en español en Views |

**Leyenda:** ✅ Cumple · ❌ No cumple · ⚠️ Cumple parcialmente

---

*Generado con /blazor-review — Basado en PurchaseReportManager feature y convenciones del proyecto*

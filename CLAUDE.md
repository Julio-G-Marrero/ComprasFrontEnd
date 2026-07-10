# Client Features — Blazor WebAssembly

Arquitectura: **MVVM + Vertical Slice** con tres capas por feature.

## Estructura de Carpetas

Cada feature tiene **TRES proyectos**:

```
Features/[FeatureName]/
  [FeatureName].Proxies/          # Comunicación HTTP con el API
    Abstractions/
      I[Entity]Proxy.cs
    [Entity]Proxy.cs
    DependencyContainer.cs
    [FeatureName].Proxies.csproj

  [FeatureName].ViewModels/       # Lógica de UI y estado
    Abstractions/
      I[Entity]ViewModel.cs
    ViewModels/
      [Entity]ViewModel.cs
    Models/
      [Entity]Model.cs
    Adapters/
      [Entity]Adapter.cs
    Helpers/                      # Utilidades opcionales
    DependencyContainer.cs
    [FeatureName].ViewModels.csproj

  [FeatureName].Views/            # Componentes Razor
    Pages/
      [Page].razor
      [Page].razor.cs
    Components/
      [Component].razor
      [Component].razor.cs
    _Imports.razor
    [FeatureName].Views.csproj
```

## Flujo de Datos

```
[Page].razor
  ↓
[Page].razor.cs  (code-behind, inyecta IViewModel)
  ↓
[Entity]ViewModel  (lógica, estado, paginación)
  ↓
[Entity]Proxy  (HttpClient → API)
  ↓
HandlerRequestResult<T>
  ↓
[Entity]Adapter.To[Entity]Model()
  ↓
Render en componente
```

## Referencias entre proyectos

```
Views   → ViewModels → Proxies → Domain (DTOs)
Views   → Common.Views (componentes compartidos)
```

## Proxies — Comunicación HTTP

- Siempre usar primary constructor: `(HttpClient httpClient, ILogger<T> logger)`
- Devolver `HandlerRequestResult<T>` — nunca lanzar excepciones hacia arriba
- Try-catch en cada método del proxy
- Logging en catch con `logger.LogError`
- Headers dinámicos: `X-Tenant-Id`, `X-Application-Id` cuando aplique

```csharp
namespace [Feature].Proxies;

internal class [Entity]Proxy(HttpClient httpClient, ILogger<[Entity]Proxy> logger) : I[Entity]Proxy
{
    public async Task<HandlerRequestResult<List<[Entity]Dto>>> Get[Entities]Async(string tenantId)
    {
        HandlerRequestResult<List<[Entity]Dto>> result = new();
        try
        {
            httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
            httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
            var response = await httpClient.GetAsync("api/[resource]");
            result = await response.Content.ReadFromJsonAsync<HandlerRequestResult<List<[Entity]Dto>>>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting [entities]");
        }
        return result;
    }
}
```

### DependencyContainer (Proxies)

```csharp
namespace [Feature].Proxies;

public static class DependencyContainer
{
    public static IServiceCollection Add[Feature]Proxies(
        this IServiceCollection services,
        Action<HttpClient> configureProxy,
        Func<IServiceProvider, DelegatingHandler> configureHandler)
    {
        services.AddHttpClient<I[Entity]Proxy, [Entity]Proxy>(configureProxy)
                .AddHttpMessageHandler(configureHandler);
        return services;
    }
}
```

## ViewModels — Lógica y Estado

- Primary constructor para inyectar proxies y logger
- Clase `internal`, interfaz `public`
- Propiedades con `private set` — la UI solo lee, no escribe directamente
- Evento `OnFailure` para comunicar errores a la View
- `InitializeViewModel()` para carga inicial
- Paginación con `CurrentPage`, `ItemsPerPage`, `TotalPages`

```csharp
namespace [Feature].ViewModels;

internal class [Entity]ViewModel(
    I[Entity]Proxy proxy,
    ILogger<[Entity]ViewModel> logger) : I[Entity]ViewModel
{
    public List<[Entity]Model> Items { get; private set; } = [];
    public int CurrentPage { get; private set; } = 1;
    public int ItemsPerPage { get; private set; } = 10;
    public int TotalPages { get; private set; }
    public string Filter { get; set; } = string.Empty;

    public event EventHandler<string> OnFailure;

    public async Task InitializeViewModel()
    {
        await Load[Entities]Async(CurrentPage, ItemsPerPage);
    }

    public async Task Load[Entities]Async(int currentPage, int itemsPerPage)
    {
        var result = await proxy.Get[Entities]Async();
        if (result.Success)
        {
            Items = result.Data.[Entity]Adapter.To[Entity]Models();
        }
        else
        {
            OnFailure?.Invoke(this, result.ErrorMessage);
        }
    }

    public async Task PageChange(int newPage)
    {
        CurrentPage = newPage;
        await Load[Entities]Async(CurrentPage, ItemsPerPage);
    }

    public async Task PageSizeItemsChange(int newSize)
    {
        ItemsPerPage = newSize;
        CurrentPage = 1;
        await Load[Entities]Async(CurrentPage, ItemsPerPage);
    }
}
```

### DependencyContainer (ViewModels)

```csharp
namespace [Feature].ViewModels;

public static class DependencyContainer
{
    public static IServiceCollection Add[Feature]ViewModels(this IServiceCollection services)
    {
        services.AddScoped<I[Entity]ViewModel, [Entity]ViewModel>();
        return services;
    }
}
```

## Models — DTOs Locales de UI

- Propiedades simples para la UI (no los DTOs del API directamente)
- Computed properties para derivar datos de display

```csharp
namespace [Feature].ViewModels.Models;

public class [Entity]Model
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string DisplayStatus => Status == 1 ? "Activo" : "Inactivo"; // computed
}
```

## Adapters — Mapeo DTO → Model

```csharp
namespace [Feature].ViewModels.Adapters;

internal static class [Entity]Adapter
{
    internal static [Entity]Model To[Entity]Model(this [Entity]Dto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
    };

    internal static List<[Entity]Model> To[Entity]Models(this IEnumerable<[Entity]Dto> dtos) =>
        dtos.Select(d => d.To[Entity]Model()).ToList();
}
```

## Views — Code-Behind (.razor.cs)

- Primary constructor: inyectar `IViewModel` y `NavigationManager` si es necesario
- Suscribir a `OnFailure` en `OnInitializedAsync`, desuscribir en `Dispose`
- Implementar `IDisposable`
- No lógica de negocio — solo binding y navegación

```csharp
namespace [Feature].Views.Pages;

public partial class [Page](
    I[Entity]ViewModel viewModel,
    NavigationManager navigationManager) : IDisposable
{
    private I[Entity]ViewModel ViewModel => viewModel;
    private string ErrorMessage { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        viewModel.OnFailure += HandleFailure;
        await viewModel.InitializeViewModel();
    }

    private void HandleFailure(object? sender, string errorMessage)
    {
        ErrorMessage = errorMessage;
        StateHasChanged();
    }

    public void Dispose()
    {
        viewModel.OnFailure -= HandleFailure;
    }
}
```

## Views — Razor (.razor)

```razor
@page "/[route]"
@attribute [Authorize]

@if (!string.IsNullOrEmpty(ErrorMessage))
{
    <div class="alert alert-danger">@ErrorMessage</div>
}

@foreach (var item in ViewModel.Items)
{
    <div>@item.Name</div>
}
```

## Filosofía: Blazor primero

Antes de implementar cualquier comportamiento en una View, preguntarse:

> **¿Blazor ya resuelve esto de forma nativa?**

Consultar siempre qué ofrece el framework antes de escribir código manual. Blazor tiene soluciones built-in para la mayoría de los casos comunes — usarlas evita bugs sutiles (como el de cultura/locale en inputs numéricos) y reduce código innecesario.

Ejemplos de lo que Blazor ya resuelve y no se debe reimplementar a mano:

| Necesidad | Solución Blazor | No hacer |
|---|---|---|
| Bindear un input | `@bind="prop"` | `value=` + `@onchange=` manual |
| Validación de formulario | `<EditForm>` + DataAnnotations | Validación manual en code-behind |
| Navegación | `NavigationManager.NavigateTo()` | JS interop para navegar |
| Parámetros de ruta | `[Parameter]` + `@page "/{Id:int}"` | Parsear la URL manualmente |
| Ciclo de vida | `OnInitializedAsync`, `OnParametersSetAsync` | Timers o hacks equivalentes |
| Re-render | `StateHasChanged()` | Forzar re-renders desde JS |
| Eventos entre componentes | `EventCallback<T>` | Servicios singleton como bus de eventos |

## Directivas Blazor — Reglas de binding

### Usar siempre `@bind` en lugar de `value=` + `@onchange=`

Blazor maneja la cultura internamente con `@bind`. Para tipos numéricos (`decimal`, `int`, `double`) usa `CultureInfo.InvariantCulture` al escribir el atributo `value`, lo que evita que `input[type=number]` muestre vacío en navegadores con locale de coma decimal (es-MX, es-ES, etc.) — problema que ocurre especialmente en builds Release / Docker.

**Incorrecto:**
```razor
<input type="number" value="@model.Amount"
       @onchange="e => model.Amount = decimal.TryParse(e.Value?.ToString(), out decimal v) ? v : 0" />

<input type="text" value="@model.Name"
       @onchange="e => model.Name = e.Value?.ToString() ?? string.Empty" />

<input type="checkbox" checked="@model.Active"
       @onchange="e => model.Active = (bool)e.Value" />

<textarea value="@model.Notes"
          @onchange="e => model.Notes = e.Value?.ToString() ?? string.Empty"></textarea>
```

**Correcto:**
```razor
<input type="number" @bind="model.Amount" />
<input type="text"   @bind="model.Name" />
<input type="checkbox" @bind="model.Active" />
<textarea @bind="model.Notes"></textarea>
```

- `@bind` usa `onchange` por defecto (al perder el foco). Para actualizar mientras el usuario escribe usar `@bind:event="oninput"`.
- `<select>` ya usa `@bind` — no cambia.
- Inputs de **solo lectura** (`disabled`, vista autorizada) pueden seguir usando `value="@prop"` ya que no hay interacción del usuario.

## .csproj por capa

Las versiones de paquetes se gestionan centralmente en `Directory.Packages.props` — no agregar `Version` en los `PackageReference`.

**Proxies:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Http" />
    <ProjectReference Include="..\..\..\..\Domain\Domain.csproj" />
  </ItemGroup>
</Project>
```

**ViewModels:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\[Feature].Proxies\[Feature].Proxies.csproj" />
  </ItemGroup>
</Project>
```

**Views:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" />
    <ProjectReference Include="..\[Feature].ViewModels\[Feature].ViewModels.csproj" />
    <ProjectReference Include="..\..\Common\Common.Views\Common.Views.csproj" />
  </ItemGroup>
</Project>
```


## Checklist al crear un Feature

- [ ] Tres proyectos: Proxies, ViewModels, Views
- [ ] `DependencyContainer.cs` en Proxies y ViewModels
- [ ] Interfaces en `Abstractions/` para Proxies y ViewModels
- [ ] Proxies con primary constructor y try-catch en cada método
- [ ] ViewModels con `OnFailure` event y `InitializeViewModel()`
- [ ] Adapters para mapear DTOs → Models
- [ ] Code-behind implementa `IDisposable` y desuscribe eventos
- [ ] Registrar en `Program.cs` del proyecto web (ViewModels + Proxies)
- [ ] Agregar referencia a `[Feature].Views.csproj` en el proyecto web

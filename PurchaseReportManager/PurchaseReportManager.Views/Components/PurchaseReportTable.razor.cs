using Common.Views.Helpers;
using Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using PurchaseReportManager.ViewModels.Abstractions;

namespace PurchaseReportManager.Views.Components;

public partial class PurchaseReportTable
{
    [Parameter] public IPurchaseReportViewModel Vm { get; set; } = default!;
    [Parameter] public EventCallback<PurchaseReportLine> OnRowClick { get; set; }
    [Parameter] public EventCallback OnPageChanged { get; set; }

    private RenderFragment SortHeader(string label, string col, string thClass) => __builder =>
    {
        var isActive = Vm.SortColumn == col;
        var icon = isActive ? (Vm.SortAscending ? "↑" : "↓") : "⇅";
        var iconOpacity = isActive ? "1" : "0.35";
        var extraClass = string.IsNullOrEmpty(thClass) ? "" : $" {thClass}";
        __builder.OpenElement(0, "th");
        __builder.AddAttribute(1, "class", $"sort-header{extraClass}");
        __builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(__builder, async () => await HandleSort(col)));
        __builder.AddAttribute(3, "title", $"Ordenar por {label}");
        __builder.AddContent(4, label);
        __builder.AddMarkupContent(5, $" <span style=\"opacity:{iconOpacity};font-size:0.75em\">{icon}</span>");
        __builder.CloseElement();
    };

    private async Task HandleSort(string col)
    {
        Vm.SetSort(col);
        await OnPageChanged.InvokeAsync();
    }

    private async Task HandleSearch(ChangeEventArgs e)
    {
        Vm.SearchText = e.Value?.ToString() ?? string.Empty;
        Vm.ResetPagination();
        await OnPageChanged.InvokeAsync();
    }

    private async Task HandleClearSearch()
    {
        Vm.SearchText = string.Empty;
        Vm.ResetPagination();
        await OnPageChanged.InvokeAsync();
    }

    private async Task HandlePrevious()
    {
        Vm.GoToPreviousPage();
        await OnPageChanged.InvokeAsync();
    }

    private async Task HandleNext()
    {
        Vm.GoToNextPage();
        await OnPageChanged.InvokeAsync();
    }

    private static string AlertBadgeClass(string nivel) => nivel.ToUpperInvariant() switch
    {
        "CRITICO" => "badge bg-danger",
        "ALTO"    => "badge bg-warning",
        "MEDIO"   => "badge bg-info",
        "BAJO"    => "text-muted small",
        _         => "badge bg-secondary"
    };

    private static string AlertDisplayText(string nivel) => nivel.ToUpperInvariant() switch
    {
        "BAJO" => "N/A",
        _      => nivel
    };

    private static string InventarioBadgeClass(string estado) => InventoryDisplayHelper.BadgeClass(estado);
    private static string InventarioLabel(string estado)     => InventoryDisplayHelper.Label(estado);
}

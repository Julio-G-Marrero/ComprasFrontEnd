using Common.Views.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using PurchaseReportManager.ViewModels.Abstractions;
using PurchaseReportManager.ViewModels.Models;

namespace PurchaseReportManager.Views.Components;

public partial class PurchaseReportTable
{
    [Parameter] public IPurchaseReportViewModel Vm { get; set; } = default!;
    [Parameter] public EventCallback<PurchaseReportLineModel> OnRowClick { get; set; }
    [Parameter] public EventCallback OnPageChanged { get; set; }

    private static readonly (string Value, string Label)[] AlertLevelOptions =
    [
        ("CRITICO", "Crítico"),
        ("ALTO", "Alto"),
        ("MEDIO", "Medio"),
        ("BAJO", "Bajo"),
    ];

    private static readonly (string Value, string Label)[] InventoryStatusOptions =
    [
        ("SALUDABLE", InventoryDisplayHelper.Label("SALUDABLE")),
        ("BAJO_OBJETIVO", InventoryDisplayHelper.Label("BAJO_OBJETIVO")),
        ("SOBRESTOCK", InventoryDisplayHelper.Label("SOBRESTOCK")),
        ("SIN_VENTA_CON_STOCK", InventoryDisplayHelper.Label("SIN_VENTA_CON_STOCK")),
    ];

    private async Task HandleFilterChanged()
    {
        Vm.ResetPagination();
        await OnPageChanged.InvokeAsync();
    }

    private RenderFragment SortHeader(string label, string col, string thClass) => __builder =>
    {
        var isActive = Vm.SortColumn == col;
        var icon = isActive ? (Vm.SortAscending ? "↑" : "↓") : "⇅";
        var iconOpacity = isActive ? "1" : "0.35";
        var extraClass = string.IsNullOrEmpty(thClass) ? "" : $" {thClass}";
        __builder.OpenElement(0, "th");
        __builder.AddAttribute(1, "class", $"sort-header{extraClass}");
        __builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, async () => await HandleSort(col)));
        __builder.AddAttribute(3, "title", $"Ordenar por {label}");
        __builder.AddContent(4, label);
        __builder.AddMarkupContent(5, $" <span style=\"opacity:{iconOpacity};font-size:0.75em\">{icon}</span><span class=\"col-resize-handle\"></span>");
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

    private static string InventarioBadgeClass(string estado) => InventoryDisplayHelper.BadgeClass(estado);
    private static string InventarioLabel(string estado)     => InventoryDisplayHelper.Label(estado);
}

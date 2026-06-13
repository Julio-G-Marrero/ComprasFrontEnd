using Common.Views.Helpers;
using Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace PurchaseReportManager.Views.Components;

public partial class PurchaseReportDetailPanel
{
    [Parameter] public PurchaseReportLine? Item { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public int WindowDays { get; set; } = 45;

    private static string InventarioLabel(string estado) => InventoryDisplayHelper.Label(estado);

    private static RenderFragment Row(string label, string value) => __builder =>
    {
        __builder.OpenElement(0, "div");
        __builder.AddAttribute(1, "class", "d-flex justify-content-between border-bottom py-1 small");
        __builder.OpenElement(2, "span");
        __builder.AddAttribute(3, "class", "text-muted");
        __builder.AddContent(4, label);
        __builder.CloseElement();
        __builder.OpenElement(5, "span");
        __builder.AddAttribute(6, "class", "fw-semibold text-end");
        __builder.AddContent(7, value);
        __builder.CloseElement();
        __builder.CloseElement();
    };
}

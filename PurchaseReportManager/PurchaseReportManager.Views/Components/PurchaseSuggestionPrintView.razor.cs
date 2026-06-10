using Domain;
using Microsoft.AspNetCore.Components;

namespace PurchaseReportManager.Views.Components;

public partial class PurchaseSuggestionPrintView
{
    [Parameter] public IReadOnlyList<PurchaseReportLine> Items { get; set; } = [];
    [Parameter] public string SucursalName { get; set; } = string.Empty;
    [Parameter] public string Familia { get; set; } = string.Empty;
    [Parameter] public string Subfamilia { get; set; } = string.Empty;

    private readonly string _fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
}

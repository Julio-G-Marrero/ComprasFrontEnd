using Microsoft.AspNetCore.Components;
using PurchaseReportManager.ViewModels.Models;

namespace PurchaseReportManager.Views.Components;

public partial class PurchaseSuggestionPrintView
{
    [Parameter] public IReadOnlyList<PurchaseReportLineModel> Items { get; set; } = [];
    [Parameter] public string BranchName { get; set; } = string.Empty;
    [Parameter] public string Family { get; set; } = string.Empty;
    [Parameter] public string SubFamily { get; set; } = string.Empty;

    private readonly string _fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
}

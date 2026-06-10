using Microsoft.AspNetCore.Components;
using PurchaseReportManager.ViewModels;

namespace PurchaseReportManager.Views.Components;

public partial class PurchaseReportSummaryCards
{
    [Parameter] public IPurchaseReportViewModel Vm { get; set; } = default!;
    [Parameter] public EventCallback OnFilterChanged { get; set; }

    private async Task HandleToggleSugerida()
    {
        Vm.ToggleOnlyWithSuggestedPurchase();
        await OnFilterChanged.InvokeAsync();
    }

    private async Task HandleToggleCriticos()
    {
        Vm.ToggleOnlyCritical();
        await OnFilterChanged.InvokeAsync();
    }

    private async Task HandleToggleRevision()
    {
        Vm.ToggleOnlyRequiresReview();
        await OnFilterChanged.InvokeAsync();
    }

    private static string CardClass(bool active, string color) =>
        $"card text-center h-100 border-{color}{(active ? " border-2 shadow-sm" : "")}";

    private static string CardStyle(bool active, string bg) =>
        active ? $"cursor:pointer;background-color:{bg};transition:all .15s"
               : "cursor:pointer;transition:all .15s";
}

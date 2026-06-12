using Microsoft.AspNetCore.Components;
using PurchaseReportManager.ViewModels.Abstractions;

namespace PurchaseReportManager.Views.Components;

public partial class PurchaseReportFilters
{
    [Parameter] public IPurchaseReportViewModel Vm { get; set; } = default!;
    [Parameter] public EventCallback OnLoad { get; set; }
    [Parameter] public EventCallback OnClearFilters { get; set; }
    [Parameter] public EventCallback OnFilterChanged { get; set; }

    private async Task HandleTenantChanged()
    {
        await Vm.OnTenantChangedAsync();
        await OnFilterChanged.InvokeAsync();
    }

    private async Task HandleFamiliaChanged()
    {
        await Vm.OnFamilyChangedAsync();
        await OnFilterChanged.InvokeAsync();
    }

    private async Task NotifyChanged()
    {
        Vm.ResetPagination();
        await OnFilterChanged.InvokeAsync();
    }
}

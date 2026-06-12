using Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PurchaseReportManager.ViewModels.Abstractions;

namespace PurchaseReportManager.Views.Pages;

public sealed partial class PurchaseReportPage : IDisposable
{
    [Inject] private IPurchaseReportViewModel Vm { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IReadOnlyList<PurchaseReportLine> PrintItems =>
        Vm.FilteredItems.Where(x => x.SuggestedQuantity > 0 || x.SuggestedPackages > 0).ToList().AsReadOnly();

    private string TenantDisplayName =>
        Vm.Tenants.FirstOrDefault(t => t.Id == Vm.SelectedTenantId)?.Name ?? Vm.SelectedTenantId;

    protected override async Task OnInitializedAsync()
    {
        Vm.StateChanged += HandleStateChanged;
        Vm.OnFailure += HandleFailure;
        await Vm.InitializeAsync();
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async void HandleFailure(string message) =>
        await InvokeAsync(StateHasChanged);

    private async Task HandleLoad()
    {
        await Vm.LoadAsync();
        StateHasChanged();
    }

    private void HandleClearFilters()
    {
        Vm.ClearFilters();
        StateHasChanged();
    }

    private async Task HandlePrint()
    {
        if (PrintItems.Count == 0) return;
        await JS.InvokeVoidAsync("print");
    }

    private void HandleRowClick(PurchaseReportLine item) => Vm.SelectItem(item);
    private void HandleCloseDetail() => Vm.ClearSelection();

    public void Dispose()
    {
        Vm.StateChanged -= HandleStateChanged;
        Vm.OnFailure -= HandleFailure;
    }
}

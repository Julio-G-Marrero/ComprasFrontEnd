using Microsoft.AspNetCore.Components;
using PurchaseReportManager.ViewModels.Abstractions;

namespace PurchaseReportManager.Views.Components;

public partial class PurchaseReportParamsPanel
{
    [Parameter] public IPurchaseReportViewModel Vm { get; set; } = default!;
    [Parameter] public EventCallback OnApply { get; set; }

    private bool _expanded;
    private int _windowDays;
    private int _reviewFrequencyDays;
    private decimal _serviceLevelPercent;
    private int _defaultSupplierDays;
    private decimal _minOperationalStock;
    private int _xyzTolerancePreset;

    private static readonly (decimal X, decimal Y)[] XyzPresets =
    [
        (0.50m, 1.00m), (0.75m, 1.50m), (1.00m, 2.00m), (1.50m, 3.00m), (2.00m, 4.00m),
    ];

    private decimal XyzX => XyzPresets[_xyzTolerancePreset - 1].X;
    private decimal XyzY => XyzPresets[_xyzTolerancePreset - 1].Y;

    protected override void OnInitialized() => SyncFromVm();

    private void SyncFromVm()
    {
        _windowDays = Vm.WindowDays;
        _reviewFrequencyDays = Vm.ReviewFrequencyDays;
        _serviceLevelPercent = Vm.ServiceLevelPercent;
        _defaultSupplierDays = Vm.DefaultSupplierDays;
        _minOperationalStock = Vm.MinOperationalStock;
        _xyzTolerancePreset = Vm.XyzTolerancePreset;
    }

    private void TogglePanel() => _expanded = !_expanded;

    private async Task HandleApply()
    {
        Vm.WindowDays = _windowDays;
        Vm.ReviewFrequencyDays = _reviewFrequencyDays;
        Vm.ServiceLevelPercent = _serviceLevelPercent;
        Vm.DefaultSupplierDays = _defaultSupplierDays;
        Vm.MinOperationalStock = _minOperationalStock;
        Vm.XyzTolerancePreset = _xyzTolerancePreset;
        _expanded = false;
        await OnApply.InvokeAsync();
    }

    private async Task HandleRestore()
    {
        Vm.RestoreDefaults();
        SyncFromVm();
        _expanded = false;
        if (!string.IsNullOrWhiteSpace(Vm.SelectedTenantId))
            await OnApply.InvokeAsync();
    }
}

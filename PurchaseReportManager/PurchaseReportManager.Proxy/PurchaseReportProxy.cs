using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PurchaseReportManager.Proxy.Dtos;

namespace PurchaseReportManager.Proxy;

internal sealed class PurchaseReportProxy(
    HttpClient httpClient,
    IOptions<PurchaseReportProxyOptions> options,
    ILogger<PurchaseReportProxy> logger) : IPurchaseReportProxy
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly PurchaseReportProxyOptions _options = options.Value;

    public async Task<IReadOnlyList<TenantOption>> GetTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("/api/tenants", cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content
            .ReadFromJsonAsync<List<TenantOption>>(JsonOptions, cancellationToken);
        return result?.AsReadOnly() ?? [];
    }

    public async Task<IReadOnlyList<string>> GetFamiliesAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        using var request = TenantRequest(HttpMethod.Get, "/api/catalog/families", tenantId);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content
            .ReadFromJsonAsync<List<string>>(JsonOptions, cancellationToken);
        return result?.AsReadOnly() ?? [];
    }

    public async Task<IReadOnlyList<string>> GetSubfamiliesAsync(
        string tenantId,
        string familia,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/catalog/subfamilies?familia={Uri.EscapeDataString(familia)}";
        using var request = TenantRequest(HttpMethod.Get, url, tenantId);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content
            .ReadFromJsonAsync<List<string>>(JsonOptions, cancellationToken);
        return result?.AsReadOnly() ?? [];
    }

    public async Task<IReadOnlyList<PurchaseReportLine>> GetPurchaseReportAsync(
        string tenantId,
        string familia,
        string? subFamilia,
        int windowDays,
        int reviewFrequencyDays,
        decimal serviceLevel,
        int defaultSupplierDays,
        decimal minOperationalStock,
        decimal xyzXThreshold,
        decimal xyzYThreshold,
        CancellationToken cancellationToken = default)
    {
        var url = BuildReportUrl(familia, subFamilia, windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzXThreshold, xyzYThreshold);
        using var request = TenantRequest(HttpMethod.Get, url, tenantId);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content
            .ReadFromJsonAsync<ApiResponseDto<List<PurchaseReportLineDto>>>(JsonOptions, cancellationToken);

        if (apiResponse is null || !apiResponse.Success)
            throw new InvalidOperationException(
                apiResponse?.Message ?? "La API devolvió un resultado fallido.");

        if (apiResponse.Data is null or { Count: 0 })
            return [];

        var result = apiResponse.Data.Select(Map).ToList().AsReadOnly();

        if (result.Count > 0)
        {
            var f = result[0];
            logger.LogInformation(
                "PurchaseReportProxy OK — items={Count} | SKU={Sku} | Ventas45Dias={Ventas} | CantidadSugerida={Sugerida} | NivelAlerta={Alerta}",
                result.Count, f.Sku, f.Ventas45Dias, f.CantidadSugerida, f.NivelAlerta);
        }

        return result;
    }

    public async Task<IReadOnlyList<PurchaseReportLine>> GetAllPurchaseReportAsync(
        string tenantId,
        int windowDays,
        int reviewFrequencyDays,
        decimal serviceLevel,
        int defaultSupplierDays,
        decimal minOperationalStock,
        decimal xyzXThreshold,
        decimal xyzYThreshold,
        CancellationToken cancellationToken = default)
    {
        var url = BuildParamsUrl("/api/reports/purchase-v2/all", windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzXThreshold, xyzYThreshold);
        using var request = TenantRequest(HttpMethod.Get, url, tenantId);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content
            .ReadFromJsonAsync<ApiResponseDto<List<PurchaseReportLineDto>>>(JsonOptions, cancellationToken);

        if (apiResponse is null || !apiResponse.Success)
            throw new InvalidOperationException(
                apiResponse?.Message ?? "La API devolvió un resultado fallido.");

        if (apiResponse.Data is null or { Count: 0 })
            return [];

        return apiResponse.Data.Select(Map).ToList().AsReadOnly();
    }

    private HttpRequestMessage TenantRequest(HttpMethod method, string url, string tenantId)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Headers.Add("X-Application-Id", _options.ApplicationId);
        return request;
    }

    private static string BuildReportUrl(string familia, string? subFamilia,
        int windowDays, int reviewFrequencyDays, decimal serviceLevel,
        int defaultSupplierDays, decimal minOperationalStock, decimal xyzX, decimal xyzY)
    {
        var sb = new StringBuilder("/api/reports/purchase-v2?familia=")
            .Append(Uri.EscapeDataString(familia));
        if (!string.IsNullOrWhiteSpace(subFamilia))
            sb.Append("&subFamilia=").Append(Uri.EscapeDataString(subFamilia));
        AppendParams(sb, windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzX, xyzY);
        return sb.ToString();
    }

    private static string BuildParamsUrl(string baseUrl,
        int windowDays, int reviewFrequencyDays, decimal serviceLevel,
        int defaultSupplierDays, decimal minOperationalStock, decimal xyzX, decimal xyzY)
    {
        var sb = new StringBuilder(baseUrl).Append('?');
        AppendParams(sb, windowDays, reviewFrequencyDays, serviceLevel, defaultSupplierDays, minOperationalStock, xyzX, xyzY, firstParam: true);
        return sb.ToString();
    }

    private static void AppendParams(StringBuilder sb, int windowDays, int reviewFrequencyDays,
        decimal serviceLevel, int defaultSupplierDays, decimal minOperationalStock,
        decimal xyzX, decimal xyzY, bool firstParam = false)
    {
        var ic = System.Globalization.CultureInfo.InvariantCulture;
        var sep = firstParam ? string.Empty : "&";
        sb.Append(sep).Append("windowDays=").Append(windowDays)
          .Append("&reviewFrequencyDays=").Append(reviewFrequencyDays)
          .Append("&serviceLevel=").Append(serviceLevel.ToString("G", ic))
          .Append("&defaultSupplierDays=").Append(defaultSupplierDays)
          .Append("&minOperationalStock=").Append(minOperationalStock.ToString("G", ic))
          .Append("&xyzXThreshold=").Append(xyzX.ToString("G", ic))
          .Append("&xyzYThreshold=").Append(xyzY.ToString("G", ic));
    }

    private static PurchaseReportLine Map(PurchaseReportLineDto d) => new()
    {
        Sku = d.Sku,
        Descripcion = d.Descripcion,
        Familia = d.Familia,
        SubFamilia = d.SubFamilia,
        RequeridoStock = d.RequeridoStock,
        ExistenciaEfectiva = d.ExistenciaEfectiva,
        Ventas45Dias = d.Ventas45Dias,
        DemandaDiaria = d.DemandaDiaria,
        DesviacionEstandarDiaria = d.DesviacionEstandarDiaria,
        CoeficienteVariacion = d.CoeficienteVariacion,
        Abc = d.Abc,
        Xyz = d.Xyz,
        DiasProveedor = d.DiasProveedor,
        FrecuenciaRevision = d.FrecuenciaRevision,
        PeriodoProteccion = d.PeriodoProteccion,
        StockSeguridad = d.StockSeguridad,
        StockMinimoOperativo = d.StockMinimoOperativo,
        Rop = d.Rop,
        InventarioObjetivoRotacion = d.InventarioObjetivoRotacion,
        InventarioObjetivoFinal = d.InventarioObjetivoFinal,
        CantidadBruta = d.CantidadBruta,
        CantidadPorEmpaque = d.CantidadPorEmpaque,
        PaquetesSugeridos = d.PaquetesSugeridos,
        CantidadSugerida = d.CantidadSugerida,
        MotivoCompra = d.MotivoCompra,
        NivelAlerta = d.NivelAlerta,
        RequiereRevision = d.RequiereRevision,
        MotivoRevision = d.MotivoRevision,
        DiasCoberturaActual = d.DiasCoberturaActual,
        ExcesoInventario = d.ExcesoInventario,
        PorcentajeExcesoInventario = d.PorcentajeExcesoInventario,
        EstadoInventario = d.EstadoInventario,
        MotivoInventario = d.MotivoInventario
    };
}

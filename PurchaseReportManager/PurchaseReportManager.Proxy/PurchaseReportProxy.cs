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

    public async Task<IReadOnlyList<PurchaseReportLine>> GetPurchaseReportAsync(
        string tenantId,
        string familia,
        string? subFamilia = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(familia, subFamilia);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Tenant-Id", tenantId);
        request.Headers.Add("X-Application-Id", _options.ApplicationId);

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

    private static string BuildUrl(string familia, string? subFamilia)
    {
        var sb = new StringBuilder("/api/reports/purchase-v2?familia=")
            .Append(Uri.EscapeDataString(familia));

        if (!string.IsNullOrWhiteSpace(subFamilia))
            sb.Append("&subFamilia=").Append(Uri.EscapeDataString(subFamilia));

        return sb.ToString();
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
        MotivoRevision = d.MotivoRevision
    };
}

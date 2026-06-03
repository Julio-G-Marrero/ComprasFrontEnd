namespace Domain;

public sealed class PurchaseReportLine
{
    public string Sku { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string Familia { get; init; } = string.Empty;
    public string SubFamilia { get; init; } = string.Empty;

    public bool RequeridoStock { get; init; }
    public decimal ExistenciaEfectiva { get; init; }

    public decimal Ventas45Dias { get; init; }
    public decimal DemandaDiaria { get; init; }
    public decimal DesviacionEstandarDiaria { get; init; }
    public decimal? CoeficienteVariacion { get; init; }

    public string Abc { get; init; } = string.Empty;
    public string Xyz { get; init; } = string.Empty;

    public decimal DiasProveedor { get; init; }
    public decimal FrecuenciaRevision { get; init; }
    public decimal PeriodoProteccion { get; init; }
    public decimal StockSeguridad { get; init; }
    public decimal StockMinimoOperativo { get; init; }
    public decimal Rop { get; init; }
    public decimal InventarioObjetivoRotacion { get; init; }
    public decimal InventarioObjetivoFinal { get; init; }

    public decimal CantidadBruta { get; init; }
    public decimal CantidadPorEmpaque { get; init; }
    public decimal PaquetesSugeridos { get; init; }
    public decimal CantidadSugerida { get; init; }
    public string MotivoCompra { get; init; } = string.Empty;

    public string NivelAlerta { get; init; } = string.Empty;
    public bool RequiereRevision { get; init; }
    public string MotivoRevision { get; init; } = string.Empty;

    public bool EsCritico => NivelAlerta.Equals("CRITICO", StringComparison.OrdinalIgnoreCase);
    public bool TieneSugerencia => CantidadSugerida > 0;
}

namespace PurchaseReportManager.ViewModels;

public static class LegacyTextNormalizer
{
    private const char R = '�'; // Unicode replacement character

    // Palabras/frases conocidas que llegan rotas. Agregar nuevas aquí conforme se descubran.
    private static readonly (string Broken, string Fixed)[] KnownWords =
    [
        ($"CAF{R}",       "CAFÉ"),
        ($"Caf{R}",       "Café"),
        ($"caf{R}",       "café"),
        ($"TABAC{R}",     "TABACO"),   // por si llega así también
        ($"A{R}OS",       "AROS"),     // AEROSOL partido
        ($"CARAM{R}LO",   "CARAMELO"),
        ($"CARAM{R}LOS",  "CARAMELOS"),
        ($"JALAP{R}O",    "JALAPEÑO"),
        ($"CHAMPI{R}{R}N","CHAMPIÑÓN"),
        ($"AZ{R}CAR",     "AZÚCAR"),
        ($"az{R}car",     "azúcar"),
        ($"LIMPIA{R}OR",  "LIMPIADOR"),
    ];

    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (!value.Contains(R)) return value;

        var result = value;
        foreach (var (broken, corrected) in KnownWords)
            result = result.Replace(broken, corrected, StringComparison.Ordinal);

        return result;
    }
}

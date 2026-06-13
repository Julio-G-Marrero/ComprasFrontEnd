namespace Common.Views.Helpers;

public static class InventoryDisplayHelper
{
    public static string Label(string estado) => estado.ToUpperInvariant() switch
    {
        "SALUDABLE"           => "Óptimo",
        "BAJO_OBJETIVO"       => "Bajo objetivo",
        "SOBRESTOCK"          => "Sobrestock",
        "SIN_VENTA_CON_STOCK" => "Sin venta con stock",
        _                     => string.IsNullOrEmpty(estado) ? "-" : estado
    };

    public static string BadgeClass(string estado) => estado.ToUpperInvariant() switch
    {
        "SALUDABLE"           => "badge bg-success",
        "BAJO_OBJETIVO"       => "badge bg-warning",
        "SOBRESTOCK"          => "badge bg-dark",
        "SIN_VENTA_CON_STOCK" => "badge bg-secondary",
        _                     => "badge bg-light text-dark"
    };
}

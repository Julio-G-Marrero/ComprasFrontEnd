namespace Web.Components.Layout;

public partial class MainLayout
{
    // ═══════════════════════════════════════════════════════════
    // MODELS
    // ═══════════════════════════════════════════════════════════

    record AppLinkDto(string Label, string Url, string Icon);
    record AppLink(string Label, string Url, string Icon, string? Policy = null);
    record AppGroup(string GroupLabel, string GroupIcon, List<AppLink> Apps);

    // ═══════════════════════════════════════════════════════════
    // NAVIGATION DATA
    // To add a policy: ["/url"] = "Policy.Name"
    // Links without an entry will always be visible.
    // ═══════════════════════════════════════════════════════════

    static readonly Dictionary<string, string> PolicyMap = new()
    {
        ["/budget-authorization/orders"] = "BudgetAuthorization.Page.Orders",
        ["/valuation/orders"]            = "Valuation.Page.Orders",
        ["/valuation/orders-assignment"] = "Valuation.Page.AssignOrder",
        ["/order-budgets"]               = "Budget.Page.Orders",

        ["/users"]        = "MultiAccess.Page.Users",
        ["/applications"] = "MultiAccess.Page.Application",
        ["/tenants"]      = "MultiAccess.Page.Tenant",
        ["/roles"]        = "MultiAccess.Page.Role",

        ["/transfers"]         = "Wearehouse.Page.Orders",
        ["/transferswithcost"] = "Wearehouse.Page.ViewCost",
        ["/scan"]              = "Inventory.Page.Scan",
        ["/reporte-compras"]   = "PurchaseReportManager.Page.Report",
    };

    // ═══════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════

    List<AppGroup>   Groups        = [];
    HashSet<string>  expandedGroups = [];
    bool             sidebarOpen   = false;
    string           SearchText    = "";

    // ═══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    protected override Task OnInitializedAsync()
    {
        Groups         = LoadApps();
        expandedGroups = Groups.Select(g => g.GroupLabel).ToHashSet();
        return Task.CompletedTask;
    }

    // ═══════════════════════════════════════════════════════════
    // SEARCH
    // ═══════════════════════════════════════════════════════════

    IReadOnlyList<AppGroup> FilteredGroups
    {
        get
        {
            if (!HasSearch()) return Groups;

            var term = SearchText.Trim();

            return Groups
                .Select(g =>
                {
                    // If the group name matches, show all its links
                    bool groupMatches = g.GroupLabel.Contains(term, StringComparison.OrdinalIgnoreCase);

                    var apps = groupMatches
                        ? g.Apps
                        : g.Apps.Where(a =>
                            a.Label.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            a.Url.Contains(term, StringComparison.OrdinalIgnoreCase))
                          .ToList();

                    return (group: g, apps);
                })
                .Where(x => x.apps.Count > 0)
                .Select(x => new AppGroup(x.group.GroupLabel, x.group.GroupIcon, x.apps.ToList()))
                .ToList();
        }
    }

    bool HasSearch()   => !string.IsNullOrWhiteSpace(SearchText);
    void ClearSearch() => SearchText = "";

    // ═══════════════════════════════════════════════════════════
    // SIDEBAR
    // ═══════════════════════════════════════════════════════════

    void ToggleSidebar() => sidebarOpen = !sidebarOpen;
    void CloseSidebar()  => sidebarOpen = false;

    // ═══════════════════════════════════════════════════════════
    // GROUPS
    // ═══════════════════════════════════════════════════════════

    // Called from the group header button. Behavior differs by sidebar state:
    // - Sidebar open   → toggle the group (expand / collapse)
    // - Sidebar closed → open the sidebar and force the group open
    void HandleGroupClick(string groupLabel)
    {
        if (sidebarOpen) ToggleGroup(groupLabel);
        else             OpenSidebarAndExpandGroup(groupLabel);
    }

    void ToggleGroup(string label)
    {
        if (HasSearch()) return; // Groups are forced open during search
        if (!expandedGroups.Remove(label))
            expandedGroups.Add(label);
    }

    void OpenSidebarAndExpandGroup(string groupLabel)
    {
        sidebarOpen = true;
        expandedGroups.Add(groupLabel);
    }

    // During search all groups are expanded; otherwise the saved set applies.
    bool IsGroupExpanded(string label) =>
        HasSearch() || expandedGroups.Contains(label);

    // ═══════════════════════════════════════════════════════════
    // DATA
    // TODO: replace LoadApps() with:
    //   Groups = await Http.GetFromJsonAsync<List<AppGroup>>("api/apps");
    // ═══════════════════════════════════════════════════════════

    List<AppGroup> LoadApps()
    {
        List<AppLinkDto> orders =
        [
            new("Presupuestos",          "/order-budgets",                   "bi-file-earmark-text-fill"),
            new("Valuación",             "/valuation/orders",                "bi-cash-coin"),
            new("Asignar Valuación",     "/valuation/orders-assignment",     "bi-cash-coin"),
            new("Autorizar Presupuesto", "/budget-authorization/orders",     "bi-check2-circle"),
        ];

        List<AppLinkDto> config =
        [
            new("Usuarios",     "/users",        "bi-people-fill"),
            new("Aplicaciones", "/applications", "bi-grid-3x3-gap-fill"),
            new("Tenants",      "/tenants",      "bi-building-fill"),
            new("Roles",        "/roles",        "bi-shield-lock-fill"),
        ];

        List<AppLinkDto> pos =
        [
            new("Traspasos con Costo", "/transferswithcost", "bi-arrow-left-right"),
            new("Traspasos",           "/transfers",         "bi-arrow-left-right"),
            new("Inventario",          "/scan",              "bi-box-seam-fill"),
            new("Módulo de Compras",   "/reporte-compras",   "bi-clipboard2-data-fill"),
        ];

        return
        [
            new("Órdenes",       "bi-car-front-fill", MapLinks(orders)),
            new("Configuración", "bi-gear-fill",      MapLinks(config)),
            new("POS",           "bi-shop",           MapLinks(pos)),
        ];
    }

    List<AppLink> MapLinks(List<AppLinkDto> dtos) =>
        dtos.Select(dto => new AppLink(
            dto.Label,
            dto.Url,
            dto.Icon,
            PolicyMap.GetValueOrDefault(dto.Url)
        )).ToList();
}

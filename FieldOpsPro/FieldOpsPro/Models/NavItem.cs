namespace FieldOpsPro.Models;

public partial record NavItem(
    string Id,
    string Label,
    string Icon,
    string Route,
    int? BadgeCount = null,
    bool IsActive = false
);

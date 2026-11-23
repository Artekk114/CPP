namespace Core.Reports;

public sealed record ProductMovementSummary(
    string Sku,
    string Name,
    int MovementsCount,
    int NetDelta);
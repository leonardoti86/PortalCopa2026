namespace PortalCopa2026.Components.Shared;

public record ChartDataset(
    string Label,
    IReadOnlyList<double> Data,
    string? BackgroundColor = null,
    string? BorderColor = null,
    int? BorderWidth = null);

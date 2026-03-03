namespace EnterpriseDashboard.Services;

public partial record NetworkNode(string Id, string Label, double X, double Y, int Connections);
public record NetworkEdge(string From, string To);
public record CandlestickPoint(DateTime Date, double Open, double High, double Low, double Close);
public record TreemapItem(string Label, double Value, string Group);
public record FunnelStage(string Label, double Value, double Percentage);

public interface IObservatoryService
{
    ValueTask<IImmutableList<double>> GetSignalAmplitudeAsync(CancellationToken ct);
    ValueTask<IImmutableList<(string Label, double Value)>> GetMonthlyThroughputAsync(CancellationToken ct);
    ValueTask<IImmutableList<(string Label, double[] Values)>> GetCumulativeLoadAsync(CancellationToken ct);
    ValueTask<IImmutableList<(double X, double Y, double Weight)>> GetCorrelationDataAsync(CancellationToken ct);
    ValueTask<IImmutableList<(string Category, double Value)>> GetRankedDistributionAsync(CancellationToken ct);
    ValueTask<IImmutableList<(string Axis, double Value)>> GetRadarMetricsAsync(CancellationToken ct);
    ValueTask<double[,]> GetHeatmapDataAsync(CancellationToken ct);
    ValueTask<IImmutableList<(string Label, double Value, double Max)>> GetArcGaugeDataAsync(CancellationToken ct);
    ValueTask<(int Filled, int Total)> GetWaffleDataAsync(CancellationToken ct);
    ValueTask<(double Value, double Min, double Max)> GetGaugeValueAsync(CancellationToken ct);
    ValueTask<(IImmutableList<NetworkNode> Nodes, IImmutableList<NetworkEdge> Edges)> GetNetworkNodesAsync(CancellationToken ct);
    ValueTask<IImmutableList<CandlestickPoint>> GetCandlestickDataAsync(CancellationToken ct);
    ValueTask<IImmutableList<TreemapItem>> GetTreemapDataAsync(CancellationToken ct);
    ValueTask<IImmutableList<FunnelStage>> GetFunnelDataAsync(CancellationToken ct);
}

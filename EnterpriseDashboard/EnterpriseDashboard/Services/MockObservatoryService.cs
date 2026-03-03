namespace EnterpriseDashboard.Services;

public class MockObservatoryService : IObservatoryService
{
    private readonly Random _rng = new(42);

    public ValueTask<IImmutableList<double>> GetSignalAmplitudeAsync(CancellationToken ct)
    {
        var values = new List<double>();
        double phase = 0;
        for (int i = 0; i < 24; i++)
        {
            values.Add(Math.Round(50 + 40 * Math.Sin(phase) + (_rng.NextDouble() - 0.5) * 15, 1));
            phase += 0.55;
        }
        return ValueTask.FromResult<IImmutableList<double>>(values.ToImmutableList());
    }

    public ValueTask<IImmutableList<(string Label, double Value)>> GetMonthlyThroughputAsync(CancellationToken ct)
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var items = months.Select(m => (m, Math.Round(200 + _rng.NextDouble() * 800, 0))).ToImmutableList();
        return ValueTask.FromResult<IImmutableList<(string, double)>>(items);
    }

    public ValueTask<IImmutableList<(string Label, double[] Values)>> GetCumulativeLoadAsync(CancellationToken ct)
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var items = months.Select(m =>
        {
            var v1 = Math.Round(100 + _rng.NextDouble() * 200, 0);
            var v2 = Math.Round(80 + _rng.NextDouble() * 150, 0);
            var v3 = Math.Round(60 + _rng.NextDouble() * 120, 0);
            return (m, new[] { v1, v2, v3 });
        }).ToImmutableList();
        return ValueTask.FromResult<IImmutableList<(string, double[])>>(items);
    }

    public ValueTask<IImmutableList<(double X, double Y, double Weight)>> GetCorrelationDataAsync(CancellationToken ct)
    {
        var points = Enumerable.Range(0, 50).Select(_ =>
        {
            double x = _rng.NextDouble() * 100;
            double y = x * 0.6 + (_rng.NextDouble() - 0.5) * 40 + 20;
            double weight = 3 + _rng.NextDouble() * 12;
            return (Math.Round(x, 1), Math.Round(y, 1), Math.Round(weight, 1));
        }).ToImmutableList();
        return ValueTask.FromResult<IImmutableList<(double, double, double)>>(points);
    }

    public ValueTask<IImmutableList<(string Category, double Value)>> GetRankedDistributionAsync(CancellationToken ct)
    {
        var categories = new[] { "Compute", "Storage", "Network", "Database", "Cache", "CDN", "Functions", "Queue" };
        var items = categories
            .Select(c => (c, Math.Round(100 + _rng.NextDouble() * 900, 0)))
            .OrderByDescending(x => x.Item2)
            .ToImmutableList();
        return ValueTask.FromResult<IImmutableList<(string, double)>>(items);
    }

    public ValueTask<IImmutableList<(string Axis, double Value)>> GetRadarMetricsAsync(CancellationToken ct)
    {
        var axes = new[] { "Latency", "Throughput", "Uptime", "Security", "Scalability", "Cost Eff." };
        var items = axes.Select(a => (a, Math.Round(40 + _rng.NextDouble() * 60, 0))).ToImmutableList();
        return ValueTask.FromResult<IImmutableList<(string, double)>>(items);
    }

    public ValueTask<double[,]> GetHeatmapDataAsync(CancellationToken ct)
    {
        var data = new double[7, 24];
        for (int d = 0; d < 7; d++)
            for (int h = 0; h < 24; h++)
            {
                // Simulate higher activity during work hours on weekdays
                double base_val = (d < 5 && h >= 8 && h <= 18) ? 0.6 : 0.2;
                data[d, h] = Math.Clamp(base_val + (_rng.NextDouble() - 0.3) * 0.5, 0, 1);
            }
        return ValueTask.FromResult(data);
    }

    public ValueTask<IImmutableList<(string Label, double Value, double Max)>> GetArcGaugeDataAsync(CancellationToken ct)
    {
        var rings = new (string, double, double)[]
        {
            ("CPU", Math.Round(30 + _rng.NextDouble() * 60, 0), 100),
            ("Memory", Math.Round(40 + _rng.NextDouble() * 50, 0), 100),
            ("Disk", Math.Round(20 + _rng.NextDouble() * 70, 0), 100),
            ("Network", Math.Round(10 + _rng.NextDouble() * 80, 0), 100),
        };
        return ValueTask.FromResult<IImmutableList<(string, double, double)>>(rings.ToImmutableList());
    }

    public ValueTask<(int Filled, int Total)> GetWaffleDataAsync(CancellationToken ct)
    {
        return ValueTask.FromResult((73, 100));
    }

    public ValueTask<(double Value, double Min, double Max)> GetGaugeValueAsync(CancellationToken ct)
    {
        return ValueTask.FromResult((72.5, 0.0, 100.0));
    }

    public ValueTask<(IImmutableList<NetworkNode> Nodes, IImmutableList<NetworkEdge> Edges)> GetNetworkNodesAsync(CancellationToken ct)
    {
        // Pre-computed circular layout
        var nodeNames = new[] { "Gateway", "Auth", "API", "Cache", "DB-Primary", "DB-Replica", "Worker", "Queue", "CDN", "Monitor" };
        var nodes = new List<NetworkNode>();
        for (int i = 0; i < nodeNames.Length; i++)
        {
            double angle = 2 * Math.PI * i / nodeNames.Length;
            double x = 0.5 + 0.35 * Math.Cos(angle);
            double y = 0.5 + 0.35 * Math.Sin(angle);
            nodes.Add(new NetworkNode($"n{i}", nodeNames[i], Math.Round(x, 3), Math.Round(y, 3), 0));
        }

        var edges = new List<NetworkEdge>
        {
            new("n0", "n1"), new("n0", "n2"), new("n0", "n8"),
            new("n1", "n2"), new("n2", "n3"), new("n2", "n4"),
            new("n3", "n6"), new("n4", "n5"), new("n5", "n4"),
            new("n6", "n7"), new("n7", "n2"), new("n8", "n2"),
            new("n9", "n0"), new("n9", "n2"), new("n9", "n4"),
            new("n9", "n6"),
        };

        // Calculate connection counts
        var connCounts = new Dictionary<string, int>();
        foreach (var e in edges)
        {
            connCounts[e.From] = connCounts.GetValueOrDefault(e.From) + 1;
            connCounts[e.To] = connCounts.GetValueOrDefault(e.To) + 1;
        }
        var finalNodes = nodes.Select(n => n with { Connections = connCounts.GetValueOrDefault(n.Id) }).ToImmutableList();

        return ValueTask.FromResult<(IImmutableList<NetworkNode>, IImmutableList<NetworkEdge>)>(
            (finalNodes, edges.ToImmutableList()));
    }

    public ValueTask<IImmutableList<CandlestickPoint>> GetCandlestickDataAsync(CancellationToken ct)
    {
        var baseDate = new DateTime(2025, 10, 1);
        double price = 145.0;
        var points = new List<CandlestickPoint>();
        for (int i = 0; i < 30; i++)
        {
            double open = price;
            double change = (_rng.NextDouble() - 0.45) * 8;
            double close = open + change;
            double high = Math.Max(open, close) + _rng.NextDouble() * 4;
            double low = Math.Min(open, close) - _rng.NextDouble() * 4;
            points.Add(new CandlestickPoint(
                baseDate.AddDays(i),
                Math.Round(open, 2),
                Math.Round(high, 2),
                Math.Round(low, 2),
                Math.Round(close, 2)));
            price = close;
        }
        return ValueTask.FromResult<IImmutableList<CandlestickPoint>>(points.ToImmutableList());
    }

    public ValueTask<IImmutableList<TreemapItem>> GetTreemapDataAsync(CancellationToken ct)
    {
        var items = new List<TreemapItem>
        {
            new("API Gateway", 340, "Infrastructure"),
            new("Auth Service", 220, "Infrastructure"),
            new("Load Balancer", 180, "Infrastructure"),
            new("CDN", 120, "Infrastructure"),
            new("PostgreSQL", 290, "Data"),
            new("Redis Cache", 200, "Data"),
            new("Elasticsearch", 160, "Data"),
            new("S3 Storage", 95, "Data"),
            new("React App", 260, "Frontend"),
            new("Mobile SDK", 150, "Frontend"),
            new("Admin Panel", 110, "Frontend"),
            new("Worker Pool", 240, "Compute"),
            new("ML Pipeline", 190, "Compute"),
            new("Batch Jobs", 130, "Compute"),
            new("Monitoring", 170, "Ops"),
            new("CI/CD", 140, "Ops"),
        };
        return ValueTask.FromResult<IImmutableList<TreemapItem>>(items.ToImmutableList());
    }

    public ValueTask<IImmutableList<FunnelStage>> GetFunnelDataAsync(CancellationToken ct)
    {
        var stages = new List<FunnelStage>
        {
            new("Visitors", 12400, 100),
            new("Sign-ups", 5800, 46.8),
            new("Activated", 3200, 25.8),
            new("Subscribed", 1850, 14.9),
            new("Retained", 1120, 9.0),
            new("Advocates", 480, 3.9),
        };
        return ValueTask.FromResult<IImmutableList<FunnelStage>>(stages.ToImmutableList());
    }
}

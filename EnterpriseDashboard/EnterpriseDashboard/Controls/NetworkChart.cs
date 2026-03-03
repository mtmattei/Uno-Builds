using EnterpriseDashboard.Services;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Uno.WinUI.Graphics2DSK;
using Windows.Foundation;

namespace EnterpriseDashboard.Controls;

public class NetworkChart : SKCanvasElement
{
    private IImmutableList<NetworkNode>? _nodes;
    private IImmutableList<NetworkEdge>? _edges;
    private float _animProgress;
    private DispatcherTimer? _timer;
    private bool _isTerminal;
    private bool _animStarted;

    public NetworkChart()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_nodes != null && !_animStarted)
        {
            StartAnimation();
        }
    }

    public void SetData(IImmutableList<NetworkNode> nodes, IImmutableList<NetworkEdge> edges, bool terminal = false)
    {
        _nodes = nodes;
        _edges = edges;
        _isTerminal = terminal;
        _animStarted = false;
        if (IsLoaded)
        {
            StartAnimation();
        }
    }

    public void SetTheme(bool terminal)
    {
        _isTerminal = terminal;
        if (_animProgress >= 1f) Invalidate();
    }

    private void StartAnimation()
    {
        _animStarted = true;
        _animProgress = 0f;
        _timer?.Stop();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (s, e) =>
        {
            _animProgress = Math.Min(1f, _animProgress + 0.02f);
            Invalidate();
            if (_animProgress >= 1f) _timer.Stop();
        };
        _timer.Start();
    }

    protected override void RenderOverride(SKCanvas canvas, Size area)
    {
        canvas.Clear(SKColors.Transparent);
        if (_nodes == null || _edges == null || area.Width < 1 || area.Height < 1) return;

        float w = (float)area.Width;
        float h = (float)area.Height;
        float padding = 40f;
        float drawW = w - padding * 2;
        float drawH = h - padding * 2;

        var positions = new Dictionary<string, SKPoint>();
        foreach (var node in _nodes)
        {
            positions[node.Id] = new SKPoint(
                padding + (float)(node.X * drawW),
                padding + (float)(node.Y * drawH));
        }

        float progress = _animStarted ? _animProgress : 1f;
        float nodeAlpha = Math.Min(1f, progress * 2f);
        float edgeProgress = Math.Max(0f, (progress - 0.3f) / 0.7f);

        if (edgeProgress > 0)
        {
            int visibleEdges = (int)(_edges.Count * edgeProgress);
            using var edgePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                IsAntialias = true,
                Color = ObservatoryColors.GetSubtle(_isTerminal)
            };

            for (int i = 0; i < visibleEdges; i++)
            {
                var edge = _edges[i];
                if (positions.TryGetValue(edge.From, out var from) && positions.TryGetValue(edge.To, out var to))
                {
                    canvas.DrawLine(from, to, edgePaint);
                }
            }
        }

        using var nodePaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        using var nodeStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true,
            Color = ObservatoryColors.GetEmphasis(_isTerminal).WithAlpha((byte)(nodeAlpha * 255))
        };

        using var labelFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI"), 9f);
        using var labelPaint = new SKPaint { IsAntialias = true };

        int maxConn = _nodes.Max(n => n.Connections);

        foreach (var node in _nodes)
        {
            if (!positions.TryGetValue(node.Id, out var pos)) continue;

            float baseSize = 6f;
            float sizeScale = maxConn > 0 ? (float)node.Connections / maxConn : 0.5f;
            float radius = baseSize + sizeScale * 10f;

            float brightness = 0.3f + sizeScale * 0.7f;
            nodePaint.Color = ObservatoryColors.MapValue(brightness, _isTerminal).WithAlpha((byte)(nodeAlpha * 255));
            canvas.DrawCircle(pos, radius, nodePaint);
            canvas.DrawCircle(pos, radius, nodeStroke);

            labelPaint.Color = ObservatoryColors.GetText(_isTerminal).WithAlpha((byte)(nodeAlpha * 255));
            canvas.DrawText(node.Label, pos.X, pos.Y + radius + 12, SKTextAlign.Center, labelFont, labelPaint);
        }
    }
}

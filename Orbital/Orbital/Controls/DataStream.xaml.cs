namespace Orbital.Controls;

public sealed partial class DataStream : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    public DataStream()
    {
        this.InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += (_, _) => GenerateStream();
        GenerateStream();

        this.Loaded += (_, _) =>
        {
            _timer.Start();
            FlickerAnimation.Begin();
        };
        this.Unloaded += (_, _) =>
        {
            _timer.Stop();
            FlickerAnimation.Stop();
        };
    }

    private void GenerateStream()
    {
        var hex = new char[24 * 3 - 1];
        for (var i = 0; i < 24; i++)
        {
            var b = _random.Next(256);
            var offset = i * 3;
            hex[offset] = GetHexChar(b >> 4);
            hex[offset + 1] = GetHexChar(b & 0xF);
            if (i < 23) hex[offset + 2] = ' ';
        }
        StreamText.Text = new string(hex);
    }

    private static char GetHexChar(int val) =>
        val < 10 ? (char)('0' + val) : (char)('a' + val - 10);
}

// Reticle Loader geometry generator (spec Appendix A).
// Run: dotnet run tools/reticle-geometry.cs
// Prints the Path Data pasted into Themes/ReticleRing.xaml. All arcs are circular
// because Uno's Path.Data parser throws on rx != ry.
using System.Globalization;
using System.Text;

var hero = new Polar(300, 300);
var compact = new Polar(20, 20);

Print("Hero.OuterArcs",
    hero.Arc(240, 0, 195) + " " + hero.Arc(235, 35, 88) + " " + hero.Tick(240, 248, 90));

Print("Hero.TickScale", hero.Ticks(222, 229, 234, from: 200, to: 356, step: 3, longEvery: 5));

Print("Hero.MainHalf", hero.Arc(215, 180, 360));

Print("Hero.GapArc", hero.Arc(156, 150, 420));

Print("Hero.Band", hero.Ticks(168, 176, 181, from: 95, to: 190, step: 2, longEvery: 6));

Print("Hero.Radials", string.Join(" ",
    hero.Tick(182, 206, 105), hero.Tick(182, 206, 122), hero.Tick(182, 206, 140), hero.Tick(182, 206, 158),
    hero.Tick(176, 190, 225), hero.Tick(176, 190, 315),
    hero.Tick(170, 185, 0),
    hero.Tick(172, 190, 270),
    hero.Tick(232, 262, 270),
    hero.Tick(180, 214, 90)));

Print("Hero.Reticle", string.Join(" ",
    hero.Tick(22, 34, 45), hero.Tick(22, 34, 135), hero.Tick(22, 34, 225), hero.Tick(22, 34, 315)));

// Hand-placed in the prototype (not polar), carried through verbatim.
Print("Hero.Callouts", "M405,150 L450,78 L532,78 M478,470 L543,528 M175,478 L175,500 A240,240 0 0 0 235,535 L230,543 L55,543");
Print("Hero.Marker", "M42,62 L72,62 L57,87 Z");

Print("CompactArc", compact.Arc(18, 0, 70));
Print("CompactGap", compact.Arc(12, 150, 420));

static void Print(string name, string data) => Console.WriteLine($"{name}\n{data}\n");

// Angles run clockwise from 12 o'clock: P(r,a) = (cx + r·sin a, cy − r·cos a).
readonly record struct Polar(double Cx, double Cy)
{
    public string P(double r, double deg)
    {
        var a = deg * Math.PI / 180;
        return $"{N(Cx + r * Math.Sin(a))},{N(Cy - r * Math.Cos(a))}";
    }

    public string Arc(double r, double from, double to)
    {
        var large = to - from > 180 ? 1 : 0;
        return $"M{P(r, from)} A{N(r)},{N(r)} 0 {large} 1 {P(r, to)}";
    }

    public string Tick(double r0, double r1, double deg) => $"M{P(r0, deg)} L{P(r1, deg)}";

    public string Ticks(double r0, double r1, double rLong, double from, double to, double step, int longEvery)
    {
        var sb = new StringBuilder();
        var i = 0;
        for (var a = from; a <= to + 1e-9; a += step, i++)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(Tick(r0, i % longEvery == 0 ? rLong : r1, a));
        }
        return sb.ToString();
    }

    static string N(double v)
    {
        var s = Math.Round(v, 2).ToString("0.##", CultureInfo.InvariantCulture);
        return s == "-0" ? "0" : s;
    }
}

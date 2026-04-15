using Uno.Extensions.Reactive;

namespace PrecisionDial.Samples;

public partial record AudioMixerModel
{
    public IState<double> Volume => State.Value(this, () => 75.0);
    public IState<double> Bass => State.Value(this, () => 50.0);
    public IState<double> Treble => State.Value(this, () => 50.0);
}

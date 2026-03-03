using YUL.Models;

namespace YUL.Presentation;

public partial record BoardingPassModel
{
    public IState<BoardingPass> BoardingPass => State.Value(this, () => MockFlightData.SampleFlights[0]);
}

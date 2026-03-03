namespace YUL.Models;

public record BoardingPass
{
    public string FlightNumber { get; init; } = string.Empty;
    public string Airline { get; init; } = string.Empty;
    public string DepartureCity { get; init; } = string.Empty;
    public string DepartureAirport { get; init; } = string.Empty;
    public string ArrivalCity { get; init; } = string.Empty;
    public string ArrivalAirport { get; init; } = string.Empty;
    public string PassengerName { get; init; } = string.Empty;
    public string Seat { get; init; } = string.Empty;
    public string Gate { get; init; } = string.Empty;
    public string Terminal { get; init; } = string.Empty;
    public string BoardingTime { get; init; } = string.Empty;
    public string DepartureTime { get; init; } = string.Empty;
    public string ConfirmationCode { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    public string QRCodeData => $"{FlightNumber}|{PassengerName}|{Seat}|{ConfirmationCode}|{Date}";
}

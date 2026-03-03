using YUL.Models;

namespace YUL.Presentation;

public static class MockFlightData
{
    public static readonly BoardingPass[] SampleFlights = 
    [
        new BoardingPass
        {
            FlightNumber = "AA123",
            Airline = "Air Canada",
            DepartureCity = "Montreal",
            DepartureAirport = "YUL",
            ArrivalCity = "Orlando",
            ArrivalAirport = "MCO",
            PassengerName = "Matt Uno",
            Seat = "12A",
            Gate = "B12",
            Terminal = "2",
            BoardingTime = "10:45 AM",
            DepartureTime = "11:30 AM",
            ConfirmationCode = "ABC123",
            Date = "November 16, 2025",
            Status = "Now Boarding"
        },
        new BoardingPass
        {
            FlightNumber = "DL456",
            Airline = "Delta Air Lines",
            DepartureCity = "Los Angeles",
            DepartureAirport = "LAX",
            ArrivalCity = "Miami",
            ArrivalAirport = "MIA",
            PassengerName = "Jane Smith",
            Seat = "8C",
            Gate = "A7",
            Terminal = "1",
            BoardingTime = "2:15 PM",
            DepartureTime = "3:00 PM",
            ConfirmationCode = "DEF456",
            Date = "November 17, 2025",
            Status = "Now Boarding"
        },
        new BoardingPass
        {
            FlightNumber = "UA789",
            Airline = "United Airlines",
            DepartureCity = "Chicago",
            DepartureAirport = "ORD",
            ArrivalCity = "Seattle",
            ArrivalAirport = "SEA",
            PassengerName = "Michael Johnson",
            Seat = "15F",
            Gate = "C22",
            Terminal = "3",
            BoardingTime = "5:30 PM",
            DepartureTime = "6:15 PM",
            ConfirmationCode = "GHI789",
            Date = "November 17, 2025",
            Status = "Boarding Soon"
        },
        new BoardingPass
        {
            FlightNumber = "SW234",
            Airline = "Southwest Airlines",
            DepartureCity = "Dallas",
            DepartureAirport = "DFW",
            ArrivalCity = "Denver",
            ArrivalAirport = "DEN",
            PassengerName = "Emily Davis",
            Seat = "21B",
            Gate = "D15",
            Terminal = "4",
            BoardingTime = "7:45 AM",
            DepartureTime = "8:30 AM",
            ConfirmationCode = "JKL234",
            Date = "November 18, 2025",
            Status = "Now Boarding"
        },
        new BoardingPass
        {
            FlightNumber = "BA567",
            Airline = "British Airways",
            DepartureCity = "New York",
            DepartureAirport = "JFK",
            ArrivalCity = "London",
            ArrivalAirport = "LHR",
            PassengerName = "Robert Wilson",
            Seat = "3A",
            Gate = "B32",
            Terminal = "7",
            BoardingTime = "9:30 PM",
            DepartureTime = "10:45 PM",
            ConfirmationCode = "MNO567",
            Date = "November 17, 2025",
            Status = "Final Call"
        }
    ];

    public static BoardingPass GetRandomFlight()
    {
        var random = new Random();
        return SampleFlights[random.Next(SampleFlights.Length)];
    }

    public static BoardingPass GetFlight(int index)
    {
        return SampleFlights[index % SampleFlights.Length];
    }
}

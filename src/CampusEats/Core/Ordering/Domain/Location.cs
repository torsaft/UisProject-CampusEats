namespace CampusEats.Core.Ordering.Domain;

public sealed class Location
{
    public Location(string building, string roomNumber, string? notes)
    {
        Building = building;
        RoomNumber = roomNumber;
        Notes = notes;
    }
    public string Building { get; private set; }
    public string RoomNumber { get; private set; }
    public string? Notes { get; private set; } = "";
}
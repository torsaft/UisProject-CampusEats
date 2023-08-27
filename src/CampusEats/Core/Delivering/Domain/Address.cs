namespace CampusEats.Core.Delivering.Domain;

public sealed class Address
{
	public Address(string building, string roomNumber, string notes)
	{
		Building = building;
		RoomNumber = roomNumber;
		Notes = notes;
	}

	public string Building { get; private set; }
	public string RoomNumber { get; private set; }
	public string Notes { get; private set; }
}

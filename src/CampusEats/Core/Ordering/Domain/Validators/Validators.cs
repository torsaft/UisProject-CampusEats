using CampusEats.Core.Common;

namespace CampusEats.Core.Ordering.Domain.Validators;

public class LocationBuildingValidator : IValidator<Location>
{
    public (bool, string) IsValid(Location location)
    {
        _ = location ?? throw new ArgumentNullException(nameof(location), "Cannot validate a null object");
        if(string.IsNullOrWhiteSpace(location.Building)) return (false, $"{nameof(location.Building)} cannot be empty");
        return (true, "");
    }
}
public class LocationRoomNumberValidator : IValidator<Location>
{
    public (bool, string) IsValid(Location location)
    {
        _ = location ?? throw new ArgumentNullException(nameof(location), "Cannot validate a null object");
        if(string.IsNullOrWhiteSpace(location.RoomNumber)) return (false, $"{nameof(location.RoomNumber)} cannot be empty");
        return (true, "");
    }
}
public class LocationNotesValidator : IValidator<Location>
{
    public (bool, string) IsValid(Location location)
    {
        _ = location ?? throw new ArgumentNullException(nameof(location), "Cannot validate a null object");
        if(string.IsNullOrWhiteSpace(location.Notes)) return (false, $"{nameof(location.Notes)} cannot be empty");
        return (true, "");
    }
}


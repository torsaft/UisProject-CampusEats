namespace CampusEats.Core.Identity.Domain.Dto;

public sealed class CourierStatusDto
{
    public CourierStatusDto(AppUser user)
    {
        UserEmail = user.Email;
        UserCourierStatus = (RequestStatus)user.RequestStatus!;
    }
    public string UserEmail { get; set; }
    public RequestStatus UserCourierStatus { get; set; }
}

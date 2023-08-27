using CampusEats.Core.Common;

namespace CampusEats.Core.Identity.Domain.Events;

public sealed record CourierRequest : IDomainEvent
{
    public CourierRequest(string userEmail, RequestStatus userCourierStatus)
    {
        UserEmail = userEmail;
        UserCourierStatus = userCourierStatus;
    }
    public string UserEmail { get; set; }
    public RequestStatus UserCourierStatus { get; set; }
}

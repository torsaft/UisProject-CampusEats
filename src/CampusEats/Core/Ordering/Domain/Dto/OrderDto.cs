namespace CampusEats.Core.Ordering.Domain.Dto;

public sealed record OrderDto
{
    public OrderDto(Order order)
    {
        OrderId = order.Id;
        CustomerEmail = order.Customer.Email;
        OrderDate = order.OrderDate;
        Status = order.Status;
        DeliveryFee = order.DeliveryFee;
        OrderLines = order.OrderLines;
        IsActive = order.IsActive;
    }
    public Guid OrderId { get; init; }
    public string CustomerEmail { get; init; }
    public DateTime OrderDate { get; init; }
    public Status Status { get; init; }
    public decimal DeliveryFee { get; init; }
    public IEnumerable<OrderLine> OrderLines { get; init; }
    public bool IsActive { get; init; }
}

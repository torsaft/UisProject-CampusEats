namespace CampusEats.Core.Ordering.Domain.Dto;

public sealed record OrderExpenseDto
{
    public OrderExpenseDto(Order order)
    {
        OrderId = order.Id;
        OrderDate = order.OrderDate;
        DeliveryFee = order.DeliveryFee;
        TotalCost = order.OrderLines.Sum(ol => ol.Sum);
    }
    public Guid OrderId { get; init; }
    public DateTime OrderDate { get; init; }
    public decimal DeliveryFee { get; init; }
    public decimal TotalCost { get; init; }
}

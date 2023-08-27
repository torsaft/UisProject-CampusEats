using CampusEats.Core.Common;
using CampusEats.Core.Products.Events;

namespace CampusEats.Core.Products.Domain;

public sealed class Product : BaseEntity
{
    public Product(string name, decimal price, string description)
    {
        Id = Guid.NewGuid();
        _name = name;
        _price = price;
        Description = description;
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if(!string.IsNullOrEmpty(value) && _name != value)
            {
                AddEvent(new ProductNameChanged(Id, _name, value));
                _name = value;
            }
        }
    }

    private decimal _price;
    public decimal Price
    {
        get => _price;
        set
        {
            if(_price > 0 && _price != value)
            {
                AddEvent(new ProductPriceChanged(Id, _price, value));
                _price = value;
            }
        }
    }

    public string Description { get; set; }
}
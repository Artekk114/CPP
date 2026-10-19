using System.Collections.Generic;
using System.Linq;
using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public bool IsConfirmed { get; private set; }
    public decimal Total => _lines.Sum(l => l.Price * l.Quantity);

    private Order(string id, bool isConfirmed)
    {
        Id = id;
        IsConfirmed = isConfirmed;
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));

        return new Order(id.Trim(), false);
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {
        if (IsConfirmed)
            throw new InvalidOperationException($"Замовлення {Id} вже підтверджене, рядки додавати не можна");

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару не може бути порожнім", nameof(productId));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна не може бути від'ємною");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Кількість у рядку має бути більшою за нуль");

        _lines.Add(new OrderLine(productId, name, price, quantity));
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException($"Замовлення {Id} порожнє, підтвердження неможливе");

        if (IsConfirmed)
            throw new InvalidOperationException($"Замовлення {Id} вже підтверджене");

        IsConfirmed = true;
    }
    public OrderDto ToDto()
    {
        var lineDtos = _lines.Select(l => new OrderLineDto(l.ProductId, l.Name, l.Price, l.Quantity)).ToList();
        return new OrderDto(Id, IsConfirmed, lineDtos);
    }
    public static Order FromDto(OrderDto dto)
    {
        Order order = Create(dto.Id);

        foreach (var line in dto.Lines)
        {
            order.AddLine(line.ProductId, line.Name, line.Price, line.Quantity);
        }

        if (dto.IsConfirmed)
        {
            order.Confirm();
        }

        return order;
    }
}
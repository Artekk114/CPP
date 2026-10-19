using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public OrderStatus Status { get; private set; }

    public decimal Total => _lines.Sum(l => l.Price * l.Quantity);

    private Order(string id, OrderStatus status)
    {
        Id = id;
        Status = status;
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));

        return new Order(id.Trim(), OrderStatus.Draft);
    }

    public void AddLine(string productId, string name, decimal price, int quantity)
    {

        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Замовлення {Id} має статус {Status}, рядки додавати не можна");

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

        Status = Status switch
        {
            OrderStatus.Draft => OrderStatus.Confirmed,
            OrderStatus.Confirmed => throw new InvalidOperationException($"Замовлення {Id} вже підтверджене"),
            OrderStatus.Cancelled => throw new InvalidOperationException($"Скасоване замовлення {Id} не можна підтвердити"),
            _ => throw new NotImplementedException("Невідомий статус")
        };
    }

    public void Cancel()
    {
        Status = Status switch
        {
            OrderStatus.Draft => OrderStatus.Cancelled,
            OrderStatus.Confirmed => throw new InvalidOperationException($"Підтверджене замовлення {Id} не можна скасувати"),
            OrderStatus.Cancelled => throw new InvalidOperationException($"Замовлення {Id} вже скасоване"),
            _ => throw new NotImplementedException("Невідомий статус")
        };
    }

    // Оновлені методи DTO (припускаємо, що в OrderDto замість bool IsConfirmed тепер string Status або OrderStatus)
    /*
    public OrderDto ToDto()
    {
        var lineDtos = _lines.Select(l => new OrderLineDto(l.ProductId, l.Name, l.Price, l.Quantity)).ToList();
        return new OrderDto(Id, Status.ToString(), lineDtos); 
    }
    */
}
using System;
using Core.Dto;

namespace Core.Domain;

public sealed class Product
{
    private int _quantity;

    public string Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string Unit { get; }
    public decimal Price { get; }

    public int Quantity => _quantity;
    public ProductStatus Status { get; private set; } 

    private Product(string id, string sku, string name, string unit, decimal price, int quantity, ProductStatus status)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Unit = unit;
        Price = price;
        _quantity = quantity;
        Status = status;
    }

    public static Product Create(string id, string sku, string name, string unit, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU не може бути порожнім", nameof(sku));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва не може бути порожньою", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна не може бути від'ємною");
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Початковий залишок не може бути від'ємним");

        return new Product(id.Trim(), sku.Trim().ToUpperInvariant(), name.Trim(), unit.Trim(), price, quantity, ProductStatus.Active);
    }

    public void RegisterArrival(int amount)
    {
        if (Status == ProductStatus.Discontinued)
            throw new InvalidOperationException($"Товар {Sku} знято з виробництва, прихід неможливий");
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість приходу має бути більшою за нуль");

        _quantity += amount;
    }

    public void Issue(int amount)
    {
        if (Status == ProductStatus.Discontinued)
            throw new InvalidOperationException($"Товар {Sku} знято з виробництва, видача неможлива");
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість видачі має бути більшою за нуль");
        if (amount > _quantity)
            throw new InvalidOperationException($"Не можна видати {amount}: залишок {Sku} = {_quantity}");

        _quantity -= amount;
    }

    public void Discontinue()
    {
        Status = Status switch
        {
            ProductStatus.Active => ProductStatus.Discontinued,
            ProductStatus.Discontinued => throw new InvalidOperationException($"Товар {Sku} вже знято з виробництва"),
            _ => throw new NotImplementedException()
        };
    }

    public ProductDto ToDto() => new(Id, Sku, Name, Unit, Price, Quantity); 

    public static Product FromDto(ProductDto dto) =>
        Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Price, dto.Quantity);
}
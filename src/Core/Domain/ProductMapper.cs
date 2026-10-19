using System;
using System.Collections.Generic;
using Core.Dto;

namespace Core.Domain;

public static class ProductMapper
{
    public static (IReadOnlyList<Product> ValidProducts, IReadOnlyList<string> Errors) MapProducts(IEnumerable<ProductDto> dtos)
    {
        var validProducts = new List<Product>();
        var errors = new List<string>();

        foreach (var dto in dtos)
        {
            try
            {
                Product product = Product.FromDto(dto);
                validProducts.Add(product);
            }
            catch (Exception ex)
            {
                errors.Add($"Помилка імпорту товару {dto.Id}: {ex.Message}");
            }
        }

        return (validProducts.AsReadOnly(), errors.AsReadOnly());
    }
}
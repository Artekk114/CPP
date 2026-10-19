namespace Core.Dto;

public record ProductDto(
    string Id,
    string Sku,
    string Name,
    string Unit,
    decimal Price,
    int Quantity,
    string? Note = null
);
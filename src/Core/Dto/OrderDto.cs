using System.Collections.Generic;

namespace Core.Dto;

public record OrderLineDto(string ProductId, string Name, decimal Price, int Quantity);

public record OrderDto(string Id, bool IsConfirmed, IReadOnlyList<OrderLineDto> Lines);
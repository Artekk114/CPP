namespace Core.Import;

using System.Globalization;
using Core.Dto;

public record WarehouseDto(string Id, string Name);

public record MixedImportResult(
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<WarehouseDto> Warehouses,
    IReadOnlyList<string> Errors);

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;
            if (number == 1 && (line.StartsWith("id", StringComparison.OrdinalIgnoreCase) || line.StartsWith("type", StringComparison.OrdinalIgnoreCase)))
                continue;

            switch (ParseLine(line))
            {
                case ParseProductOk p:
                    products.Add(p.Value);
                    break;
                case ParseWarehouseOk w:
                    warehouses.Add(w.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }
        return new MixedImportResult(products, warehouses, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);
        return parts switch
        {

        ["P", "", _, ..] or ["P", _, "", ..] => new ParseFailed("ID або назва товару порожні"),


            ["P", var id, var name, var price] when decimal.TryParse(price, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal p) && p >= 0
                => new ParseProductOk(new ProductDto(id, name, p)),

            ["P", var id, var name, var price, var note] when decimal.TryParse(price, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal p) && p >= 0
                => new ParseProductOk(new ProductDto(id, name, p, string.IsNullOrWhiteSpace(note) ? null : note)),

            
            ["P", _, _, var badPrice, ..]
                => new ParseFailed($"ціна '{badPrice}' не є коректним додатним числом"),

                
                ["W", "", _] or ["W", _, ""] => new ParseFailed("ID або назва складу порожні"),
            ["W", var id, var name]
                => new ParseWarehouseOk(new WarehouseDto(id, name)),

            
            [var prefix, ..] when prefix != "P" && prefix != "W"
                => new ParseFailed($"невідомий префікс '{prefix}'"),
            _ => new ParseFailed($"неправильний формат рядка: очікується префікс P або W та відповідна кількість колонок ({parts.Length})")
        };
    }


    private abstract record ParseOutcome;
    private sealed record ParseProductOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseWarehouseOk(WarehouseDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
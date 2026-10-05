namespace Core.Import;

using System.Text.Json;
using Core.Dto;

public static class ProductJsonImporter
{
    public static MixedImportResult Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var items = JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? [];

            return new MixedImportResult(items, [], []);
        }
        catch (Exception ex)
        {
            return new MixedImportResult([], [], [$"Помилка JSON: {ex.Message}"]);
        }
    }
}
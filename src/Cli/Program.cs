using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}


MixedImportResult result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => MixedCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => throw new NotSupportedException($"Формат файлу {Path.GetExtension(path)} не підтримується")
};

Console.WriteLine($"Завантажено товарів: {result.Products.Count}, складів: {result.Warehouses.Count}");

foreach (var p in result.Products.Take(5))
    Console.WriteLine($" Товар: {p.Id,-6} {p.Name,-20} {p.Price,8:F2}");

foreach (var w in result.Warehouses.Take(5))
    Console.WriteLine($" Склад: {w.Id,-6} {w.Name}");

if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
        Console.WriteLine($" ! {e}");
}

int accepted = result.Products.Count + result.Warehouses.Count;
int errors = result.Errors.Count;
int total = accepted + errors;
double errorRate = total == 0 ? 0 : (double)errors / total * 100;

Console.WriteLine(new string('-', 50));
Console.WriteLine($"Статистика: Усього: {total} | Прийнято: {accepted} | Пропущено: {errors} | Помилок: {errorRate:F1}%");

return 0;
using System;
using System.IO;
using System.Linq;
using Core.Domain;
using Core.Dto;
using Core.Import;

Console.WriteLine("--- Сценарій 1: Успіх (Доменна модель) ---");
Product product = Product.Create("P-001", "sku-001", "Цемент М400", "шт", 150.50m, 100);
Console.WriteLine($"Створено: {product.Name}, Залишок: {product.Quantity}");
product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine($"Після операцій: Залишок = {product.Quantity}, Статус = {product.Status}\n");

Console.WriteLine("--- Сценарій 2: Порушення інваріантів ---");
TryDo("Видача більша за залишок", () => product.Issue(1000));
TryDo("Порожній SKU при створенні", () => Product.Create("P-002", "   ", "Пісок", "т", 100m, 10));
TryDo("Від'ємна ціна", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5m, 100));

product.Discontinue();
TryDo("Видача товару, знятого з виробництва", () => product.Issue(10));

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював - інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" {title}: {ex.GetType().Name} - {ex.Message}");
    }
}

Console.WriteLine("\n--- Додаткове завдання 1: Мапінг результатів 3-ї лаби ---");
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return;
}


MixedImportResult result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => MixedCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => throw new NotSupportedException($"Формат файлу {Path.GetExtension(path)} не підтримується")
};

var (validProducts, domainErrors) = ProductMapper.MapProducts(result.Products);

Console.WriteLine($"Успішно імпортовано та пройшли бізнес-правила: {validProducts.Count} товарів");
foreach (var p in validProducts.Take(5))
    Console.WriteLine($"  [+] {p.Id,-6} {p.Name,-20} Залишок: {p.Quantity}");

if (domainErrors.Count > 0)
{
    Console.WriteLine($"\nВідхилено доменною моделлю (інваріанти): {domainErrors.Count}");
    foreach (string e in domainErrors.Take(10))
        Console.WriteLine($"  [-] {e}");
}
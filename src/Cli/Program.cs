using System;
using System.IO;
using System.Linq;
using Core.Abstractions;
using Core.Events;
using Core.Import;
using Core.Notifications;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore();

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}\n");

// Додаткове 1: Ручний DI sink-об'єктів
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
INotificationSink consoleSink = new ConsoleSink();
INotificationSink fileSink = new FileSink(logPath);

bool forwardingEnabled = true;

void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    if (!forwardingEnabled) return;
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} \u2192 залишок {e.QuantityAfter}";
    consoleSink.Write(line);
    fileSink.Write(line);
}

service.Changed += OnCatalogChanged;

int changes = 0;
service.Changed += (_, _) => changes++;

Console.WriteLine("--- Сценарій 1: Успішні зміни та підписники ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 150.50m, 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}");

forwardingEnabled = false;
service.Receive(created.Id, 5);
Console.WriteLine($"Подій отримано: {changes}");
forwardingEnabled = true;

Console.WriteLine("\n--- Сценарій 2: Відмови (подія НЕ має виникати) ---");
TryDo("Неіснуючий id", () => service.Receive("FAKE-ID", 10));
TryDo("Від'ємна кількість приходу", () => service.Receive(created.Id, -5));

Console.WriteLine("\n--- Сценарій 3: FindAll з Func (залишок < 10) ---");
var lowStock = service.FindAll(p => p.Quantity < 10);
Console.WriteLine($"Товарів із залишком < 10: {lowStock.Count}");
foreach (var p in lowStock)
{
    Console.WriteLine($"  {p.Id} | {p.Sku,-10} | {p.Name,-20} | Залишок: {p.Quantity} {p.Unit}");
}

// Додаткове 2: Імпорт CSV з колбеком Action<string> для пропущених рядків
Console.WriteLine("\n--- Сценарій 4: Імпорт CSV з Action<string> ---");
var sampleCsv = Path.Combine(AppContext.BaseDirectory, "data", "sample_mixed.csv");
Directory.CreateDirectory(Path.GetDirectoryName(sampleCsv)!);
File.WriteAllLines(sampleCsv,
[
    "type;id;name;price",
    "P;P1;Патч-корд 1м;120.00",
    "P;;Порожній ID;50.00",
    "W;W1;Головний склад",
    "P;P2;Кабель живлення;не_число"
]);

var imported = MixedCsvImporter.Load(sampleCsv, warning =>
    Console.WriteLine($"  [УВАГА імпорту] {warning}"));

Console.WriteLine($"Імпортовано товарів: {imported.Products.Count}, складів: {imported.Warehouses.Count}, помилок: {imported.Errors.Count}");

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" [ ] {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" [X] {title}: {ex.GetType().Name} - {ex.Message}");
    }
}
using System;
using System.IO;
using System.Linq;
using Core.Abstractions;
using Core.Events;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore();

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}\n");

// Перший підписник — локальний метод (щоб можна було відписатися через -=): вивід у консоль + logs/app.log
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} \u2192 залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged;

// Другий підписник — лямбда із замиканням над локальною змінною changes
int changes = 0;
service.Changed += (_, _) => changes++;

// Успішні зміни -> відписка -> ще одна зміна
Console.WriteLine("--- Сценарій 1: Успішні зміни та підписники ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 150.50m, 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}");

service.Changed -= OnCatalogChanged;
service.Receive(created.Id, 5); // локальний обробник мовчить, але лямбда-лічильник додає +1
Console.WriteLine($"Подій отримано: {changes}");

// Сценарій відмови: інваріант або відсутній id -> виняток є, події немає
Console.WriteLine("\n--- Сценарій 2: Відмови (подія НЕ має виникати) ---");
TryDo("Неіснуючий id", () => service.Receive("FAKE-ID", 10));
TryDo("Від'ємна кількість приходу", () => service.Receive(created.Id, -5));

// Пошук через делегат Func<Product, bool>
Console.WriteLine("\n--- Сценарій 3: FindAll з Func (залишок < 10) ---");
var lowStock = service.FindAll(p => p.Quantity < 10);
Console.WriteLine($"Товарів із залишком < 10: {lowStock.Count}");
foreach (var p in lowStock)
{
    Console.WriteLine($"  {p.Id} | {p.Sku,-10} | {p.Name,-20} | Залишок: {p.Quantity} {p.Unit}");
}

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
using System;
using System.IO;
using System.Linq;
using Core.Storage;
using Core.Services;
using Core.Abstractions;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore();

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}\n");

Console.WriteLine("--- Сценарій 1: Успіх ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 150.50m, 50);
Console.WriteLine($"Додано товар: {created.Name}, Залишок: {created.Quantity}");

service.Receive(created.Id, 25);
Console.WriteLine("Після приходу:");

foreach (var p in service.All())
{
    Console.WriteLine($" {p.Id} | {p.Sku,-10} | {p.Name,-20} | Залишок: {p.Quantity,5} | Ціна: {p.Price}");
}

Console.WriteLine("\n--- Сценарій 2: Помилки (Відмови) ---");

TryDo("Спроба отримати неіснуючий товар", () => service.Receive("FAKE-ID", 10));

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
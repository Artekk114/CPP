using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Import;
using Core.Notifications;
using Core.Reports;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

// Якщо запускаємо в пам'яті без --file, засіваємо SampleData.Products()
ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}\n");

// Підписник на події каталогу (консоль + logs/app.log через INotificationSink)
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
INotificationSink consoleSink = new ConsoleSink();
INotificationSink fileSink = new FileSink(logPath);

void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} \u2192 залишок {e.QuantityAfter}";
    consoleSink.Write(line);
    fileSink.Write(line);
}

service.Changed += OnCatalogChanged;

int changes = 0;
service.Changed += (_, _) => changes++;

// Крок 4. Підкоманди звітів через switch (ігноруємо --file при виборі команди)
var items = service.All();
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

Console.WriteLine($"--- Звіт: {command} ---");
switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;
    case "report-low":
        Print(ReportService.LowStock(items, 10));
        break;
    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
            Console.WriteLine($"  {g.Key,-6} позицій: {g.Count,3}   сумарно: {g.TotalQuantity,6}");
        break;
    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($"  позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;
    default:
        Console.WriteLine("report-top | report-low | report-groups | report-sum");
        break;
}

// Крок 3. Демонстрація відкладеного виконання LINQ-запиту
Console.WriteLine("\n--- Демо відкладеного виконання ---");
var source = service.All().ToList();
var query = source.Where(p => p.Quantity < 10); // запит описано, ще не виконано
Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

source.Add(Product.Create("tmp00001", "SKU-999", "Скоба кріпильна", "шт", 3.00m, 2));
Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // вибірка виконується заново й бачить новий елемент

var snapshot = query.ToList();
source.Clear();
Console.WriteLine($"У знімку лишилось: {snapshot.Count}"); // знімок матеріалізовано, очищення source його не змінює

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($"  {l.Sku,-10} {l.Name,-24} {l.Quantity,6} {l.Unit}");
}
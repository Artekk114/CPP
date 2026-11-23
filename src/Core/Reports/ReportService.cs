using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // 1) Фільтр дефіциту + сортування + проєкція в DTO
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold, int take = 10) =>
     source
         .Where(p => p.Quantity < threshold)
         .TopBy(p => p.Quantity, p => p.Sku, take, descending: false)
         .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
         .ToList();


    // 2) Топ-N за залишком зі стабільним порядком (вторинний ключ Sku)
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
       source
           .TopBy(p => p.Quantity, p => p.Sku, n, descending: true)
           .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
           .ToList();

    // 3) Групування за одиницею виміру (без урахування регістру) + сума за групою
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source
            .GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
            .OrderByDescending(s => s.TotalQuantity)
            .ToList();

    // 4) Підсумкові агрегати з захистом від порожньої колекції
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity), list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }
    // 5) GroupJoin товарів і рухів у зведений DTO
    public static IReadOnlyList<ProductMovementSummary> MovementSummary(
        IEnumerable<Product> products,
        IEnumerable<StockMovement> movements) =>
        products
            .GroupJoin(
                movements,
                p => p.Id,
                m => m.ProductId,
                (p, ms) =>
                {
                    var list = ms.ToList();
                    return new ProductMovementSummary(
                        p.Sku,
                        p.Name,
                        list.Count,
                        list.Sum(m => m.Delta));
                })
            .OrderByDescending(s => s.MovementsCount)
            .ThenBy(s => s.Sku, StringComparer.Ordinal)
            .ToList();
}

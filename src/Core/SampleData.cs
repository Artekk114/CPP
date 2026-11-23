using System.Collections.Generic;
using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Product> Products() =>
    [
        Product.Create("p001", "SKU-101", "Кабель UTP cat6", "м", 18.50m, 240),
        Product.Create("p002", "SKU-102", "Кабель FTP cat6a", "м", 24.00m, 120),
        Product.Create("p003", "SKU-103", "Кабель живлення 3x1.5", "м", 32.00m, 85),
        Product.Create("p004", "SKU-104", "Гофра ПВХ 20мм", "м", 9.40m, 160),
        Product.Create("p005", "SKU-105", "Автомат 16A 1P", "шт", 145.00m, 42),
        Product.Create("p006", "SKU-106", "Дифреле 25A 30mA", "шт", 890.00m, 14),
        Product.Create("p007", "SKU-107", "Розетка подвійна", "шт", 115.00m, 65),
        Product.Create("p008", "SKU-108", "Вимикач 2-клавішний", "шт", 98.00m, 7),
        Product.Create("p009", "SKU-109", "Клемник WAGO 221-413", "шт", 14.20m, 190),
        Product.Create("p010", "SKU-110", "Патч-корд RJ45 2м", "шт", 75.00m, 4),
        Product.Create("p011", "SKU-111", "Дюбель 6x40", "уп", 65.00m, 55),
        Product.Create("p012", "SKU-112", "Саморіз по гіпсокартону", "уп", 110.00m, 38),
        Product.Create("p013", "SKU-113", "Кабельні стяжки 200мм", "уп", 48.00m, 9),
        Product.Create("p014", "SKU-114", "Ізолента ПВХ синя", "уп", 29.00m, 72),
        Product.Create("p015", "SKU-115", "Припій ПОС-61", "кг", 620.00m, 12),
        Product.Create("p016", "SKU-116", "Флюс паяльний RMA-223", "кг", 540.00m, 6),
        Product.Create("p017", "SKU-117", "Силікон герметик", "шт", 135.00m, 28),
        Product.Create("p018", "SKU-118", "Лоток кабельний 100x50", "м", 190.00m, 45)
    ];
}
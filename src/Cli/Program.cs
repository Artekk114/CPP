using System;
using Core.Domain;

Console.WriteLine("Все успішно");
Order order = Order.Create("ORD-001");
order.AddLine("P-101", "Ноутбук", 25000m, 1);
order.AddLine("P-102", "Мишка", 800m, 2);
Console.WriteLine($"Замовлення {order.Id} створено. Рядків: {order.Lines.Count}. Сума: {order.Total}");

order.Confirm();
Console.WriteLine($"Замовлення {order.Id} підтверджено: {order.IsConfirmed}\n");


Console.WriteLine("Порушення інваріантів");
TryDo("Порожній ID замовлення", () => Order.Create("   "));

TryDo("Додавання рядка до підтвердженого замовлення", () => order.AddLine("P-103", "Килимок", 300m, 1));

Order emptyOrder = Order.Create("ORD-002");
TryDo("Від'ємна ціна товару", () => emptyOrder.AddLine("P-201", "Брак", -100m, 1));
TryDo("Нульова кількість товару", () => emptyOrder.AddLine("P-202", "Тест", 50m, 0));
TryDo("Підтвердження порожнього замовлення", () => emptyOrder.Confirm());

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював, інваріант відсутній");
    }
    catch (Exception ex) { 
    
        Console.WriteLine($" {title}: {ex.GetType().Name} - {ex.Message}");
    }
}
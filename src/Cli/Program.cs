using System;
using Core.Domain;

Console.WriteLine("Все успішно");
Order order = Order.Create("ORD-001");
order.AddLine("P-101", "Ноутбук", 25000m, 1);
order.AddLine("P-102", "Мишка", 800m, 2);
Console.WriteLine($"Замовлення {order.Id} створено. Рядків: {order.Lines.Count}. Сума: {order.Total}");
Console.WriteLine($"Поточний статус: {order.Status}");

order.Confirm();
Console.WriteLine($"Замовлення {order.Id} підтверджено. Новий статус: {order.Status}\n");


Console.WriteLine("Порушення інваріантів");
TryDo("Порожній ID замовлення", () => Order.Create("   "));

TryDo("Додавання рядка до підтвердженого замовлення", () => order.AddLine("P-103", "Килимок", 300m, 1));

Order emptyOrder = Order.Create("ORD-002");
TryDo("Від'ємна ціна товару", () => emptyOrder.AddLine("P-201", "Брак", -100m, 1));
TryDo("Нульова кількість товару", () => emptyOrder.AddLine("P-202", "Тест", 50m, 0));
TryDo("Підтвердження порожнього замовлення", () => emptyOrder.Confirm());
Console.WriteLine("\n--- Тестування статусів (Додаткове завдання) ---");

Order orderToCancel = Order.Create("ORD-003");
orderToCancel.Cancel();
Console.WriteLine($"Замовлення {orderToCancel.Id} переведено в статус: {orderToCancel.Status}");

TryDo("Додавання товару до скасованого замовлення", () => orderToCancel.AddLine("P-301", "Кабель", 100m, 1));
TryDo("Підтвердження скасованого замовлення", () => orderToCancel.Confirm());
TryDo("Скасування підтвердженого замовлення", () => order.Cancel()); 
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
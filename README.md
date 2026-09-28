# CrossApp
Наскрізний проєкт з крос-платформного програмування.

Предметна область: Замовлення. Сутності: Customer (клієнт), Product (товар), Order (замовлення), OrderLine (рядок замовлення).
Призначення: оформлення замовлень і підрахунок сум.

## Запуск
```bash
dotnet build
dotnet run --project src/Cli

RID	          Режим	                 Розмір publish	              Потрібен runtime
win-x64	  self-contained	          ~76.8 МБ	                       ні
win-x64	  framework-dependent	      ~0.19 МБ	                    так (.NET 10)
NET SDK 10.0, Windows 11 x6

using Lab2;

// Порядок: демонстрация, задания 1-3, затем задания 4-7
// Задание 7 запускается отдельно аргументом "task7", чтобы утечка не влияла на замеры остальных заданий
if (args.Length > 0 && args[0] == "task7")
{
    Task7.Run();
    return;
}

// Аргумент "snapshots": этапы для снимков памяти в профилировщике
if (args.Length > 0 && args[0] == "snapshots")
{
    Snapshots.Run();
    return;
}

// ГДЕ ЧТО: Demo - статика vs экземпляр; Task1 - задание 1; Task2And3 (MemoryMonitor.cs) - задания 2-3;
// Task4..Task7 - одноимённые задания. Запуск: dotnet run -c Release [-- task7 | -- snapshots]
Demo.Run();
Task1.Run();
Task2And3.Run();
Task4.Run();
Task5.Run();
Task6.Run();

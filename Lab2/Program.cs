using Lab2;

// Запуск: dotnet run -c Release [-- task7 | -- snapshots]
if (args.Length > 0 && args[0] == "task7")
{
    Task7.Run();
    return;
}

if (args.Length > 0 && args[0] == "snapshots")
{
    Snapshots.Run();
    return;
}

Demo.Run();
Task1.Run();
Task2And3.Run();
Task4.Run();
Task5.Run();
Task6.Run();

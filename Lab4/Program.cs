using Lab4;

// Аргумент "calltree": задание по профилированию (Call Tree и собственный код)
// ГДЕ ЧТО: BaseCode.cs - базовый код методички; Tasks.cs - задания 1.1-3.2;
// BuggyCode.cs - задание 4 (поиск утечек, режим buggy); CallTree.cs + OwnCode.cs - профилирование (режим calltree)
if (args.Length > 0 && args[0] == "calltree")
{
    // Прогрев JIT без вывода, чтобы замеры сравнивались в одинаковых условиях
    // Первый прогон без вывода - прогрев JIT, чтобы замеры были честными
    var output = Console.Out;
    Console.SetOut(TextWriter.Null);
    CallTree.Run(parseExact: false);
    Console.SetOut(output);

    Console.WriteLine("=== Call Tree: DateTime.TryParse ===\n");
    CallTree.Run(parseExact: false);
    Console.WriteLine("\n=== Call Tree: DateTime.ParseExact ===\n");
    // Сравнение TryParse и ParseExact - горячий путь IsAnomaly (разбор даты)
    CallTree.Run(parseExact: true);
    OwnCode.Run();
    return;
}

// Аргумент "buggy": задание 4 (BuggyCode и исправленная версия)
if (args.Length > 0 && args[0] == "buggy")
{
    LeakSearch.Run();
    LeakSearch.RunFixed();
    return;
}

// Режим по умолчанию: базовый код и задания 1.1-3.2 по порядку
BaseCode.Run();
Tasks.Task1_1();
Tasks.Task1_2();
Tasks.Task2_1();
Tasks.Task2_2();
Tasks.Task3_1();
Tasks.Task3_2();

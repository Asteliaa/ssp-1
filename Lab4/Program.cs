// ТЕОРИЯ К ЗАЩИТЕ (ЛР4): утечки и профилирование
// ВИДЫ УТЕЧЕК: статические ссылки (корень GC живёт вечно), события (подписчик не отписан), кэши без вытеснения,
// неосвобождённые IDisposable, замыкания (лямбда держит большой объект), долгоживущие объекты (синглтон, поток).
// Признак: память растёт и после GC.Collect() не падает. Поиск: снимки памяти и путь к корню (GC root).
// ГОРЯЧИЙ ПУТЬ (hot path) - цепочка вызовов в Call Tree, тратящая больше всего времени; оптимизируют её в первую очередь.
// Own (Self) Time - время самого метода без вызванных; Total Time - вместе с вызванными.
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

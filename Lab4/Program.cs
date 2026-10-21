using Lab4;

// Режимы: без аргумента - базовый код и задания 1-3; buggy - задание 4; calltree - Call Tree и свой код
if (args.Length > 0 && args[0] == "calltree")
{
    // Прогрев JIT без вывода
    var output = Console.Out;
    Console.SetOut(TextWriter.Null);
    CallTree.Run(parseExact: false);
    CallTree.Run(parseExact: true);
    Console.SetOut(output);

    Console.WriteLine("=== Call Tree: DateTime.TryParse ===\n");
    CallTree.Run(parseExact: false);
    Console.WriteLine("\n=== Call Tree: DateTime.ParseExact ===\n");
    CallTree.Run(parseExact: true);
    OwnCode.Run();
    return;
}

if (args.Length > 0 && args[0] == "buggy")
{
    LeakSearch.Run();
    LeakSearch.RunFixed();
    return;
}

BaseCode.Run();
Tasks.Task1_1();
Tasks.Task1_2();
Tasks.Task2_1();
Tasks.Task2_2();
Tasks.Task3_1();
Tasks.Task3_2();

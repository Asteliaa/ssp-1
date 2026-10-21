using LabReflection;

// dotnet run -c Release [-- extra]
if (args.Length > 0 && args[0] == "extra")
{
    PluginManagerDemo.Run();
    return;
}

Examples.InspectType();
Examples.PrivateMembers();
Examples.LateBinding();
ExamplesExtra.PluginInspection();
ExamplesExtra.InvokeMembers();

Tasks.Task1();
Tasks.Task2();
Tasks.Task3();
Tasks.Task4();
Tasks.Task5();
Tasks.Task6();
PluginLoader.Task7();

PluginManagerDemo.Run();

if (!Console.IsInputRedirected)
{
    Console.WriteLine("\nНажмите любую клавишу для завершения...");
    Console.ReadKey();
}

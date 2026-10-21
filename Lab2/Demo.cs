namespace Lab2;

public class StaticReferenceDemo
{
    // статическая ссылка - одна для всех объектов
    private static List<int> _staticList = new List<int>();

    // экземплярная ссылка - своя у каждого объекта
    private List<int> _instanceList = new List<int>();

    public void AddToStatic(int value)
    {
        _staticList.Add(value);
    }

    public void AddToInstance(int value)
    {
        _instanceList.Add(value);
    }

    public void PrintLists(string objectName)
    {
        Console.WriteLine($"{objectName}:");
        Console.WriteLine($"  Статический список (общий): [{string.Join(", ", _staticList)}]");
        Console.WriteLine($"  Экземплярный список (свой): [{string.Join(", ", _instanceList)}]");
        Console.WriteLine();
    }
}

public class StaticFieldDemo
{
    private static int _staticCounter = 0;
    private int _instanceCounter = 0;

    public void Increment()
    {
        _staticCounter++;
        _instanceCounter++;
    }

    public void ShowCounters(string objectName)
    {
        Console.WriteLine($"{objectName}:");
        Console.WriteLine($"  Статический счетчик (общий): {_staticCounter}");
        Console.WriteLine($"  Экземплярный счетчик (свой): {_instanceCounter}");
    }

    public static void ShowStaticFieldDemo()
    {
        var objA = new StaticFieldDemo();
        var objB = new StaticFieldDemo();

        objA.Increment();
        objA.Increment();
        objB.Increment();

        Console.WriteLine("После операций:");
        objA.ShowCounters("Объект A");
        objB.ShowCounters("Объект B");

        Console.WriteLine("\nСтатический счетчик ОДИН для обоих объектов!");
        Console.WriteLine("Экземплярные счетчики РАЗНЫЕ для каждого!");
    }
}

public static class Demo
{
    public static void Run()
    {
        Console.WriteLine("=== ОДНА статическая ссылка на ВСЕ объекты ===\n");

        var obj1 = new StaticReferenceDemo();
        var obj2 = new StaticReferenceDemo();
        var obj3 = new StaticReferenceDemo();

        Console.WriteLine("1. Добавляем данные через разные объекты:\n");

        obj1.AddToStatic(100);
        obj1.AddToInstance(1);

        obj2.AddToStatic(200);
        obj2.AddToInstance(2);

        obj3.AddToStatic(300);
        obj3.AddToInstance(3);

        obj1.PrintLists("Объект 1");
        obj2.PrintLists("Объект 2");
        obj3.PrintLists("Объект 3");

        Console.WriteLine("Статический список ОДИН для всех!");
        Console.WriteLine("Экземплярные списки РАЗНЫЕ для каждого!\n");

        Console.WriteLine("2. Демонстрация через статическое поле:\n");
        StaticFieldDemo.ShowStaticFieldDemo();
    }
}

using System.Diagnostics;

namespace Lab2;

// Задание 1: базовый пример с утечкой памяти
public class MemoryLeakExample
{
    // static-поле = корень GC: всё, что в списке, не собирается никогда
    private static List<byte[]> _staticList = new List<byte[]>();

    public static void CreateMemoryLeak()
    {
        // Создаем объекты, которые никогда не будут собраны
        for (int i = 0; i < 1000; i++)
        {
            _staticList.Add(new byte[1024 * 10]); // 10KB каждый
        }
    }

    public static void ClearMemoryLeak()
    {
        _staticList.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}

public class EventLeakExample
{
    public event EventHandler? BigEvent;

    public void SubscribeLeak()
    {
        // Подписка на событие без отписки
        // Лямбда подписана на событие и не отписывается - объект-подписчик остаётся жить
        BigEvent += (sender, e) =>
        {
            var data = new byte[1024 * 100];
        };
    }
}

// ТЕОРИЯ: boxing = упаковка значения (int) в объект кучи (object/ArrayList/List<object>):
// память на каждый элемент, больше работы GC, unboxing проверяет тип. Лечение: List<int> (обобщения).
public static class Task1
{
    public static void Run()
    {
        Console.WriteLine("=== Лабораторная работа: Управление памятью ===");
        var monitor = new MemoryMonitor();

        // 1. Тест памяти и LOH
        Console.WriteLine("\n1. Выделение памяти:");
        // 10MB в обычной куче; затем 100 объектов >85KB попадают в LOH (Gen2)
        monitor.AllocateMemory(10);
        monitor.AllocateLOHObjects(100);
        monitor.PrintMemoryInfo("После выделения");

        // 2. Тест утечек
        Console.WriteLine("\n2. Тест утечек:");
        MemoryLeakExample.CreateMemoryLeak();
        var eventExample = new EventLeakExample();
        for (int i = 0; i < 100; i++)
            eventExample.SubscribeLeak();
        monitor.PrintMemoryInfo("После утечек");

        // 3. Тест Boxing
        Console.WriteLine("\n3. Тест упаковки:");
        var sw = Stopwatch.StartNew();
        // Boxing: каждый int превращается в объект в куче - медленнее и больше мусора
        monitor.SimulateBoxing(1000000);
        sw.Stop();
        Console.WriteLine($"Время с упаковкой: {sw.ElapsedMilliseconds}ms");

        // Без упаковки
        sw.Restart();
        // Тот же цикл без упаковки: List<int> хранит значения внутри массива
        var optimizedList = new List<int>();
        for (int i = 0; i < 1000000; i++)
            optimizedList.Add(i);
        sw.Stop();
        Console.WriteLine($"Время без упаковки: {sw.ElapsedMilliseconds}ms");

        // 4. Очистка
        Console.WriteLine("\n4. После очистки:");
        // Очистка списка + принудительный GC: память должна вернуться
        MemoryLeakExample.ClearMemoryLeak();
        monitor.Cleanup();
        monitor.PrintMemoryInfo("Очищено");

        // 5. Поколения объектов
        Console.WriteLine("\n5. Поколения объектов:");
        // Поколения: новый объект Gen0, после GC.Collect() переезжает в Gen1, потом Gen2
        var obj = new object();
        Console.WriteLine($"Начальное поколение: {GC.GetGeneration(obj)}");
        GC.Collect();
        Console.WriteLine($"После GC: {GC.GetGeneration(obj)}");

        GC.KeepAlive(eventExample);
        GC.KeepAlive(optimizedList);
    }
}

// Large Object Heap - пример
public class LOHExample
{
    public static void CreateLargeObjects()
    {
        // Объекты > 85KB попадают в LOH
        for (int i = 0; i < 100; i++)
        {
            var largeArray = new byte[100000]; // ~100KB
            // Используем largeArray
        }
    }
}

// Boxing и его влияние
public class BoxingExample
{
    private List<object> _boxedItems = new List<object>();
    private List<int> _unboxedItems = new List<int>();

    public void BoxingOperation()
    {
        // Упаковка - создает дополнительные объекты в куче
        for (int i = 0; i < 1000000; i++)
        {
            // Здесь упаковка int -> object
            _boxedItems.Add(i); // int упаковывается в object
        }
    }

    public void UnboxedOperation()
    {
        // Без упаковки - эффективнее
        for (int i = 0; i < 1000000; i++)
        {
            _unboxedItems.Add(i);
        }
    }
}

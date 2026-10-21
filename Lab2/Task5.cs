namespace Lab2;

// Задание 5: Large Object Heap (LOH)
public static class Task5
{
    static List<byte[]> _keep = new List<byte[]>();

    public static void Run()
    {
        Console.WriteLine("\n=== Задание 5: объекты в LOH ===");

        long before = GC.GetTotalMemory(false);
        Console.WriteLine($"Память до выделения: {before / 1024} KB");

        for (int i = 0; i < 100; i++)
            _keep.Add(new byte[100 * 1024]); // 100KB > 85KB -> LOH

        long after = GC.GetTotalMemory(false);
        Console.WriteLine($"Память после выделения 100 x 100KB: {after / 1024} KB");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        Console.WriteLine($"Память после GC.Collect() (ссылки удерживаются): {GC.GetTotalMemory(false) / 1024} KB");

        _keep.Clear();
        GC.Collect(2); // LOH собирается только вместе с Gen2
        GC.WaitForPendingFinalizers();
        Console.WriteLine($"Память после очистки списка и GC.Collect(2): {GC.GetTotalMemory(false) / 1024} KB");

        var small = new byte[80 * 1024];
        var large = new byte[90 * 1024];
        Console.WriteLine("\nСравнение размещения:");
        Console.WriteLine($"80KB ({small.Length} байт): Gen{GC.GetGeneration(small)}");
        Console.WriteLine($"90KB ({large.Length} байт): Gen{GC.GetGeneration(large)}");

        MonitorLoh();
    }

    static void MonitorLoh()
    {
        Console.WriteLine("\n=== Задание 5: мониторинг LOH ===");
        int[] sizesKb = { 10, 50, 90, 200 };
        var keep = new List<byte[]>();
        foreach (int kb in sizesKb)
        {
            var array = new byte[kb * 1024];
            keep.Add(array);
            string heap = array.Length >= 85_000 ? "LOH" : "обычная куча (SOH)";
            Console.WriteLine($"Объект {kb}KB ({array.Length} байт): Gen{GC.GetGeneration(array)}, {heap}");
        }
        Console.WriteLine($"Сборок Gen0: {GC.CollectionCount(0)}, Gen1: {GC.CollectionCount(1)}, Gen2: {GC.CollectionCount(2)}");
        GC.KeepAlive(keep);
    }
}

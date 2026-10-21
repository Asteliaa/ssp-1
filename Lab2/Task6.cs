using System.Collections;
using System.Diagnostics;

namespace Lab2;

// Задание 6: оптимизация через устранение упаковки (Boxing Elimination)
public class UnoptimizedCode
{
    private ArrayList _data = new ArrayList();

    public void AddData(int value)
    {
        _data.Add(value); // Упаковка!
    }

    public int GetSum()
    {
        int sum = 0;
        foreach (object item in _data) // Распаковка!
        {
            sum += (int)item;
        }
        return sum;
    }
}

public class OptimizedCode
{
    private List<int> _data = new List<int>();

    public void AddData(int value)
    {
        _data.Add(value);
    }

    public int GetSum()
    {
        int sum = 0;
        foreach (int item in _data)
        {
            sum += item;
        }
        return sum;
    }
}

public static class Task6
{
    const int Count = 1_000_000;

    static object ProcessWithBoxing()
    {
        var list = new List<object>();
        for (int i = 0; i < Count; i++)
            list.Add(i);
        return list;
    }

    static object ProcessWithoutBoxing()
    {
        var list = new List<int>();
        for (int i = 0; i < Count; i++)
            list.Add(i);
        return list;
    }

    // время (мс) и прирост памяти (KB) по GC.GetTotalMemory
    static (long ms, long kb) Measure(Func<object> action)
    {
        long memBefore = GC.GetTotalMemory(true);
        var sw = Stopwatch.StartNew();
        object result = action();
        sw.Stop();
        long memAfter = GC.GetTotalMemory(false);
        GC.KeepAlive(result);
        return (sw.ElapsedMilliseconds, (memAfter - memBefore) / 1024);
    }

    public static void Run()
    {
        Console.WriteLine("\n=== Задание 6.1: сравнение производительности (1 000 000 операций) ===");

        ProcessWithBoxing(); // прогрев JIT
        ProcessWithoutBoxing();

        var boxing = Measure(ProcessWithBoxing);
        var noBoxing = Measure(ProcessWithoutBoxing);

        Console.WriteLine($"{"Метод",-25}{"Время, мс",12}{"Память, KB",14}");
        Console.WriteLine($"{"ProcessWithBoxing",-25}{boxing.ms,12}{boxing.kb,14}");
        Console.WriteLine($"{"ProcessWithoutBoxing",-25}{noBoxing.ms,12}{noBoxing.kb,14}");
        Console.WriteLine($"Без упаковки быстрее в {(double)boxing.ms / Math.Max(1, noBoxing.ms):F1} раза, памяти меньше в {(double)boxing.kb / Math.Max(1, noBoxing.kb):F1} раза");

        Console.WriteLine("\n=== Задание 6.2: оптимизация кода с ArrayList ===");

        var before = Measure(() =>
        {
            var code = new UnoptimizedCode();
            for (int i = 0; i < Count; i++) code.AddData(i);
            code.GetSum();
            return code;
        });
        var after = Measure(() =>
        {
            var code = new OptimizedCode();
            for (int i = 0; i < Count; i++) code.AddData(i);
            code.GetSum();
            return code;
        });

        Console.WriteLine($"{"Класс",-25}{"Время, мс",12}{"Память, KB",14}");
        Console.WriteLine($"{"UnoptimizedCode",-25}{before.ms,12}{before.kb,14}");
        Console.WriteLine($"{"OptimizedCode",-25}{after.ms,12}{after.kb,14}");
    }
}

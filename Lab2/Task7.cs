namespace Lab2;

// Задание 7: поиск утечек в коде
public class BuggyCode
{
    private static List<IDisposable> _cache = new List<IDisposable>();
    private static event EventHandler? _globalEvent;
    private byte[] _data = new byte[1024 * 1024];

    public BuggyCode()
    {
        _globalEvent += OnEvent;              // утечка 1: подписка на статическое событие без отписки
        _cache.Add(new MemoryStream(1024));   // утечка 2: статический кэш не освобождает потоки
    }

    private void OnEvent(object? sender, EventArgs e)
    {
        var temp = new byte[1024 * 100];
    }

    public void SubscribeAll()
    {
        for (int i = 0; i < 1000; i++)
        {
            new BuggyCode();                  // утечка 3: _data (1MB) каждого объекта держит событие
        }
    }

    public static int CacheCount => _cache.Count;
}

public class FixedCode : IDisposable
{
    private static List<IDisposable> _cache = new List<IDisposable>();
    private static event EventHandler? _globalEvent;
    private byte[]? _data = new byte[1024 * 1024];
    private MemoryStream _stream = new MemoryStream(1024);

    public FixedCode()
    {
        _globalEvent += OnEvent;
        _cache.Add(_stream);
    }

    private void OnEvent(object? sender, EventArgs e)
    {
        var temp = new byte[1024 * 100];
    }

    public void SubscribeAll()
    {
        for (int i = 0; i < 1000; i++)
        {
            using (new FixedCode())
            {
            }
        }
    }

    public void Dispose()
    {
        _globalEvent -= OnEvent;
        _cache.Remove(_stream);
        _stream.Dispose();
        _data = null;
    }

    public static int CacheCount => _cache.Count;
}

public static class Task7
{
    public static void Run()
    {
        Console.WriteLine("\n=== Задание 7: код с утечками ===");
        Console.WriteLine($"Память до: {GC.GetTotalMemory(true) / (1024 * 1024)} MB");
        new BuggyCode().SubscribeAll();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Console.WriteLine($"Память после SubscribeAll() и GC.Collect(): {GC.GetTotalMemory(false) / (1024 * 1024)} MB");
        Console.WriteLine($"Объектов в статическом кэше: {BuggyCode.CacheCount}");

        Console.WriteLine("\n=== Задание 7: исправленная версия ===");
        Console.WriteLine($"Память до: {GC.GetTotalMemory(false) / (1024 * 1024)} MB");
        using (var fixedCode = new FixedCode())
        {
            fixedCode.SubscribeAll();
        }
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Console.WriteLine($"Память после SubscribeAll() и GC.Collect(): {GC.GetTotalMemory(false) / (1024 * 1024)} MB");
        Console.WriteLine($"Объектов в статическом кэше: {FixedCode.CacheCount}");
    }
}

namespace Lab5;

// Искусственный deadlock (код методических указаний) и два способа борьбы с ним
public static class DeadlockDemo
{
    // Два объекта для блокировки
    private static object _lockA = new object();
    private static object _lockB = new object();

    // Исходная программа: потоки берут блокировки в разном порядке, программа зависает
    public static void Run()
    {
        Console.WriteLine("=== Демонстрация Deadlock ===\n");
        Task task1 = Task.Run(() => Thread1());
        Task task2 = Task.Run(() => Thread2());
        Console.WriteLine("Ожидание завершения потоков... (программа зависнет)");
        Console.WriteLine("Для диагностики откройте окно 'Parallel Stacks' в VS.");
        Task.WaitAll(task1, task2); // Здесь программа зависнет навсегда
        Console.WriteLine("Программа завершена (это сообщение не будет выведено)");
    }

    static void Thread1()
    {
        lock (_lockA)
        {
            Console.WriteLine("Thread1: захватил lockA");
            Thread.Sleep(1000); // Имитация работы
            lock (_lockB)
            {
                Console.WriteLine("Thread1: захватил lockB");
            }
        }
    }

    static void Thread2()
    {
        lock (_lockB)
        {
            Console.WriteLine("Thread2: захватил lockB");
            Thread.Sleep(1000); // Имитация работы
            lock (_lockA)
            {
                Console.WriteLine("Thread2: захватил lockA");
            }
        }
    }

    // Задание 2: оба потока захватывают блокировки в одинаковом порядке (сначала A, потом B)
    public static void RunFixed()
    {
        Console.WriteLine("=== Deadlock исправлен: одинаковый порядок захвата ===\n");
        Task task1 = Task.Run(() => Thread1());
        Task task2 = Task.Run(() => Thread2Fixed());
        Task.WaitAll(task1, task2);
        Console.WriteLine("Программа завершена");
    }

    static void Thread2Fixed()
    {
        lock (_lockA)
        {
            Console.WriteLine("Thread2: захватил lockA");
            Thread.Sleep(1000); // Имитация работы
            lock (_lockB)
            {
                Console.WriteLine("Thread2: захватил lockB");
            }
        }
    }

    // Задание 3: Monitor.TryEnter с тайм-аутом вместо lock
    public static void RunTimeout()
    {
        Console.WriteLine("=== Обнаружение deadlock по тайм-ауту (Monitor.TryEnter) ===\n");
        Task task1 = Task.Run(() => TakeBoth("Thread1", _lockA, "lockA", _lockB, "lockB"));
        Task task2 = Task.Run(() => TakeBoth("Thread2", _lockB, "lockB", _lockA, "lockA"));
        Task.WaitAll(task1, task2);
        Console.WriteLine("Программа завершена: зависания нет, потоки сообщили о тайм-ауте");
    }

    static void TakeBoth(string name, object first, string firstName, object second, string secondName)
    {
        if (!Monitor.TryEnter(first, TimeSpan.FromSeconds(3)))
        {
            Console.WriteLine($"{name}: не удалось захватить {firstName} за 3 секунды");
            return;
        }
        try
        {
            Console.WriteLine($"{name}: захватил {firstName}");
            Thread.Sleep(1000); // Имитация работы
            if (Monitor.TryEnter(second, TimeSpan.FromSeconds(3)))
            {
                try { Console.WriteLine($"{name}: захватил {secondName}"); }
                finally { Monitor.Exit(second); }
            }
            else
            {
                Console.WriteLine($"{name}: не удалось захватить {secondName} за 3 секунды – возможен deadlock");
            }
        }
        finally
        {
            Monitor.Exit(first);
        }
    }
}

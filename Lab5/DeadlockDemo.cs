namespace Lab5;

// Deadlock: демонстрация, исправление единым порядком, обнаружение по тайм-ауту
public static class DeadlockDemo
{
    // Два объекта для блокировки
    private static object _lockA = new object();
    private static object _lockB = new object();

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

// Обедающие философы: потоки = философы, ресурсы = палочки (рис ресурсом не является)
public static class Philosophers
{
    const int N = 5;
    static readonly object[] Sticks = Enumerable.Range(0, N).Select(_ => new object()).ToArray();

    // fixedOrder=false: все берут сначала левую палочку -> deadlock; true: сначала палочку с меньшим номером
    public static void Run(bool fixedOrder)
    {
        Console.WriteLine($"=== Философы ({(fixedOrder ? "единый порядок палочек" : "левая, потом правая - возможен deadlock")}) ===\n");
        var tasks = new Task[N];
        for (int i = 0; i < N; i++)
        {
            int id = i;
            tasks[i] = Task.Run(() => Dine(id, fixedOrder));
        }
        if (!Task.WaitAll(tasks, 8000))
        {
            Console.WriteLine("\nDEADLOCK: за 8 секунд философы не доели - каждый держит левую палочку и ждёт правую");
            return;
        }
        Console.WriteLine("\nВсе философы поели, deadlock нет");
    }

    static void Dine(int id, bool fixedOrder)
    {
        int left = id, right = (id + 1) % N;
        int first = left, second = right;
        if (fixedOrder && left > right) (first, second) = (right, left);
        for (int round = 0; round < 3; round++)
        {
            lock (Sticks[first])
            {
                Console.WriteLine($"Философ {id}: взял палочку {first}");
                Thread.Sleep(100);
                lock (Sticks[second])
                {
                    Console.WriteLine($"Философ {id}: взял палочку {second}, ест (раунд {round + 1})");
                    Thread.Sleep(50);
                }
            }
        }
    }
}

// Ещё два способа лечения философов: официант (семафор N-1) и тайм-аут с откатом
public static class PhilosophersFixes
{
    const int N = 5;
    static readonly object[] Sticks = Enumerable.Range(0, N).Select(_ => new object()).ToArray();

    public static void RunWaiter()
    {
        Console.WriteLine("=== Философы: официант пускает за стол не более N-1 человек ===\n");
        var waiter = new SemaphoreSlim(N - 1, N - 1);
        var tasks = new Task[N];
        for (int i = 0; i < N; i++)
        {
            int id = i;
            tasks[i] = Task.Run(() =>
            {
                for (int round = 0; round < 3; round++)
                {
                    waiter.Wait();
                    lock (Sticks[id])
                    {
                        Thread.Sleep(100);
                        lock (Sticks[(id + 1) % N])
                        {
                            Console.WriteLine($"Философ {id}: ест (раунд {round + 1})");
                            Thread.Sleep(50);
                        }
                    }
                    waiter.Release();
                }
            });
        }
        Console.WriteLine(Task.WaitAll(tasks, 8000) ? "\nВсе философы поели, deadlock нет" : "\nDEADLOCK");
    }

    public static void RunTimeout()
    {
        Console.WriteLine("=== Философы: тайм-аут второй палочки, откат и повтор ===\n");
        var tasks = new Task[N];
        for (int i = 0; i < N; i++)
        {
            int id = i;
            tasks[i] = Task.Run(() =>
            {
                var rnd = new Random(id);
                int round = 0;
                while (round < 3)
                {
                    lock (Sticks[id])
                    {
                        Thread.Sleep(100);
                        if (Monitor.TryEnter(Sticks[(id + 1) % N], 200))
                        {
                            try
                            {
                                Console.WriteLine($"Философ {id}: ест (раунд {round + 1})");
                                Thread.Sleep(50);
                                round++;
                            }
                            finally { Monitor.Exit(Sticks[(id + 1) % N]); }
                        }
                        else Console.WriteLine($"Философ {id}: не взял вторую палочку, кладёт первую и повторит");
                    }
                    Thread.Sleep(rnd.Next(10, 80));
                }
            });
        }
        Console.WriteLine(Task.WaitAll(tasks, 15000) ? "\nВсе философы поели, deadlock нет" : "\nDEADLOCK");
    }
}

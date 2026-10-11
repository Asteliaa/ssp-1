namespace Lab5;

public static class MutexCrossProcess
{
    // Именованный Mutex для синхронизации между процессами
    public static void Run()
    {
        using (Mutex mutex = new Mutex(false, "Global\\MyAppMutex"))
        {
            Console.WriteLine("Попытка захвата глобального мьютекса...");
            if (mutex.WaitOne(TimeSpan.FromSeconds(3)))
            {
                try
                {
                    Console.WriteLine("Мьютекс захвачен. Программа выполняется.");
                    Console.WriteLine("Нажмите Enter для освобождения мьютекса...");
                    Console.ReadLine();
                }
                finally
                {
                    mutex.ReleaseMutex();
                    Console.WriteLine("Мьютекс освобожден.");
                }
            }
            else
            {
                Console.WriteLine("Не удалось захватить мьютекс (возможно, уже запущен другой экземпляр).");
            }
        }
    }
}

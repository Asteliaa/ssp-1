namespace Lab5;

// ТЕОРИЯ: Mutex - взаимное исключение с владельцем (освобождает тот же поток); именованный Global\Имя виден всем
// процессам, поэтому второй экземпляр программы не может его захватить. Semaphore владельца не имеет и пускает до N потоков.
public static class MutexCrossProcess
{
    // Именованный Mutex для синхронизации между процессами
    public static void Run()
    {
        // Именованный Global\ мьютекс виден всем процессам: запусти 2 окна - второе не захватит
        using (Mutex mutex = new Mutex(false, "Global\\MyAppMutex"))
        {
            Console.WriteLine("Попытка захвата глобального мьютекса...");
            // Ждём 3 секунды; если занят другим процессом - сообщение
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

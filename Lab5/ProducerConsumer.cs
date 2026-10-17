namespace Lab5;

// Паттерн Producer-Consumer с семафорами и мьютексом (код методических указаний)
public static class ProducerConsumer
{
    // Общий буфер (ограниченная очередь)
    // Общий буфер - критический ресурс; его защищает Mutex
    private static Queue<int> _buffer = new Queue<int>();
    // _emptySlots = сколько свободных мест (производитель ждёт), _filledSlots = сколько занятых (потребитель ждёт)
    private static SemaphoreSlim _emptySlots = null!;
    private static SemaphoreSlim _filledSlots = null!;
    // Мьютекс для защиты доступа к буферу
    private static Mutex _mutex = new Mutex();
    // Флаг остановки потоков
    private static bool _isRunning = true;

    public static void Run(int bufferSize, int producers, int consumers)
    {
        _buffer = new Queue<int>();
        // Свободных мест сначала bufferSize, занятых 0
        _emptySlots = new SemaphoreSlim(bufferSize, bufferSize);
        _filledSlots = new SemaphoreSlim(0, bufferSize);
        _isRunning = true;

        Console.WriteLine($"=== Producer-Consumer with Semaphore (буфер {bufferSize}, производителей {producers}, потребителей {consumers}) ===\n");

        var tasks = new List<Task>();
        // Запускаем производителей и потребителей как задачи Task
        for (int i = 1; i <= producers; i++) { int id = i; tasks.Add(Task.Run(() => Producer(id))); }
        for (int i = 1; i <= consumers; i++) { int id = i; tasks.Add(Task.Run(() => Consumer(id))); }

        // Работаем 5 секунд
        // Работаем 5 секунд, потом флаг остановки и ожидание задач
        Thread.Sleep(5000);
        _isRunning = false;

        // Ждем завершения всех задач
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine("\nВсе потоки завершены.");
    }

    // Метод производителя
    // ПРОИЗВОДИТЕЛЬ: ждёт свободное место -> Mutex -> Enqueue -> Release занятых мест
    static async Task Producer(int id)
    {
        Random rand = new Random(id);
        while (_isRunning)
        {
            // Ждем, пока освободится место в буфере (с тайм-аутом, чтобы после остановки поток мог завершиться)
            if (!await _emptySlots.WaitAsync(500)) continue;
            // Захватываем мьютекс для безопасной работы с буфером
            _mutex.WaitOne();
            try
            {
                int item = rand.Next(1, 100);
                _buffer.Enqueue(item);
                Console.WriteLine($"[Producer {id}] Добавлен элемент: {item}. В буфере: {_buffer.Count} элементов.");
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            // Увеличиваем счетчик занятых мест
            _filledSlots.Release();
            // Имитация времени на производство
            await Task.Delay(rand.Next(200, 600));
        }
        Console.WriteLine($"[Producer {id}] Завершен.");
    }

    // Метод потребителя
    // ПОТРЕБИТЕЛЬ: ждёт занятое место -> Mutex -> Dequeue -> Release свободных мест
    static async Task Consumer(int id)
    {
        Random rand = new Random(id + 10);
        while (_isRunning)
        {
            // Ждем, пока появится хотя бы один элемент в буфере (с тайм-аутом по той же причине)
            if (!await _filledSlots.WaitAsync(500)) continue;
            // Захватываем мьютекс
            // Критическая секция: к буферу только один поток; ReleaseMutex в finally
            _mutex.WaitOne();
            int item;
            try
            {
                item = _buffer.Dequeue();
                Console.WriteLine($"[Consumer {id}] Извлечен элемент: {item}. В буфере: {_buffer.Count} элементов.");
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            // Увеличиваем счетчик свободных мест
            _emptySlots.Release();
            // Имитация обработки данных
            await Task.Delay(rand.Next(300, 800));
        }
        Console.WriteLine($"[Consumer {id}] Завершен.");
    }
}

// Усложненное задание: в буфере хранятся объекты с полями
// УСЛОЖНЁННОЕ ЗАДАНИЕ: в буфер кладутся объекты DataItem (Id + Payload), логика та же
public class DataItem
{
    public int Id;
    public string Payload = "";
}

public static class ProducerConsumerItems
{
    private static Queue<DataItem> _buffer = new Queue<DataItem>();
    private const int BufferSize = 5;
    private static SemaphoreSlim _emptySlots = new SemaphoreSlim(BufferSize, BufferSize);
    private static SemaphoreSlim _filledSlots = new SemaphoreSlim(0, BufferSize);
    private static Mutex _mutex = new Mutex();
    private static bool _isRunning = true;
    private static int _nextId = 0;

    public static void Run()
    {
        Console.WriteLine("=== Producer-Consumer: буфер хранит объекты DataItem ===\n");
        Task producer1 = Task.Run(() => Producer(1));
        Task producer2 = Task.Run(() => Producer(2));
        Task consumer1 = Task.Run(() => Consumer(1));
        Task consumer2 = Task.Run(() => Consumer(2));
        Task consumer3 = Task.Run(() => Consumer(3));

        Thread.Sleep(5000);
        _isRunning = false;
        Task.WaitAll(producer1, producer2, consumer1, consumer2, consumer3);
        Console.WriteLine("\nВсе потоки завершены.");
    }

    static async Task Producer(int id)
    {
        Random rand = new Random(id);
        while (_isRunning)
        {
            if (!await _emptySlots.WaitAsync(500)) continue;
            _mutex.WaitOne();
            try
            {
                var item = new DataItem { Id = Interlocked.Increment(ref _nextId), Payload = $"данные от производителя {id}" };
                _buffer.Enqueue(item);
                Console.WriteLine($"[Producer {id}] Добавлен DataItem {{ Id = {item.Id}, Payload = \"{item.Payload}\" }}. В буфере: {_buffer.Count}.");
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            _filledSlots.Release();
            await Task.Delay(rand.Next(200, 600));
        }
        Console.WriteLine($"[Producer {id}] Завершен.");
    }

    static async Task Consumer(int id)
    {
        Random rand = new Random(id + 10);
        while (_isRunning)
        {
            if (!await _filledSlots.WaitAsync(500)) continue;
            _mutex.WaitOne();
            DataItem item;
            try
            {
                item = _buffer.Dequeue();
                Console.WriteLine($"[Consumer {id}] Извлечен DataItem {{ Id = {item.Id}, Payload = \"{item.Payload}\" }}. В буфере: {_buffer.Count}.");
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
            _emptySlots.Release();
            await Task.Delay(rand.Next(300, 800));
        }
        Console.WriteLine($"[Consumer {id}] Завершен.");
    }
}

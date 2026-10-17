using Lab5;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Аргумент выбирает задание (список режимов описан в docs/01-ЗАПУСК.md)
// ГДЕ ЧТО: ЧАСТЬ 1 - ProducerConsumer.cs (pc*), DeadlockDemo.cs (deadlock*), MutexCrossProcess.cs (mutex);
// ЧАСТЬ 2 - Part2.cs: p2t1..p2t5 (Parallel.For/ForEach, PLINQ, Partitioner, "картинка"), variant (индивидуальный вариант)
switch (args.Length > 0 ? args[0] : "")
{
    case "pc": ProducerConsumer.Run(bufferSize: 5, producers: 2, consumers: 3); break;
    case "pc2": ProducerConsumer.Run(bufferSize: 2, producers: 2, consumers: 3); break;
    case "pc10": ProducerConsumer.Run(bufferSize: 10, producers: 2, consumers: 3); break;
    case "pc-more": ProducerConsumer.Run(bufferSize: 5, producers: 3, consumers: 4); break;
    case "pc-items": ProducerConsumerItems.Run(); break;
    case "deadlock": DeadlockDemo.Run(); break;
    case "deadlock-fixed": DeadlockDemo.RunFixed(); break;
    case "deadlock-timeout": DeadlockDemo.RunTimeout(); break;
    case "mutex": MutexCrossProcess.Run(); break;
    case "p2t1": Part2.Task1(); break;
    case "p2t2": Part2.Task2(); break;
    case "p2t3": Part2.Task3(); break;
    case "p2t4": Part2.Task4(); break;
    case "p2t5": Part2.Task5(); break;
    case "variant": Variant2.Run(); break;
    default:
        Console.WriteLine("Режимы: pc, pc2, pc10, pc-more, pc-items, deadlock, deadlock-fixed, deadlock-timeout, mutex, p2t1..p2t5, variant");
        break;
}

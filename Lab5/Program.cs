using Lab5;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Часть 1: pc, pc2, pc10, pc-more, pc-items, deadlock, deadlock-fix, deadlock-timeout, philosophers, philosophers-fix, mutex
// Часть 2: p2t1..p2t5, variant 1..7
switch (args.Length > 0 ? args[0] : "")
{
    case "pc": ProducerConsumer.Run(bufferSize: 5, producers: 2, consumers: 3); break;
    case "pc2": ProducerConsumer.Run(bufferSize: 2, producers: 2, consumers: 3); break;
    case "pc10": ProducerConsumer.Run(bufferSize: 10, producers: 2, consumers: 3); break;
    case "pc-more": ProducerConsumer.Run(bufferSize: 5, producers: 3, consumers: 4); break;
    case "pc-items": ProducerConsumerItems.Run(); break;
    case "deadlock": DeadlockDemo.Run(); break;
    case "deadlock-fix": case "deadlock-fixed": DeadlockDemo.RunFixed(); break;
    case "deadlock-timeout": DeadlockDemo.RunTimeout(); break;
    case "philosophers": Philosophers.Run(fixedOrder: false); break;
    case "philosophers-fix": Philosophers.Run(fixedOrder: true); break;
    case "philosophers-waiter": PhilosophersFixes.RunWaiter(); break;
    case "philosophers-timeout": PhilosophersFixes.RunTimeout(); break;
    case "mutex": MutexCrossProcess.Run(); break;
    case "p2t1": Part2.Task1(); break;
    case "p2t2": Part2.Task2(); break;
    case "p2t3": Part2.Task3(); break;
    case "p2t4": Part2.Task4(); break;
    case "p2t5": Part2.Task5(); break;
    case "variant":
        switch (args.Length > 1 ? args[1] : "2")
        {
            case "1": Variants.V1(); break;
            case "2": Variant2.Run(); break;
            case "3": Variants.V3(); break;
            case "4": Variants.V4(); break;
            case "5": Variants.V5(); break;
            case "6": Variants.V6(); break;
            case "7": Variants.V7(); break;
            default: Console.WriteLine("Вариант 1..7"); break;
        }
        break;
    default:
        Console.WriteLine("Режимы: pc, pc2, pc10, pc-more, pc-items, deadlock, deadlock-fix, deadlock-timeout, philosophers, philosophers-fix, philosophers-waiter, philosophers-timeout, mutex, p2t1..p2t5, variant 1..7");
        break;
}

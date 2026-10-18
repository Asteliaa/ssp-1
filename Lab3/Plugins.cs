using System.Reflection;

namespace LabReflection
{
    // ============ АТРИБУТЫ ============

    [AttributeUsage(AttributeTargets.Class)]
    // Атрибуты - метаданные: [Plugin("имя")] на классе, [PluginMethod("описание")] на методе
    public class PluginAttribute : Attribute
    {
        public string Name { get; }
        public PluginAttribute(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PluginMethodAttribute : Attribute
    {
        public string Description { get; }
        public PluginMethodAttribute(string description) => Description = description;
    }

    // ============ ИНТЕРФЕЙС ============

    // Контракт плагина: загрузчик знает только этот интерфейс
    public interface IPlugin
    {
        void Execute();
    }

    // ============ ПЛАГИНЫ ============

    [Plugin("MySimplePlugin")]
    // Плагины - обычные классы с атрибутом; имя в plugins.config включает их
    public class MyPlugin : IPlugin
    {
        [PluginMethod("Основной метод")]
        public void Execute()
        {
            Console.WriteLine("Привет из плагина!");
        }

        [PluginMethod("Метод для сложения")]
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    [Plugin("Logger")]
    public class LoggerPlugin : IPlugin
    {
        [PluginMethod("Основной метод логгера")]
        public void Execute()
        {
            Console.WriteLine("[LOG] Logger plugin executed");
        }
    }

    // Новый плагин: для его подключения достаточно добавить имя в plugins.config
    [Plugin("Greeter")]
    public class GreeterPlugin : IPlugin
    {
        [PluginMethod("Основной метод приветствия")]
        public void Execute()
        {
            Console.WriteLine("Greeter plugin: добрый день!");
        }
    }

    // ============ ЗАГРУЗЧИК ПЛАГИНОВ ============

    // ТЕОРИЯ: плагины. Атрибут - метаданные ([Plugin("имя")]), читаются через GetCustomAttribute<T>().
    // Алгоритм: Assembly -> GetTypes() -> IsAssignableFrom(IPlugin) (реализует ли класс интерфейс) -> атрибут и имя из
    // plugins.config -> Activator.CreateInstance -> методы с [PluginMethod] через Invoke.
    // Новый плагин подключается добавлением класса и строки в конфиг, код загрузчика не меняется.
    public static class PluginLoader
    {
        // Загружает плагины, имена которых перечислены в конфигурационном файле
        // Загрузка: читаем имена из конфига, ищем типы в сборке, создаём и вызываем методы
        public static void Load(string configPath)
        {
            string[] names = File.ReadAllLines(configPath)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToArray();
            Console.WriteLine($"Плагины в {Path.GetFileName(configPath)}: {string.Join(", ", names)}");

            // Сборка и все её типы
            // Берём все типы текущей сборки
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                // Класс должен реализовывать интерфейс IPlugin
                if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
                    continue;

                // Атрибут PluginAttribute
                // Читаем атрибут; грузим только плагины, имя которых есть в конфиге
                PluginAttribute attribute = type.GetCustomAttribute<PluginAttribute>();
                if (attribute == null || !names.Contains(attribute.Name))
                    continue;

                // Создание экземпляра через Activator.CreateInstance
                // Создаём экземпляр плагина без new
                IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                Console.WriteLine($"\nЗагружен плагин: {attribute.Name} ({type.Name})");

                // Вызов методов плагина через рефлексию
                // Перебираем методы с [PluginMethod] и вызываем через Invoke
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    PluginMethodAttribute methodAttribute = method.GetCustomAttribute<PluginMethodAttribute>();
                    if (methodAttribute == null)
                        continue;

                    Console.WriteLine($"  Метод {method.Name}: {methodAttribute.Description}");
                    if (method.Name == "Add")
                        Console.WriteLine($"    Результат Add(5, 3) = {method.Invoke(plugin, new object[] { 5, 3 })}");
                    else
                        method.Invoke(plugin, null);
                }
            }
        }

        // Задание 7
        // ЗАДАНИЕ 7: добавляем Greeter в конфиг - код не меняется, плагин подхватывается
        public static void Task7()
        {
            Console.WriteLine("\n=== ЗАДАНИЕ 7: СИСТЕМА ПЛАГИНОВ ===\n");

            string configPath = Path.Combine(AppContext.BaseDirectory, "plugins.config");
            string original = File.ReadAllText(configPath);
            Load(configPath);

            // Добавляем новый плагин в конфигурацию: основной код не меняется
            Console.WriteLine("\n--- В plugins.config добавлена строка Greeter ---\n");
            File.AppendAllText(configPath, "Greeter" + Environment.NewLine);
            Load(configPath);

            File.WriteAllText(configPath, original);
        }
    }
}

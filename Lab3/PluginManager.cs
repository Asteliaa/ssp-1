using System.Reflection;

namespace LabReflection.Extra
{
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        void Execute();
    }

    public interface IParameterizedPlugin : IPlugin
    {
        void ExecuteWithParams(Dictionary<string, object> parameters);
    }

    public class CalculatorPlugin : IPlugin
    {
        public string Name => "Calculator";
        public string Version => "1.0.0";

        public void Execute()
        {
            Console.WriteLine("Calculator plugin loaded");
        }

        public double Add(double a, double b) => a + b;
        public double Subtract(double a, double b) => a - b;
    }

    public class LoggerPlugin : IParameterizedPlugin
    {
        public string Name => "Logger";
        public string Version => "1.0.0";

        public void Execute()
        {
            Console.WriteLine("Logger plugin executed");
        }

        public void ExecuteWithParams(Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("message", out var msg))
            {
                Console.WriteLine($"[LOG] {msg}");
            }
        }
    }

    public class PluginManager
    {
        private readonly List<IPlugin> _plugins = new List<IPlugin>();
        private readonly string _pluginsPath;

        public PluginManager(string pluginsPath = null)
        {
            _pluginsPath = pluginsPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            Directory.CreateDirectory(_pluginsPath);
        }

        // Загрузка всех плагинов из папки Plugins
        public void LoadPlugins()
        {
            var dllFiles = Directory.GetFiles(_pluginsPath, "*.dll");
            foreach (var dllPath in dllFiles)
            {
                try
                {
                    LoadPluginFromAssembly(Assembly.LoadFrom(dllPath));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading plugin {dllPath}: {ex.Message}");
                }
            }
        }

        public void LoadPluginFromAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                    _plugins.Add(plugin);
                    Console.WriteLine($"Loaded plugin: {plugin.Name} v{plugin.Version}");
                }
            }
        }

        public IEnumerable<IPlugin> GetPlugins() => _plugins;

        public IPlugin GetPlugin(string name)
        {
            return _plugins.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void ExecutePluginMethod(IPlugin plugin, string methodName, object[] parameters)
        {
            MethodInfo method = plugin.GetType().GetMethod(methodName);
            if (method != null)
            {
                var result = method.Invoke(plugin, parameters);
                Console.WriteLine($"Method {methodName} executed. Result: {result ?? "null"}");
            }
            else
            {
                Console.WriteLine($"Method {methodName} not found in plugin {plugin.Name}");
            }
        }

        public T ExecutePluginMethod<T>(IPlugin plugin, string methodName, object[] parameters)
        {
            MethodInfo method = plugin.GetType().GetMethod(methodName);
            if (method != null && method.ReturnType == typeof(T))
            {
                return (T)method.Invoke(plugin, parameters);
            }
            return default;
        }
    }
}

namespace LabReflection
{
    using LabReflection.Extra;

    public static class PluginManagerDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n=== ДОПОЛНИТЕЛЬНО: PluginManager ===\n");

            var manager = new PluginManager();
            // LoadPlugins() читает *.dll из папки Plugins; здесь плагины лежат в этой же сборке
            manager.LoadPlugins();
            manager.LoadPluginFromAssembly(Assembly.GetExecutingAssembly());

            manager.GetPlugin("Calculator").Execute();
            manager.ExecutePluginMethod(manager.GetPlugin("calculator"), "Add", new object[] { 2.5, 4.0 });
            double diff = manager.ExecutePluginMethod<double>(manager.GetPlugin("Calculator"), "Subtract", new object[] { 10.0, 3.5 });
            Console.WriteLine($"Subtract<double>(10, 3.5) = {diff}");

            var logger = (IParameterizedPlugin)manager.GetPlugin("Logger");
            logger.ExecuteWithParams(new Dictionary<string, object> { ["message"] = "Hello from PluginManager" });
            manager.ExecutePluginMethod(logger, "Missing", null);
        }
    }
}

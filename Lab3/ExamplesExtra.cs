using System.Reflection;

namespace LabReflection
{
    // Два оставшихся примера из методички: "Пример инспекции интерфейса" (атрибуты и Invoke)
    // и "Вызов методов через Invoke" (публичные, приватные, защищённые, статические члены Product)
    public static class ExamplesExtra
    {
        // Пример инспекции интерфейса: читаем атрибуты плагина и вызываем его методы через Invoke
        public static void PluginInspection()
        {
            Console.WriteLine("\n=== ПРИМЕР: ДЕМОНСТРАЦИЯ РЕФЛЕКСИИ (АТРИБУТЫ И INVOKE) ===\n");

            // 1. Создаем экземпляр плагина
            IPlugin plugin = new MyPlugin();
            Type type = plugin.GetType();

            // 2. Читаем атрибут класса
            PluginAttribute classAttr = type.GetCustomAttribute<PluginAttribute>();
            Console.WriteLine($"Имя плагина: {classAttr?.Name ?? "Не указано"}");

            // 3. Получаем все методы
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            Console.WriteLine("\nМетоды плагина:");
            foreach (MethodInfo method in methods)
            {
                // Пропускаем методы Object (ToString, GetHashCode и т.д.)
                if (method.DeclaringType == typeof(object))
                    continue;

                // Читаем атрибут метода
                PluginMethodAttribute methodAttr = method.GetCustomAttribute<PluginMethodAttribute>();
                Console.WriteLine($"\nМетод: {method.Name}");
                if (methodAttr != null)
                    Console.WriteLine($"  Описание: {methodAttr.Description}");

                // 4. Вызываем метод через Invoke
                try
                {
                    if (method.Name == "Execute")
                    {
                        // Метод без параметров
                        method.Invoke(plugin, null);
                    }
                    else if (method.Name == "Add")
                    {
                        // Метод с параметрами
                        object result = method.Invoke(plugin, new object[] { 5, 3 });
                        Console.WriteLine($"  Результат: {result}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Ошибка: {ex.InnerException?.Message ?? ex.Message}");
                }
            }

            // 5. Дополнительный пример - динамический вызов по имени
            Console.WriteLine("\n=== ДИНАМИЧЕСКИЙ ВЫЗОВ ===");
            CallMethodByName(plugin, "Add", 10, 20);
            CallMethodByName(plugin, "Execute");
        }

        // Универсальный метод для вызова любого публичного метода по имени
        static void CallMethodByName(IPlugin plugin, string methodName, params object[] parameters)
        {
            Type type = plugin.GetType();
            MethodInfo method = type.GetMethod(methodName);
            if (method != null)
            {
                Console.WriteLine($"\nВызов {methodName}()");
                object result = method.Invoke(plugin, parameters);
                if (result != null)
                    Console.WriteLine($"  Результат: {result}");
            }
            else
            {
                Console.WriteLine($"\nМетод {methodName} не найден");
            }
        }

        // Вызов методов Product через Invoke: публичные, приватный, защищённый, статический; статическое поле
        public static void InvokeMembers()
        {
            Console.WriteLine("\n=== ПРИМЕР: ВЫЗОВ ЧЛЕНОВ Product ЧЕРЕЗ INVOKE ===\n");
            Type productType = typeof(Product);

            // Создание экземпляра
            object product = Activator.CreateInstance(productType)!;

            // Свойство Id: SetValue / GetValue
            PropertyInfo idProp = productType.GetProperty("Id")!;
            idProp.SetValue(product, 123);
            Console.WriteLine($"Id через свойство: {(int)idProp.GetValue(product)!}");

            // Приватное поле _id
            FieldInfo idField = productType.GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance)!;
            idField.SetValue(product, 456);
            Console.WriteLine($"Приватное поле _id: {(int)idField.GetValue(product)!}");

            // Вызов метода без параметров
            MethodInfo displayMethod = productType.GetMethod("DisplayInfo")!;
            displayMethod.Invoke(product, null);

            // Вызов метода с параметрами (цена задаётся через защищённое поле _price)
            productType.GetField("_price", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(product, 200.0);
            MethodInfo discountMethod = productType.GetMethod("ApplyDiscount")!;
            double discounted = (double)discountMethod.Invoke(product, new object[] { 15.5 })!;
            Console.WriteLine($"ApplyDiscount(15.5) при цене 200: {discounted}");

            // Вызов приватного метода
            MethodInfo validateMethod = productType.GetMethod("ValidatePrice", BindingFlags.NonPublic | BindingFlags.Instance)!;
            validateMethod.Invoke(product, null);
            Console.WriteLine("Приватный ValidatePrice вызван");

            // Вызов защищённого метода
            MethodInfo updateMethod = productType.GetMethod("UpdateCategory", BindingFlags.NonPublic | BindingFlags.Instance)!;
            updateMethod.Invoke(product, new object[] { "NewCategory" });
            Console.WriteLine($"Category после UpdateCategory: {productType.GetProperty("Category")!.GetValue(product)}");

            // Вызов статического метода и чтение статического поля (экземпляр не нужен: null)
            MethodInfo staticMethod = productType.GetMethod("GetTotalCount")!;
            int count = (int)staticMethod.Invoke(null, null)!;
            FieldInfo totalField = productType.GetField("TotalProducts")!;
            Console.WriteLine($"GetTotalCount() = {count}, TotalProducts = {(int)totalField.GetValue(null)!}");
        }
    }
}

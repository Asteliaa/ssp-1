using System.Collections.Concurrent;
using System.Diagnostics;

namespace Lab5;

// Индивидуальные варианты (кроме 2, он в Part2.cs): последовательно vs параллельно
public static class Variants
{
    static void Time(string name, Func<object> action)
    {
        var sw = Stopwatch.StartNew();
        var result = action();
        sw.Stop();
        Console.WriteLine($"{name,-28} {sw.ElapsedMilliseconds,6} мс  результат: {result}");
    }

    // 1. Большие массивы: сумма и максимум
    public static void V1()
    {
        Console.WriteLine("=== ВАРИАНТ 1: БОЛЬШИЕ МАССИВЫ (сумма, максимум) ===\n");
        const int N = 20_000_000;
        var rnd = new Random(1);
        double[] a = new double[N];
        for (int i = 0; i < N; i++) a[i] = rnd.NextDouble() * 100;
        Func<double, double> heavy = x => Math.Sqrt(Math.Log(x + 1)) * Math.Sin(x);

        Time("Сумма последовательно", () => { double s = 0; for (int i = 0; i < N; i++) s += heavy(a[i]); return s.ToString("F2"); });
        Time("Сумма Parallel.For local", () =>
        {
            double s = 0; object l = new();
            Parallel.For(0, N, () => 0.0, (i, st, loc) => loc + heavy(a[i]), loc => { lock (l) s += loc; });
            return s.ToString("F2");
        });
        Time("Максимум последовательно", () => a.Max(heavy).ToString("F4"));
        Time("Максимум PLINQ", () => a.AsParallel().Max(heavy).ToString("F4"));
    }

    // 3. Матрицы: умножение и транспонирование
    public static void V3()
    {
        Console.WriteLine("=== ВАРИАНТ 3: МАТРИЦЫ (умножение, транспонирование) ===\n");
        const int N = 700;
        var rnd = new Random(3);
        double[,] A = new double[N, N], B = new double[N, N];
        for (int i = 0; i < N; i++) for (int j = 0; j < N; j++) { A[i, j] = rnd.NextDouble(); B[i, j] = rnd.NextDouble(); }

        double[,] Mul(bool par)
        {
            var C = new double[N, N];
            void Row(int i) { for (int j = 0; j < N; j++) { double s = 0; for (int k = 0; k < N; k++) s += A[i, k] * B[k, j]; C[i, j] = s; } }
            if (par) Parallel.For(0, N, Row); else for (int i = 0; i < N; i++) Row(i);
            return C;
        }
        double[,] Transpose(bool par)
        {
            var T = new double[N, N];
            void Row(int i) { for (int j = 0; j < N; j++) T[j, i] = A[i, j]; }
            if (par) Parallel.For(0, N, Row); else for (int i = 0; i < N; i++) Row(i);
            return T;
        }
        Time("Умножение последовательно", () => Mul(false)[10, 10].ToString("F3"));
        Time("Умножение Parallel.For", () => Mul(true)[10, 10].ToString("F3"));
        Time("Транспонирование послед.", () => Transpose(false)[5, 7].ToString("F3"));
        Time("Транспонирование Parallel", () => Transpose(true)[5, 7].ToString("F3"));
    }

    // 4. Изображения: фильтр (размытие 3x3) и преобразование (негатив)
    public static void V4()
    {
        Console.WriteLine("=== ВАРИАНТ 4: ИЗОБРАЖЕНИЯ (размытие, негатив) ===\n");
        const int W = 3000, H = 3000;
        var rnd = new Random(4);
        byte[,] img = new byte[H, W];
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) img[y, x] = (byte)rnd.Next(256);

        byte[,] Blur(bool par)
        {
            var r = new byte[H, W];
            void Row(int y)
            {
                if (y == 0 || y == H - 1) return;
                for (int x = 1; x < W - 1; x++)
                {
                    int s = 0;
                    for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) s += img[y + dy, x + dx];
                    r[y, x] = (byte)(s / 9);
                }
            }
            if (par) Parallel.For(0, H, Row); else for (int y = 0; y < H; y++) Row(y);
            return r;
        }
        byte[,] Negative(bool par)
        {
            var r = new byte[H, W];
            void Row(int y) { for (int x = 0; x < W; x++) r[y, x] = (byte)(255 - img[y, x]); }
            if (par) Parallel.For(0, H, Row); else for (int y = 0; y < H; y++) Row(y);
            return r;
        }
        Time("Размытие последовательно", () => Blur(false)[100, 100]);
        Time("Размытие Parallel.For", () => Blur(true)[100, 100]);
        Time("Негатив последовательно", () => Negative(false)[100, 100]);
        Time("Негатив Parallel.For", () => Negative(true)[100, 100]);
    }

    // 5. Текст: подсчёт слов и поиск
    public static void V5()
    {
        Console.WriteLine("=== ВАРИАНТ 5: ТЕКСТОВЫЕ ДАННЫЕ (подсчёт, поиск) ===\n");
        string[] vocab = { "поток", "задача", "семафор", "мьютекс", "память", "куча", "стек", "рефлексия", "сборщик", "блокировка" };
        var rnd = new Random(5);
        string[] lines = Enumerable.Range(0, 400_000)
            .Select(_ => string.Join(' ', Enumerable.Range(0, 20).Select(_ => vocab[rnd.Next(vocab.Length)] + rnd.Next(50))))
            .ToArray();
        Console.WriteLine($"Строк: {lines.Length}\n");

        Time("Частоты последовательно", () =>
        {
            var d = new Dictionary<string, int>();
            foreach (var l in lines) foreach (var w in l.Split(' ')) d[w] = d.GetValueOrDefault(w) + 1;
            return d.OrderByDescending(p => p.Value).First();
        });
        Time("Частоты Parallel.ForEach", () =>
        {
            var d = new ConcurrentDictionary<string, int>();
            Parallel.ForEach(lines, l => { foreach (var w in l.Split(' ')) d.AddOrUpdate(w, 1, (_, c) => c + 1); });
            return d.OrderByDescending(p => p.Value).First();
        });
        Time("Поиск слова LINQ", () => lines.Count(l => l.Contains("семафор7")));
        Time("Поиск слова PLINQ", () => lines.AsParallel().Count(l => l.Contains("семафор7")));
    }

    // 6. Численные методы: интегрирование (метод прямоугольников)
    public static void V6()
    {
        Console.WriteLine("=== ВАРИАНТ 6: ЧИСЛЕННЫЕ МЕТОДЫ (интеграл sin(x)*sqrt(x) на [0;10]) ===\n");
        const long N = 200_000_000;
        const double a = 0, b = 10;
        double h = (b - a) / N;
        Func<double, double> f = x => Math.Sin(x) * Math.Sqrt(x);

        Time("Последовательно", () => { double s = 0; for (long i = 0; i < N; i++) s += f(a + (i + 0.5) * h); return (s * h).ToString("F6"); });
        Time("Partitioner + local", () =>
        {
            double s = 0; object l = new();
            Parallel.ForEach(Partitioner.Create(0L, N, 1_000_000L), () => 0.0, (r, st, loc) =>
            {
                for (long i = r.Item1; i < r.Item2; i++) loc += f(a + (i + 0.5) * h);
                return loc;
            }, loc => { lock (l) s += loc; });
            return (s * h).ToString("F6");
        });
    }

    // 7. Сортировка и поиск
    public static void V7()
    {
        Console.WriteLine("=== ВАРИАНТ 7: СОРТИРОВКА И ПОИСК ===\n");
        const int N = 5_000_000;
        var rnd = new Random(7);
        int[] src = new int[N];
        for (int i = 0; i < N; i++) src[i] = rnd.Next();

        Time("Сортировка Array.Sort", () => { var c = (int[])src.Clone(); Array.Sort(c); return c[0]; });
        Time("Параллельная сортировка", () => { var c = (int[])src.Clone(); ParallelMergeSort(c); return c[0]; });

        int target = src[N - 1000];
        Time("Поиск индекса послед.", () => Array.IndexOf(src, target));
        Time("Поиск индекса Parallel", () =>
        {
            int pos = int.MaxValue;
            Parallel.For(0, N, (i, st) =>
            {
                if (src[i] != target) return;
                int cur;
                while (i < (cur = pos) && Interlocked.CompareExchange(ref pos, i, cur) != cur) { }
                st.Break();
            });
            return pos;
        });
    }

    // Сортировка слиянием: половины сортируются параллельно до глубины 3
    static void ParallelMergeSort(int[] a) => Sort(a, new int[a.Length], 0, a.Length, 3);

    static void Sort(int[] a, int[] tmp, int lo, int hi, int depth)
    {
        if (hi - lo < 2) return;
        if (depth == 0) { Array.Sort(a, lo, hi - lo); return; }
        int mid = (lo + hi) / 2;
        Parallel.Invoke(() => Sort(a, tmp, lo, mid, depth - 1), () => Sort(a, tmp, mid, hi, depth - 1));
        int i = lo, j = mid, k = lo;
        while (i < mid && j < hi) tmp[k++] = a[i] <= a[j] ? a[i++] : a[j++];
        while (i < mid) tmp[k++] = a[i++];
        while (j < hi) tmp[k++] = a[j++];
        Array.Copy(tmp, lo, a, lo, hi - lo);
    }
}

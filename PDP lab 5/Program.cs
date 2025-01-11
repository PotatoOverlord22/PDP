using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

class PolynomialMultiplication
{
    static void Main(string[] args)
    {
        int[] poly1 = new int[100]; // 6x^3 + 10x^2 + 5
        int[] poly2 = new int[100];    // 4x^2 + 2x + 1

        for (int i = 0; i < poly1.Length; i++)
        {
            poly1[i] = 1;
            poly2[i] = 1;
        }

        CompareAlgorithms(poly1, poly2);
    }

    static void CompareAlgorithms(int[] poly1, int[] poly2)
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        var resultSeqRegular = RegularMultiplication(poly1, poly2);
        stopwatch.Stop();
        Console.WriteLine(PolyToString(resultSeqRegular));
        Console.WriteLine($"Sequential Regular: {stopwatch.ElapsedMilliseconds} ms\n");

        stopwatch.Restart();
        var resultParRegular = ParallelRegularMultiplication(poly1, poly2);
        stopwatch.Stop();
        Console.WriteLine(PolyToString(resultParRegular));
        Console.WriteLine($"Parallel Regular: {stopwatch.ElapsedMilliseconds} ms\n");

        stopwatch.Restart();
        var resultSeqKaratsuba = Karatsuba(poly1, poly2);
        stopwatch.Stop();
        Console.WriteLine(PolyToString(resultSeqKaratsuba));
        Console.WriteLine($"Sequential Karatsuba: {stopwatch.ElapsedMilliseconds} ms\n");

        stopwatch.Restart();
        var resultParKaratsuba = ParallelKaratsuba(poly1, poly2);
        stopwatch.Stop();
        Console.WriteLine(PolyToString(resultParKaratsuba));
        Console.WriteLine($"Parallel Karatsuba: {stopwatch.ElapsedMilliseconds} ms\n");
    }

    private static string PolyToString(int[] polynom)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(polynom[0]);
        sb.Append(" + ");

        for (int i = 1; i < polynom.Length - 1; ++i)
        {
            sb.Append($"{polynom[i]}x^{i} + ");
        }

        if (polynom.Length > 1)
        {
            sb.Append($"{polynom[polynom.Length - 1]}x^{polynom.Length - 1}");
        }

        return sb.ToString();
    }

    static int[] RegularMultiplication(int[] poly1, int[] poly2)
    {
        int n1 = poly1.Length;
        int n2 = poly2.Length;
        int[] result = new int[n1 + n2 - 1];

        for (int i = 0; i < n1; i++)
        {
            for (int j = 0; j < n2; j++)
            {
                result[i + j] += poly1[i] * poly2[j];
            }
        }

        return result;
    }

    static int[] ParallelRegularMultiplication(int[] poly1, int[] poly2)
    {
        int n1 = poly1.Length;
        int n2 = poly2.Length;
        int[] result = new int[n1 + n2 - 1];

        Parallel.For(0, n1, i =>
        {
            for (int j = 0; j < n2; j++)
            {
                int product = poly1[i] * poly2[j];

                lock (result)
                {
                    result[i + j] += product;
                }
            }
        });

        return result;
    }

    static int[] Karatsuba(int[] poly1, int[] poly2)
    {
        int n = Math.Max(poly1.Length, poly2.Length);
        if (n <= 2)
        {
            return RegularMultiplication(poly1, poly2);
        }

        n = (n % 2 == 0) ? n : n + 1;

        int[] padded1 = PadPolynomial(poly1, n);
        int[] padded2 = PadPolynomial(poly2, n);

        int mid = n / 2;
        int[] low1 = SubArray(padded1, 0, mid);
        int[] high1 = SubArray(padded1, mid, n);
        int[] low2 = SubArray(padded2, 0, mid);
        int[] high2 = SubArray(padded2, mid, n);

        int[] z0 = Karatsuba(low1, low2);
        int[] z1 = Karatsuba(AddPolynomials(low1, high1), AddPolynomials(low2, high2));
        int[] z2 = Karatsuba(high1, high2);

        int[] result = new int[2 * n - 1];
        AddToResult(result, z0, 0);
        AddToResult(result, SubtractPolynomials(z1, AddPolynomials(z0, z2)), mid);
        AddToResult(result, z2, 2 * mid);

        return TrimPolynomial(result);
    }

    static int[] ParallelKaratsuba(int[] poly1, int[] poly2)
    {
        int n = Math.Max(poly1.Length, poly2.Length);
        if (n <= 2)
        {
            return RegularMultiplication(poly1, poly2);
        }

        n = (n % 2 == 0) ? n : n + 1;

        int[] padded1 = PadPolynomial(poly1, n);
        int[] padded2 = PadPolynomial(poly2, n);

        int mid = n / 2;
        int[] low1 = SubArray(padded1, 0, mid);
        int[] high1 = SubArray(padded1, mid, n);
        int[] low2 = SubArray(padded2, 0, mid);
        int[] high2 = SubArray(padded2, mid, n);

        int[] z0 = null, z1 = null, z2 = null;

        Parallel.Invoke(
            () => z0 = Karatsuba(low1, low2),
            () => z1 = Karatsuba(AddPolynomials(low1, high1), AddPolynomials(low2, high2)),
            () => z2 = Karatsuba(high1, high2)
        );

        int[] result = new int[2 * n - 1];
        AddToResult(result, z0, 0);
        AddToResult(result, SubtractPolynomials(z1, AddPolynomials(z0, z2)), mid);
        AddToResult(result, z2, 2 * mid);

        return TrimPolynomial(result);
    }

    static int[] AddPolynomials(int[] poly1, int[] poly2)
    {
        int n = Math.Max(poly1.Length, poly2.Length);
        int[] result = new int[n];

        for (int i = 0; i < n; i++)
        {
            int val1 = i < poly1.Length ? poly1[i] : 0;
            int val2 = i < poly2.Length ? poly2[i] : 0;
            result[i] = val1 + val2;
        }

        return result;
    }

    static int[] SubtractPolynomials(int[] poly1, int[] poly2)
    {
        int n = Math.Max(poly1.Length, poly2.Length);
        int[] result = new int[n];

        for (int i = 0; i < n; i++)
        {
            int val1 = i < poly1.Length ? poly1[i] : 0;
            int val2 = i < poly2.Length ? poly2[i] : 0;
            result[i] = val1 - val2;
        }

        return result;
    }

    static int[] PadPolynomial(int[] poly, int size)
    {
        int[] result = new int[size];
        Array.Copy(poly, result, poly.Length);
        return result;
    }

    static int[] SubArray(int[] array, int start, int end)
    {
        int[] result = new int[end - start];
        Array.Copy(array, start, result, 0, end - start);
        return result;
    }

    static void AddToResult(int[] result, int[] source, int offset)
    {
        for (int i = 0; i < source.Length; i++)
        {
            result[i + offset] += source[i];
        }
    }

    static int[] TrimPolynomial(int[] poly)
    {
        int lastNonZero = poly.Length - 1;
        while (lastNonZero >= 0 && poly[lastNonZero] == 0)
        {
            lastNonZero--;
        }

        int[] trimmed = new int[lastNonZero + 1];
        Array.Copy(poly, trimmed, lastNonZero + 1);
        return trimmed;
    }
}
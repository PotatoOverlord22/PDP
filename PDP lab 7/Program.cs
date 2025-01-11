using MPI;
using System.Diagnostics;
using System.Text;

class PolynomialMultiplication
{
    static void Main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            Intracommunicator comm = Communicator.world;
            int rank = comm.Rank;
            int size = comm.Size;

            if (size < 3)
            {
                if (rank == 0)
                {
                    Console.WriteLine("You need at least 3 nodes to run this program.");
                }

                return;
            }

            if (rank == 0)
            {
                int[] poly1 = { 5, 0, 10, 6 }; // 6x^3 + 10x^2 + 5
                int[] poly2 = { 1, 2, 4 };     // 4x^2 + 2x + 1

                Console.WriteLine($"Polynomial 1: {PolyToString(poly1)}");
                Console.WriteLine($"Polynomial 2: {PolyToString(poly2)}");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                int[] result = ParallelKaratsuba(poly1, poly2, comm);

                stopwatch.Stop();

                if (result != null)
                {
                    Console.WriteLine($"Result: {PolyToString(result)}");
                }
            }
            else
            {
                WorkerProcess(comm);
            }
        }
    }

    static void WorkerProcess(Intracommunicator comm)
    {
        while (true)
        {
            comm.Receive<int>(0, 0, out int taskType);

            if (taskType == -1)
                break;

            comm.Receive(0, 1, out int[] poly1);
            comm.Receive(0, 2, out int[] poly2);

            int[] result = Karatsuba(poly1, poly2);

            comm.Send<int[]>(result, 0, 3);
        }
    }

    static int[] ParallelKaratsuba(int[] poly1, int[] poly2, Intracommunicator comm)
    {
        int rank = comm.Rank;
        int size = comm.Size;
        int n = Math.Max(poly1.Length, poly2.Length);

        if (n <= 2)
        {
            return rank == 0 ? RegularMultiplication(poly1, poly2) : null;
        }

        n = (n % 2 == 0) ? n : n + 1;
        int[] padded1 = PadPolynomial(poly1, n);
        int[] padded2 = PadPolynomial(poly2, n);

        int mid = n / 2;
        int[] low1 = SubArray(padded1, 0, mid);
        int[] high1 = SubArray(padded1, mid, n);
        int[] low2 = SubArray(padded2, 0, mid);
        int[] high2 = SubArray(padded2, mid, n);

        if (rank == 0)
        {
            int[] z0 = Karatsuba(low1, low2);

            comm.Send<int>(1, 1, 0);
            comm.Send<int[]>(AddPolynomials(low1, high1), 1, 1);
            comm.Send<int[]>(AddPolynomials(low2, high2), 1, 2);

            comm.Send<int>(1, 2, 0);
            comm.Send<int[]>(high1, 2, 1);
            comm.Send<int[]>(high2, 2, 2);

            comm.Receive(1, 3, out int[] z1);
            comm.Receive(2, 3, out int[] z2);

            int[] result = new int[2 * n - 1];
            AddToResult(result, z0, 0);
            AddToResult(result, SubtractPolynomials(z1, AddPolynomials(z0, z2)), mid);
            AddToResult(result, z2, 2 * mid);

            for (int i = 1; i < size; i++)
            {
                comm.Send<int>(-1, i, 0);
            }

            return TrimPolynomial(result);
        }

        return null;
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
        TrimPolynomial(source);
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
}
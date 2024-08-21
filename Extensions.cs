using System.Numerics;

namespace Monitoring;

public static class Extensions
{
    public static T Average<T>(this Span<T> values) where T : INumber<T>
    {
        T sum = T.Zero;
        int count = 0;

        foreach (var value in values)
        {
            sum += value;
            count++;
        }

        T average = sum / T.CreateChecked(count);

        return average;
    }

    public static T Average<T>(this List<IOperation<T>> values) where T : INumber<T>
    {
        T sum = T.Zero;
        int count = 0;

        foreach (var value in values)
        {
            sum += value.Value;
            count++;
        }

        T average = sum / T.CreateChecked(count);

        return average;
    }

    public static T Average<T>(this List<T> values) where T : INumber<T>
    {
        T sum = T.Zero;
        T count = T.Zero;

        foreach (var value in values)
        {
            sum += value;
            count++;
        }

        T average = sum / count;

        return average;
    }
}
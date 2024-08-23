using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;

namespace Monitoring;

public static class Extensions
{
    public static T Average<T>(this List<T> values) where T : INumber<T>
    {
        Span<T> span = CollectionsMarshal.AsSpan(values);
        return span.Average();
    }

    public static T Average<T>(this Span<T> values) where T : INumber<T>
    {
        T sum = TensorPrimitives.Sum<T>(values);
        T average = sum / T.CreateTruncating(values.Length);
        return average;
    }
}
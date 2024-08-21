// using System.Numerics;

// namespace Monitoring.Operations;

// public class MaxOperation<T> : IOperation<T> where T : INumber<T>, IMinMaxValue<T>
// {
//     T _value = T.MinValue;
//     bool _valueChanged = false;

//     public string Name => "Max value";

//     public T Value => _value;

//     public void Load(T value)
//     {
//         if (value > _value)
//         {
//             _valueChanged = true;
//             _value = value;
//             return;
//         }

//         if (_valueChanged is true)
//         {
//             _valueChanged = false;
//         }
//     }

//     public bool ValueChanged => _valueChanged;
// }

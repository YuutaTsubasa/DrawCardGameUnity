using System;
using System.Collections;
using System.Collections.Generic;

namespace Yuuta.Core
{
    public struct Option<T> : IEnumerable<T>, IEquatable<Option<T>>
    {
        private bool _hasValue;
        private T _value;

        public Option(T value)
        {
            _hasValue = true;
            _value = value;
        }

        public static Option<T> Some(T value)
            => new(value);

        public static Option<T> None()
            => new();

        public T ValueOr(T defaultValue)
            => _hasValue ? _value : defaultValue;

        public T ValueOrFailure()
            => _hasValue ? _value : throw new Exception("Option doesn't have value.");

        public TResult Match<TResult>(Func<T, TResult> someFunc, Func<TResult> noneFunc)
            => _hasValue ? someFunc(_value) : noneFunc();
        
        public void Match(Action<T> someFunc, Action noneFunc)
        {
            if (_hasValue) someFunc(_value);
            else noneFunc();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (!_hasValue)
                yield break;
            
            yield return _value;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => (this as IEnumerable<T>).GetEnumerator();

        public override int GetHashCode()
        {
            return HashCode.Combine(_hasValue, _value);
        }

        public bool Equals(Option<T> other)
        {
            return _hasValue == other._hasValue && EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is Option<T> other && Equals(other);
        }
    }

    public static class OptionExtension
    {
        public static Option<T> Some<T>(this T value)
            => Option<T>.Some(value);
    }
}

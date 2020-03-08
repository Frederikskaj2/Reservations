using System;

namespace Frederikskaj2.Reservations.Client
{
    public struct Maybe<T>
    {
        private readonly T value;

        private readonly bool hasValue;

        private Maybe(T value)
        {
            this.value = value;
            hasValue = true;
        }

        public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) => hasValue ? some(value) : none();

        public void Match(Action<T> some, Action none)
        {
            if (hasValue)
                some(value);
            else
                none();
        }

        public static implicit operator Maybe<T>(T value) => value is null ? new Maybe<T>() : new Maybe<T>(value);

        public static implicit operator Maybe<T>(Maybe.MaybeNone _) => new Maybe<T>();

        public bool TryGetValue(out T value)
        {
            if (hasValue)
            {
                value = this.value;
                return true;
            }

#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
            value = default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
            return false;
        }

        public Maybe<TResult> Map<TResult>(Func<T, TResult> convert)
            => !hasValue ? new Maybe<TResult>() : convert(value);

        public Maybe<TResult> Select<TResult>(Func<T, TResult> convert)
            => !hasValue ? new Maybe<TResult>() : convert(value);

        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> convert)
            => !hasValue ? new Maybe<TResult>() : convert(value);

        public Maybe<TResult> SelectMany<T2, TResult>(Func<T, Maybe<T2>> convert, Func<T, T2, TResult> finalSelect)
        {
            if (!hasValue)
                return new Maybe<TResult>();

            var converted = convert(value);
            return !converted.hasValue ? new Maybe<TResult>() : finalSelect(value, converted.value);
        }

        public Maybe<T> Where(Func<T, bool> predicate)
        {
            if (!hasValue)
                return new Maybe<T>();

            return predicate(value) ? this : new Maybe<T>();
        }

        public T ValueOr(T defaultValue) => hasValue ? value : defaultValue;

        public T ValueOr(Func<T> defaultValueFactory) => hasValue ? value : defaultValueFactory();

        public Maybe<T> ValueOrMaybe(Maybe<T> alternativeValue) => hasValue ? this : alternativeValue;

        public Maybe<T> ValueOrMaybe(Func<Maybe<T>> alternativeValueFactory)
            => hasValue ? this : alternativeValueFactory();

        public T ValueOrThrow(string errorMessage)
        {
            if (hasValue)
                return value;

            throw new Exception(errorMessage);
        }
    }

    public static class Maybe
    {
        public static MaybeNone None { get; } = new MaybeNone();

        public static Maybe<T> Some<T>(T value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return value;
        }

        public class MaybeNone
        {
        }
    }
}
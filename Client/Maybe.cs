using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Client
{
    [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "This type already provides alternatives to the implicit cast operators.")]
    public struct Maybe<T> : IEquatable<Maybe<T>>
    {
        private readonly T value;

        private readonly bool hasValue;

        private Maybe(T value)
        {
            this.value = value;
            hasValue = true;
        }

        public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
        {
            if (some is null)
                throw new ArgumentNullException(nameof(some));
            if (none is null)
                throw new ArgumentNullException(nameof(none));

            return hasValue ? some(value) : none();
        }

        public void Match(Action<T> some, Action none)
        {
            if (some is null)
                throw new ArgumentNullException(nameof(some));
            if (none is null)
                throw new ArgumentNullException(nameof(none));

            if (hasValue)
                some(value);
            else
                none();
        }

        public static implicit operator Maybe<T>(T value) => value is null ? new Maybe<T>() : new Maybe<T>(value);

        public static implicit operator Maybe<T>(MaybeNone _) => new Maybe<T>();

        public bool TryGetValue(out T value)
        {
            if (hasValue)
            {
                value = this.value;
                return true;
            }

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8653 // A default expression introduces a null value when 'T' is a non-nullable reference type.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8653 // A default expression introduces a null value when 'T' is a non-nullable reference type.
            return false;
        }

        public Maybe<TResult> Map<TResult>(Func<T, TResult> convert)
        {
            if (convert is null)
                throw new ArgumentNullException(nameof(convert));

            return !hasValue ? new Maybe<TResult>() : convert(value);
        }

        public Maybe<TResult> Select<TResult>(Func<T, TResult> convert)
        {
            if (convert is null)
                throw new ArgumentNullException(nameof(convert));

            return !hasValue ? new Maybe<TResult>() : convert(value);
        }

        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> convert)
        {
            if (convert is null)
                throw new ArgumentNullException(nameof(convert));

            return !hasValue ? new Maybe<TResult>() : convert(value);
        }

        public Maybe<TResult> SelectMany<T2, TResult>(Func<T, Maybe<T2>> convert, Func<T, T2, TResult> finalSelect)
        {
            if (convert is null)
                throw new ArgumentNullException(nameof(convert));
            if (finalSelect is null)
                throw new ArgumentNullException(nameof(finalSelect));

            if (!hasValue)
                return new Maybe<TResult>();

            var converted = convert(value);
            return !converted.hasValue ? new Maybe<TResult>() : finalSelect(value, converted.value);
        }

        public Maybe<T> Where(Func<T, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            if (!hasValue)
                return new Maybe<T>();

            return predicate(value) ? this : new Maybe<T>();
        }

        public T ValueOr(T defaultValue) => hasValue ? value : defaultValue;

        public T ValueOr(Func<T> defaultValueFactory)
        {
            if (defaultValueFactory is null)
                throw new ArgumentNullException(nameof(defaultValueFactory));

            return hasValue ? value : defaultValueFactory();
        }

        public Maybe<T> ValueOrMaybe(Maybe<T> alternativeValue) => hasValue ? this : alternativeValue;

        public Maybe<T> ValueOrMaybe(Func<Maybe<T>> alternativeValueFactory)
        {
            if (alternativeValueFactory is null)
                throw new ArgumentNullException(nameof(alternativeValueFactory));

            return hasValue ? this : alternativeValueFactory();
        }

        public T ValueOrThrow(string errorMessage)
        {
            if (hasValue)
                return value;

            throw new Exception(errorMessage);
        }

        public bool Equals(Maybe<T> other) => hasValue == other.hasValue && EqualityComparer<T>.Default.Equals(value, other.value);

        public override bool Equals(object? obj) => obj is Maybe<T> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(hasValue, value);

        public static bool operator ==(Maybe<T> maybe1, Maybe<T> maybe2) => maybe1.Equals(maybe2);

        public static bool operator !=(Maybe<T> maybe1, Maybe<T> maybe2) => !(maybe1 == maybe2);
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
    }
}
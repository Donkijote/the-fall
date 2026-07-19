using System;

namespace TheFall.Domain
{
    public readonly struct Score : IEquatable<Score>
    {
        public Score(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "A score cannot be negative.");
            }

            Value = value;
        }

        public int Value { get; }

        public Score Add(int points)
        {
            if (points < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(points));
            }

            return new Score(checked(Value + points));
        }

        public Score SubtractClamped(int points)
        {
            if (points < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(points));
            }

            return new Score(Math.Max(0, Value - points));
        }

        public bool Equals(Score other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Score other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}

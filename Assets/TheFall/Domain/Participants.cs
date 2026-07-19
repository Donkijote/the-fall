using System;
using System.Collections.Generic;

namespace TheFall.Domain
{
    public enum Seat
    {
        First,
        Second,
        Third,
        Fourth,
    }

    public enum TeamId
    {
        One,
        Two,
        Three,
        Four,
    }

    public enum PlayerControl
    {
        Human,
        Bot,
    }

    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        public PlayerId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A player identifier is required.", nameof(value));
            }

            Value = value;
        }

        public string Value { get; }

        public bool Equals(PlayerId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(PlayerId left, PlayerId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerId left, PlayerId right)
        {
            return !left.Equals(right);
        }
    }

    public sealed class Player
    {
        public Player(PlayerId id, string displayName, Seat seat, TeamId teamId, PlayerControl control)
        {
            if (string.IsNullOrWhiteSpace(id.Value))
            {
                throw new ArgumentException("A player identifier is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("A display name is required.", nameof(displayName));
            }

            Id = id;
            DisplayName = displayName;
            Seat = seat;
            TeamId = teamId;
            Control = control;
        }

        public PlayerId Id { get; }

        public string DisplayName { get; }

        public Seat Seat { get; }

        public TeamId TeamId { get; }

        public PlayerControl Control { get; }
    }

    public sealed class Team
    {
        private readonly IReadOnlyList<PlayerId> _members;

        public Team(TeamId id, params PlayerId[] members)
        {
            if (members == null || members.Length == 0)
            {
                throw new ArgumentException("A team needs at least one player.", nameof(members));
            }

            Id = id;
            _members = Array.AsReadOnly((PlayerId[])members.Clone());
        }

        public TeamId Id { get; }

        public IReadOnlyList<PlayerId> Members => _members;
    }
}

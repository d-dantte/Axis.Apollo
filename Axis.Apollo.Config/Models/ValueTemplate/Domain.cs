using Axis.Luna.Extensions;
using System;
using System.Text.RegularExpressions;

namespace Axis.Apollo.Config.Models.ValueTemplate
{
    public struct Domain
    {
        private static readonly string DefaultDomain = "*";
        private static readonly Regex DomainPattern = new Regex("^[0-9a-zA-Z_-]+(\\.[0-9a-zA-Z_-]+)*$");

        public string Name { get; }

        public string Value { get; }

        public bool IsDefault => Name == null;

        public Domain(string name, string value)
        {
            Name = DomainPattern.IsMatch(name ?? "")
                ? name
                : throw new ArgumentException($"Invalid format for {nameof(name)}: {name}");

            Value = DomainPattern.IsMatch(value ?? "")
                ? value
                : throw new ArgumentException($"Invalid format for {nameof(value)}: {value}");
        }

        public override string ToString() => string.IsNullOrEmpty(Name) ? $"{DefaultDomain}" : $"{Name}:{Value}";

        public override bool Equals(object obj)
        {
            return obj is Domain other
                && other.Name.Equals(Name)
                && other.Value.Equals(Value);
        }

        public override int GetHashCode() => HashCode.Combine(Name, Value);

        public static Domain Parse(string value)
        {
            if (TryParse(value, out var domain))
                return domain;

            else throw new FormatException($"Invalid input format: {value}");
        }

        public static bool TryParse(string value, out Domain domain)
        {
            domain = default;
            try
            {
                domain = DefaultDomain.Equals(value)
                    ? default
                    : value
                        .Split(':')
                        .ApplyTo(parts => new Domain(parts[0], parts[1]));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool operator ==(Domain first, Domain second) => first.Equals(second);

        public static bool operator !=(Domain first, Domain second) => !(first == second);

        public static implicit operator Domain(string value) => Parse(value);
    }
}

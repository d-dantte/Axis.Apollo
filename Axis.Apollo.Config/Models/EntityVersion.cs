using Axis.Luna.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Axis.Apollo.Config.Models
{
    public interface IVersion : IComparable<IVersion>
    {
        string Version { get; }
    }

    public struct SemVer: IVersion, IComparable<SemVer>
    {
        private static readonly Regex PreReleasePattern = new Regex("^[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*$");

        internal static readonly Regex SemVerPattern = new Regex("^(\\d+)(\\.\\d+)(\\.\\d+)(-[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*)?$");

        /// <summary>
        /// Major number
        /// </summary>
        public int Major { get; }

        /// <summary>
        /// Minor number
        /// </summary>
        public int Minor { get; }

        /// <summary>
        /// Patch number
        /// </summary>
        public int Patch { get; }

        /// <summary>
        /// Represents pre-release information about the version
        /// </summary>
        public string Metadata { get; }

        public string Version
        {
            get
            {
                var versionString = new StringBuilder()
                    .Append(Major)
                    .Append('.').Append(Minor)
                    .Append('.').Append(Patch);


                if (Metadata != null)
                    versionString.Append('-').Append(Metadata);

                return versionString.ToString();
            }
        }

        public SemVer(int major, int minor = 0, int patch = 0, string metadata = null)
        {
            Major = major.ThrowIf(m => m < 0, new ArgumentException($"Invalid argument: {nameof(major)}"));
            Minor = minor.ThrowIf(m => m < 0, new ArgumentException($"Invalid argument: {nameof(minor)}"));
            Patch = patch.ThrowIf(m => m < 0, new ArgumentException($"Invalid argument: {nameof(patch)}"));
            Metadata = metadata.ThrowIf(
                m => string.Empty.Equals(m?.Trim()) || !PreReleasePattern.IsMatch(m),
                new ArgumentException(null, nameof(major)));
        }

        public override bool Equals(object obj)
        {
            return obj is SemVer other
                && other.Major == Major
                && other.Minor == Minor
                && other.Patch == Patch
                && other.Metadata?.Equals(Metadata) == true;
        }

        public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, Metadata);

        public override string ToString() => Version;

        public static SemVer Parse(string value)
        {
            if (TryParse(value, out var version))
                return version;

            else throw new FormatException($"Invalid format: {value}");
        }

        public static bool TryParse(string value, out SemVer version)
        {
            version = default;

            try
            {
                if (!SemVerPattern.IsMatch(value))
                    return false;

                var match = SemVerPattern.Match(value);
                version = new SemVer(
                    major: int.Parse(match.Groups[1].Value),
                    minor: int.Parse(match.Groups[2].Value.TrimStart('.')),
                    patch: int.Parse(match.Groups[3].Value.TrimStart('.')),
                    metadata: string.Empty.Equals(match.Groups[4].Value) ? null : match.Groups[4].Value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int CompareTo(IVersion other) => Version.CompareTo(other.Version);

        public int CompareTo(SemVer other)
        {
            if (this < other)
                return -1;

            if (this == other)
                return 0;

            //if (this > other)
            return 1;
        }

        public static bool operator ==(SemVer first, SemVer second) => first.Equals(second);

        public static bool operator !=(SemVer first, SemVer second) => !(first.Equals(second));

        public static bool operator <(SemVer first, SemVer second)
        {
            if (first.Major >= second.Major)
                return false;

            if (first.Minor >= second.Minor)
                return false;

            if (first.Patch >= second.Patch)
                return false;

            if (first.Metadata.NullOrEquals(second.Metadata))
                return false;

            if (first.Metadata.CompareTo(second.Metadata) >= 0)
                return false;

            return true;
        }

        public static bool operator >(SemVer first, SemVer second)
        {
            if (first.Major <= second.Major)
                return false;

            if (first.Minor <= second.Minor)
                return false;

            if (first.Patch <= second.Patch)
                return false;

            if (first.Metadata.NullOrEquals(second.Metadata))
                return false;

            if (first.Metadata.CompareTo(second.Metadata) <= 0)
                return false;

            return true;
        }

        public static bool operator <=(SemVer first, SemVer second) => first.Equals(second) || first < second;

        public static bool operator >=(SemVer first, SemVer second) => first.Equals(second) || first > second;

        public static implicit operator SemVer(string value) => Parse(value);

    }

    public struct TimeStamp : IVersion, IComparable<TimeStamp>
    {
        public static readonly Regex TimeStampPattern = new Regex("^([0-9a-fA-F]{4}-)*[0-9a-fA-F]{1,4}$");

        public static readonly Regex TicksPattern = new Regex("^\\d+$");

        public static TimeStamp Now => new TimeStamp(Stopwatch.GetTimestamp());

        /// <summary>
        /// Value representing t
        /// </summary>
        public long Stamp { get; }

        public string Version
        {
            get
            {
                return Stamp == 0
                    ? "0"
                    : Stamp.ToString("x")
                        .SplitAtEvery(4)
                        .Select(charr => new string(charr.ToArray()))
                        .JoinUsing("-");
            }
        }

        /// <summary>
        /// Create a new TimeStamp version
        /// </summary>
        /// <param name="stamp">A positive long value representing the timestamp ticks</param>
        public TimeStamp(long stamp)
        {
            Stamp = stamp.ThrowIf(l => l < 0, new ArgumentException($"Negative stamp is forbidden: {stamp}"));
        }

        public int CompareTo(IVersion other) => Version.CompareTo(other.Version);

        public int CompareTo(TimeStamp other) => Stamp.CompareTo(other);

        public override string ToString() => Version;

        public override bool Equals(object obj)
        {
            return obj is TimeStamp other
                && other.Stamp == Stamp;
        }

        public override int GetHashCode() => Stamp.GetHashCode();

        public static TimeStamp Parse(string value)
        {
            if (TryParse(value, out var timeStamp))
                return timeStamp;

            else throw new FormatException($"Invalid input format: {value}");
        }

        public static bool TryParse(string value, out TimeStamp timeStamp)
        {
            timeStamp = default;

            try
            {
                if (TicksPattern.IsMatch(value))
                    timeStamp = new TimeStamp(long.Parse(value));

                else if (TimeStampPattern.IsMatch(value))
                    timeStamp = new TimeStamp(Convert.ToInt64(value.Replace("-", ""), 16));

                else return false;

                return true;
            }
            catch
            {
                return false;
            }
        }


        public static bool operator ==(TimeStamp first, TimeStamp second) => first.Stamp == second.Stamp;

        public static bool operator !=(TimeStamp first, TimeStamp second) => first.Stamp != second.Stamp;

        public static bool operator <(TimeStamp first, TimeStamp second) => first.Stamp < second.Stamp;

        public static bool operator >(TimeStamp first, TimeStamp second) => first.Stamp > second.Stamp;

        public static bool operator <=(TimeStamp first, TimeStamp second) => first.Stamp <= second.Stamp;

        public static bool operator >=(TimeStamp first, TimeStamp second) => first.Stamp >= second.Stamp;

        public static implicit operator TimeStamp(string value) => Parse(value);

    }

    public struct EntityVersion: IComparable<EntityVersion>
    {
        public Guid Identifier { get; }

        public IVersion Version { get; }

        public EntityVersion(IVersion version, Guid identifier)
        {
            Version = version;
            Identifier = identifier;
        }

        public EntityVersion(IVersion version)
            : this(version, Guid.NewGuid())
        { }

        public int CompareTo(EntityVersion other) => ToString().CompareTo(other.ToString());

        public int CompareTo(object obj) => ToString().CompareTo(obj?.ToString() ?? "");

        public override bool Equals(object obj)
        {
            return obj is EntityVersion other
                && other.Version.Equals(Version)
                && other.Identifier == Identifier;
        }

        public override int GetHashCode() => HashCode.Combine(Version, Identifier);

        public override string ToString() => $"{Version}::{Identifier}";

        public static EntityVersion Parse(string value)
        {
            if (TryParse(value, out var version))
                return version;

            else throw new FormatException($"Invalid format: {value}");
        }

        public static bool TryParse(string value, out EntityVersion version)
        {
            version = default;

            try
            {
                if (string.IsNullOrEmpty(value))
                    return false;

                var parts = value.Split("::");

                if (Guid.TryParse(parts[1], out var guid))
                {
                    if(SemVer.TryParse(parts[0], out var semver))
                    {
                        version = new EntityVersion(semver, guid);
                        return true;
                    }
                    else if(TimeStamp.TryParse(parts[0], out var timestamp))
                    {
                        version = new EntityVersion(timestamp, guid);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool operator ==(EntityVersion first, EntityVersion second) => first.Equals(second);

        public static bool operator !=(EntityVersion first, EntityVersion second) => !(first.Equals(second));

        public static bool operator <(EntityVersion first, EntityVersion second) => first.CompareTo(second) < 0;

        public static bool operator >(EntityVersion first, EntityVersion second) => first.CompareTo(second) > 0;

        public static bool operator <=(EntityVersion first, EntityVersion second) => first.Equals(second) || first < second;

        public static bool operator >=(EntityVersion first, EntityVersion second) => first.Equals(second) || first > second;

        public static implicit operator EntityVersion(string value) => Parse(value);
    }


    public struct EntityVersion<TVersion> : IComparable<EntityVersion<TVersion>>
        where TVersion : IVersion
    {
        public Guid Identifier { get; }

        public TVersion Version { get; }

        public EntityVersion(TVersion version, Guid identifier)
        {
            Version = version;
            Identifier = identifier;
        }

        public EntityVersion(TVersion version)
            : this(version, Guid.NewGuid())
        { }

        public int CompareTo(EntityVersion<TVersion> other) => Version.CompareTo(other.Version);

        public override bool Equals(object obj)
        {
            return obj is EntityVersion other
                && other.Version.Equals(Version)
                && other.Identifier == Identifier;
        }

        public override int GetHashCode() => HashCode.Combine(Version, Identifier);

        public override string ToString() => $"{Version}::{Identifier}";

        public static EntityVersion<TVersion> Parse(string value)
        {
            if (TryParse(value, out var version))
                return version;

            else throw new FormatException($"Invalid format: {value}");
        }

        public static bool TryParse(string value, out EntityVersion<TVersion> version)
        {
            version = default;

            try
            {
                if (string.IsNullOrEmpty(value))
                    return false;

                var parts = value.Split("::");

                if (Guid.TryParse(parts[1], out var guid))
                {
                    if (version is EntityVersion<SemVer> 
                        && SemVer.TryParse(parts[0], out var semver))
                        version = new EntityVersion<TVersion>((TVersion)(IVersion)semver, guid); 

                    else if(version is EntityVersion<TimeStamp>
                        && TimeStamp.TryParse(parts[0], out var timestamp))
                        version = new EntityVersion<TVersion>((TVersion)(IVersion)timestamp, guid);

                    else
                        throw new Exception($"Unkonwn generic <{nameof(TVersion)}> type: {typeof(TVersion).FullName}");

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool operator ==(EntityVersion<TVersion> first, EntityVersion<TVersion> second) => first.Equals(second);

        public static bool operator !=(EntityVersion<TVersion> first, EntityVersion<TVersion> second) => !(first.Equals(second));

        public static bool operator <(EntityVersion<TVersion> first, EntityVersion<TVersion> second) => first.CompareTo(second) < 0;

        public static bool operator >(EntityVersion<TVersion> first, EntityVersion<TVersion> second) => first.CompareTo(second) > 0;

        public static bool operator <=(EntityVersion<TVersion> first, EntityVersion<TVersion> second) => first.Equals(second) || first < second;

        public static bool operator >=(EntityVersion<TVersion> first, EntityVersion<TVersion> second) => first.Equals(second) || first > second;

        public static implicit operator EntityVersion<TVersion>(string value) => Parse(value);
    }
}

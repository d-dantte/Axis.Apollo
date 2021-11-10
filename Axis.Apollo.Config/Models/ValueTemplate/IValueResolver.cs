using Axis.Luna.Common.Types.Basic;

namespace Axis.Apollo.Config.Models.ValueTemplate
{
    /// <summary>
    /// Used to indicate the rules to follow while resolving values from the revisions
    /// </summary>
    public enum ResolutionType
    {
        /// <summary>
        /// Match the exact version given
        /// </summary>
        Exact = 0,

        /// <summary>
        /// Find the closest value greater than the given version.
        /// </summary>
        ClosestGreaterThan,

        /// <summary>
        /// Find the closest value less than the given version.
        /// </summary>
        ClosestLessThan,

        /// <summary>
        /// Find the exact version, or closest value greater than the given version
        /// </summary>
        ExactOrClosestGreaterThan,

        /// <summary>
        /// Find the exact version, or the closest value less than the given version
        /// </summary>
        ExactOrClosestLessThan
    }

    /// <summary>
    /// Interface representing constructs that can resolve values from a list of revisions
    /// </summary>
    public interface IValueResolver
    {
        /// <summary>
        /// Resolves the most-recent/highest version of a value.
        /// </summary>
        /// <returns>The resolved value, or null if not found</returns>
        BasicValue? ResolveValue();

        /// <summary>
        /// Resolves based on the given resolution type: defaults to <see cref="ResolutionType.Exact"/>
        /// </summary>
        /// <param name="version">The version to match against</param>
        /// <param name="resolutionType">The resolution type to use in searching</param>
        /// <returns>The resolved value, or null if not found</returns>
        BasicValue? ResolveValue(EntityVersion version, ResolutionType resolutionType = ResolutionType.Exact);
    }
}

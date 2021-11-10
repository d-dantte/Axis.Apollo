using Axis.Luna.Common.Types.Basic;
using Axis.Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Apollo.Config.Models.ValueTemplate
{
    public class DomainNode : IReadonlyIndexer<Domain, DomainNode>, IReadonlyIndexer<EntityVersion, ValueRevision>, IValueResolver
    {
        private readonly Dictionary<Domain, DomainNode> children = new Dictionary<Domain, DomainNode>();
        private readonly SortedList<EntityVersion, ValueRevision> revisions = new SortedList<EntityVersion, ValueRevision>(new DescendingVersionComparer());
        private DomainNode parent;

        public DomainNode this[Domain key]
        {
            get => children[key];
        }

        public ValueRevision this[EntityVersion version]
        {
            get => revisions[version];
        }

        public IEnumerable<Domain> Domains => children.Keys;

        public IEnumerable<EntityVersion> ValueVersions => revisions.Keys;

        public Domain Domain { get; }

        public DomainNode Parent { get; }


        public DomainNode(Domain domain, params DomainNode[] children)
        {
            Domain = domain;
            children?.Aggregate(this.children, (dict, next) =>
            {
                dict[next.Domain] = next;
                return dict;
            });
        }

        public DomainNode(Domain domain, ValueRevision initialValue, params DomainNode[] children)
            : this(domain, children)
        {
            Add(initialValue);
        }

        public DomainNode(params DomainNode[] children)
            : this(default(Domain), children)
        { }

        public DomainNode(ValueRevision initialValue, params DomainNode[] children)
            : this(default(Domain), initialValue, children)
        { }


        public bool TryAddRevision(ValueRevision revision)
        {
            if (revisions.ContainsKey(revision.Version))
                return false;

            revisions[revision.Version] = revision;
            return true;
        }

        public void Add(ValueRevision revision)
        {
            if (!TryAddRevision(revision))
                throw new ArgumentException($"A revision already exists with the given version: {revision.Version}");
        }

        public bool TryAddDomain(DomainNode node)
        {
            if (children.ContainsKey(node.Domain))
                return false;

            children[node.Domain] = node;
            node.parent = this;
            return true;
        }

        public void Add(DomainNode node)
        {
            if (!TryAddDomain(node))
                throw new ArgumentException($"A Node already exists with the given domain: {node.Domain}");
        }


        public override string ToString()
        {
            var ancestryString = parent != null
                ? $"{parent}/"
                : "";

            return $"{ancestryString}{Domain}";
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        BasicValue? IValueResolver.ResolveValue() => revisions.Values.FirstOrDefault()?.Value;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public BasicValue? ResolveValue(EntityVersion version, ResolutionType resolutionType = ResolutionType.Exact)
        {
            var result = revisions.Values.ToArray().CustomBinarySearch(version);

            switch (resolutionType)
            {
                case ResolutionType.Exact: return result.Revision?.Value;
                case ResolutionType.ExactOrClosestGreaterThan: return result.Revision?.Value ?? revisions.ValueAtOrDefault(result.Index);
                case ResolutionType.ExactOrClosestLessThan: return result.Revision?.Value ?? revisions.ValueAtOrDefault(result.Index - 1);
                case ResolutionType.ClosestGreaterThan: return revisions.ValueAtOrDefault(result.Index);
                case ResolutionType.ClosestLessThan: return revisions.ValueAtOrDefault(result.Index - 1);
                default: throw new InvalidOperationException($"Invalid {nameof(resolutionType)}: {resolutionType}");
            }
        }


        internal class DescendingVersionComparer : IComparer<EntityVersion>
        {
            public int Compare(EntityVersion x, EntityVersion y) => y.CompareTo(x);
        }
    }
}

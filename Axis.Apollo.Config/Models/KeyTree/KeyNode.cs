using Axis.Apollo.Config.Models.ValueTemplate;
using Axis.Luna.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Axis.Apollo.Config.Models.KeyTree
{
    public class KeyNode: IReadonlyIndexer<string, KeyNode>
    {
        private static readonly Regex KeyPattern = new Regex("^(\\[\\d+\\])|([0-9a-zA-Z_-]+(\\.[0-9a-zA-Z_-]+)*)$");

        private readonly Dictionary<string, KeyNode> children = new Dictionary<string, KeyNode>();
        private KeyNode parent = null;

        public string Key { get; }

        public DomainNode Value { get; }

        public KeyNode this[string key]
        {
            get => children[key];
        }

        public IEnumerable<KeyNode> Children => children.Values;

        public KeyNode(string key, params KeyNode[] children)
        {
            Key = KeyPattern.IsMatch(key) ? key : throw new ArgumentException($"Invalid key: {key}");
            children?.Aggregate(this.children, (dict, node) =>
            {
                dict[node.Key] = node;
                node.parent = this;
                return dict;
            });
        }

        public KeyNode(string key, DomainNode value)
            :this(key, (KeyNode[])null)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool TryGetNode(string key, out KeyNode node) => children.TryGetValue(key, out node);

        public bool ContainsKey(string key) => children.ContainsKey(key);

        public override string ToString()
        {
            var ancestryString = parent != null
                ? $"{parent}/"
                : "";

            return $"{ancestryString}{Key}";
        }

        public override int GetHashCode() => Key.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is KeyNode other
                && other.Key.Equals(Key);
        }
    }
}

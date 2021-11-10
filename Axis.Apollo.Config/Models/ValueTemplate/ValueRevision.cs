using Axis.Luna.Common.Types.Basic;

namespace Axis.Apollo.Config.Models.ValueTemplate
{
    public class ValueRevision
    {
        public EntityVersion Version { get; }

        public BasicValue Value { get; }

        public ValueRevision(BasicValue value, EntityVersion version)
        {
            Version = version;
            Value = value;
        }
    }
}

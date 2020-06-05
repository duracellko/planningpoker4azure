using System;

namespace Duracellko.PlanningPoker.Domain.Test
{
    public class GuidProviderMock : GuidProvider
    {
        private Guid _guid;

        public GuidProviderMock()
            : this(DefaultGuid)
        {
        }

        public GuidProviderMock(Guid newGuid)
        {
            _guid = newGuid;
        }

        public static Guid DefaultGuid { get; } = Guid.Parse("439dcfb2-89d3-419e-a953-1a4b4cabd9fa");

        public override Guid NewGuid() => _guid;

        public void SetGuid(Guid value)
        {
            _guid = value;
        }
    }
}

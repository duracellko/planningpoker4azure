using System;

namespace Duracellko.PlanningPoker.Domain.Test
{
    public class DateTimeProviderMock : DateTimeProvider
    {
        private DateTime _now = DateTime.Now;
        private DateTime _utcNow = DateTime.UtcNow;

        public override DateTime Now => _now;

        public override DateTime UtcNow => _utcNow;

        public void SetNow(DateTime value)
        {
            _now = value;
        }

        public void SetUtcNow(DateTime value)
        {
            _utcNow = value;
        }
    }
}

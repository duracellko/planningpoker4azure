using System;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure.Test
{
    public class DateTimeProviderMock : DateTimeProvider
    {
        private DateTime _now = DateTime.Now;
        private DateTime _utcNow = DateTime.UtcNow;

        public override DateTime Now
        {
            get
            {
                return _now;
            }
        }

        public override DateTime UtcNow
        {
            get
            {
                return _utcNow;
            }
        }

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

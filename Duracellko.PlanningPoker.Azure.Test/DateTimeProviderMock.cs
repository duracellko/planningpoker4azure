// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure.Test
{
    public class DateTimeProviderMock : DateTimeProvider
    {
        private DateTime now = DateTime.Now;
        private DateTime utcNow = DateTime.UtcNow;

        public override DateTime Now
        {
            get
            {
                return this.now;
            }
        }

        public override DateTime UtcNow
        {
            get
            {
                return this.utcNow;
            }
        }

        public void SetNow(DateTime value)
        {
            this.now = value;
        }

        public void SetUtcNow(DateTime value)
        {
            this.utcNow = value;
        }
    }
}

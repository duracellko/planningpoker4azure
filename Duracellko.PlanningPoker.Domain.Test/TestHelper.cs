// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain.Test
{
    public static class TestHelper
    {
        public static void ClearMessages(Observer observer)
        {
            while (observer.HasMessage)
            {
                observer.PopMessage();
            }
        }
    }
}

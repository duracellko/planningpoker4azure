using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    internal static class ArrayRangeExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this ArrayRange<T> source)
        {
            return new ArraySegment<T>(source.Array, 0, source.Count);
        }
    }
}
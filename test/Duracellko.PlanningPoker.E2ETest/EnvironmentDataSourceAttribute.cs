using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.E2ETest
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EnvironmentDataSourceAttribute : Attribute, ITestDataSource
    {
        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            yield return new object[] { false, BrowserType.Chrome, false };
            yield return new object[] { true, BrowserType.Chrome, false };
        }

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "StyleCop does not support nullable syntax.")]
        public string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
        {
            ArgumentNullException.ThrowIfNull(data);

            var blazorType = ((bool)data[0]!) ? "Server-side" : "Client-side";
            var browserType = data[1]!.ToString();
            var connectionType = ((bool)data[2]!) ? "HttpClient" : "SignalR";
            return $"{blazorType} {browserType} {connectionType}";
        }
    }
}

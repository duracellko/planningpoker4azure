using System;
using System.Collections.Generic;
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

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var blazorType = ((bool)data[0]) ? "Server-side" : "Client-side";
            var browserType = data[1].ToString();
            var connectionType = ((bool)data[2]) ? "HttpClient" : "SignalR";
            return $"{blazorType} {browserType} {connectionType}";
        }
    }
}

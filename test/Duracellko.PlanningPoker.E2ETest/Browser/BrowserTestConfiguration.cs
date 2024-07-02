using System;

namespace Duracellko.PlanningPoker.E2ETest.Browser;

public class BrowserTestConfiguration
{
    public BrowserTestConfiguration(string className, string testName, bool serverSide, bool useHttpClient)
    {
        if (string.IsNullOrEmpty(className))
        {
            throw new ArgumentNullException(nameof(className));
        }

        if (string.IsNullOrEmpty(testName))
        {
            throw new ArgumentNullException(nameof(testName));
        }

        ClassName = className;
        TestName = testName;
        ServerSide = serverSide;
        UseHttpClient = useHttpClient;
    }

    public string ClassName { get; }

    public string TestName { get; }

    public bool ServerSide { get; }

    public bool UseHttpClient { get; }
}

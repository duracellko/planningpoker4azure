using System;
using Duracellko.PlanningPoker.Client.Service;

namespace Duracellko.PlanningPoker.Web;

public class PlanningPokerServerUriProvider : IPlanningPokerUriProvider
{
    public Uri? BaseUri { get; private set; }

    public void InitializeBaseUri(Uri baseUri)
    {
        BaseUri = baseUri;
    }
}

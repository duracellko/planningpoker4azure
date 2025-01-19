using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Duracellko.PlanningPoker.Web.Model;

public class HomeModel
{
    public HomeModel(PlanningPokerClientConfiguration clientConfiguration, ClientScriptsLibrary clientScripts)
    {
        ClientConfiguration = clientConfiguration ?? throw new ArgumentNullException(nameof(clientConfiguration));
        ClientScripts = clientScripts ?? throw new ArgumentNullException(nameof(clientScripts));
    }

    public PlanningPokerClientConfiguration ClientConfiguration { get; }

    public ClientScriptsLibrary ClientScripts { get; }

    public IComponentRenderMode ApplicationRenderMode
    {
        get
        {
            return ClientConfiguration.ApplicationMode switch
            {
                ApplicationMode.Auto => RenderMode.InteractiveAuto,
                ApplicationMode.ClientSide => RenderMode.InteractiveWebAssembly,
                ApplicationMode.ServerSide => RenderMode.InteractiveServer,
                _ => RenderMode.InteractiveAuto
            };
        }
    }
}

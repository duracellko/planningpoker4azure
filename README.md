# Planning Poker 4 Azure

**Live** @ [http://planningpoker.duracellko.net/](http://planningpoker.duracellko.net/)

- [Overview](#Overview)
- [How to play Planning Poker](#How-to-play-Planning-Poker)
- [Installation](#Installation)
- [Architecture](#Architecture)
- [Configuration](#Configuration)
- [Buid and test](#Buid-and-test)
- [Solution projects](#Solution-projects)
- [Attributions](#Attributions)

## Overview

Planning Poker web application allows distributed teams to play [planning poker](https://en.wikipedia.org/wiki/Planning_poker) using just their web browser and is optimized for mobile. The application does not require any registration at all. Scrum Master simply creates a team and all other members connect to the team. Observer role is supported too. The observer can watch the game, but cannot estimate. This is ideal role for a product owner.

## How to play Planning Poker

This guide assumes that you already know what planning poker is about and how to play with real cards. If you need any information about planning poker see the [wikipedia page](https://en.wikipedia.org/wiki/Planning_poker). To play planning poker using this application is very simple. For distributed team a conference call is required as this is not provided by the application.

1. At first Scrum Master creates new team by entering Team name and Scrum master name.
2. Then all team members join the team by entering the same Team name and unique member name.
3. Optionally product owner can join the team by entering the same Team name, unique member name and checking Observer checkbox.
4. When all participants joined the team, the team can discuss first story.
5. After discussion Scrum Master starts the estimation.
6. Then all members pick estimates. Optionally Scrum Master can cancel estimation when any of the members requests that.
7. After all members picked an estimate, the estimates are shown to all participants.
8. If concensus is found then team can discuss another story and Scrum Master can start estimation again. Otherwise team discusses their estimates and then Scrum Master starts estimation again.

![Planning Poker screenshot](docs/images/Screenshot.png)

## Installation

Requirements:

- .NET Core 3.1 runtime

Run: `dotnet Duracellko.PlanningPoker.Web.dll`

### Docker

It is also possible to install the application using Docker image from [duracellko/planningpoker](https://hub.docker.com/r/duracellko/planningpoker) repository.

```
docker pull duracellko/planningpoker
docker run -p 80:80 duracellko/planningpoker
```

## Architecture

Application is implemented using [ASP.NET Core 3.1](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-3.1). Front-end is Single-Page Application implemented using [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor). This technology allows deployment to different environments:

- Locally on Windows or Linux
- In Docker container
- In [Azure AppService](https://azure.microsoft.com/en-us/services/app-service/)
- or any environment supported by ASP.NET Core

The application does not have any dependencies to run in basic mode. However, there are configurable advanced features.

### Blazor models

Blazor supports 2 [hosting models](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-3.0): Client-side and Server-side. It is possible to simply switch between the models by configuring **UseServerSide** setting.

### Teams storage

By default, when server application is restarted, all teams are lost. It is possible to specify folder to store teams data between application restarts. .NET binary serialization is used to store team data.

### Azure Service Bus

The application is state-full (it stores all teams data in memory). Therefore, it is not simple to deploy the application to a web-farm. Each server in web-farm would have different state of team. However, it is possible to setup [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/) to synchronize state between nodes in web-farm.

## Configuration

The application can be configured using default ASP.NET Core configuration sources:

- appsettings.json
- appsettings._Environment_.json
- Environment variables
- Command-line arguments

The application has following configuration settings:

```javascript
{
    "PlanningPokerClient": {
        "UseServerSide": false // boolean
    },
    "PlanningPoker": {
        "RepositoryFolder": "", // string
        "RepositoryTeamExpiration": 1200, // integer - time in seconds
        "ClientInactivityTimeout": 900, // integer - time in seconds
        "ClientInactivityCheckInterval": 60, // integer - time in seconds
        "WaitForMessageTimeout": 60, // integer - time in seconds

        // Azure Service Bus configuration
        "ServiceBusConnectionString": "", // string
        "ServiceBusTopic": "PlanningPoker", // string
        "InitializationTimeout": 60, // integer - time in seconds
        "InitializationMessageTimeout": 5, // integer - time in seconds
        "SubscriptionMaintenanceInterval": 300, //integer - time in seconds
        "SubscriptionInactivityTimeout": 300 // integer - time in seconds
    }
}
```

- **UseServerSide** (default: false) - When true, Blazor is run in server-side and HTML is synchronized with browser using SignalR. Otherwise Blazor runs in WebAssembly on client.
- **RepositoryFolder** (default: empty) - Path to folder, where data are stored between application restarts. Path is relative to the application folder. When this setting is empty, no data are stored and all are lost on application restart.
- **RepositoryTeamExpiration** (default: 1200) - Team is deleted after specified time with no user activity.
- **ClientInactivityTimeout** (default: 900) - User is disconnected from the team after specified time with no connection from the user.
- **ClientInactivityCheckInterval** (default: 60) - Time interval to run periodic job that disconnects innactive users.
- **WaitForMessageTimeout** (default: 60) - Each client requests regularly for status updates. When there is no change in specified time, client receives response that there is no change and requests for update again. This way client notifies to keep connection is alive.
- **ServiceBusConnectionString** (default: empty) - Connection string to Azure Service Bus used to synchronize data between servers in web-farm. For example: Endpoint=myEndpoint;SharedSecretIssuer=mySecret;SharedSecretValue=myPassword;
- **ServiceBusTopic** (default: PlanningPoker) - Nodes uses the specified Service Bus topic for communication.
- **InitializationTimeout** (default: 60) - Time after initialization phase is cancelled and server assumes that it is alone. This timeout should not be reached, because InitializationMessageTimeout is shorter.
- **InitializationMessageTimeout** (default: 5) - Timeout to wait for message from another node during initialization phase. When the timeout is reached, server contacts another node or assumes that it is alone in web-farm.
- **SubscriptionMaintenanceInterval** (default: 300) - Time interval to do periodic check, which nodes in web-farm are responding.
- **SubscriptionInactivityTimeout** (default: 300) - Service Bus subcsriptions are deleted for nodes, which do not respond in the specified time.

## Buid and test

Requirements:

- .NET Core SDK 3.1
- Java SE Development Kit version 8 or higher (for end-2-end tests only)
- Node.js and NPM (for end-2-end tests only)

To run build and tests simply execute PowerShell script BuildAndRun.ps1.

```powershell
.\BuildAndRun.ps1
```

Optionally it is possible to include execution of end-2-end tests using Selenium.

```powershell
.\BuildAndRun.ps1 -E2ETest:$true
```

### Run in Visual Studio

**PlanningPokerCore.sln** solution can be normally open, built and debugged in Visual Studio 2019. Also unit tests can be executed.

For end-2-end tests (Duracellko.PlanningPoker.E2ETest) Selenium drivers need to be downloaded. Simply execute following commands:

```
npm install
.\node_modules\.bin\selenium-standalone install
```

## Solution projects

PlanningPoker solution contains following projects:

* **PlanningPoker.Shared** contains common settings for all projects. This includes build configuration, static code analysis, assembly version, etc.
* **PlanningPoker.Domain** contains domain classes of the application. This includes domain logic and entities e.g. ScrumMaster, Estimation.
* **PlanningPoker** implements host of domain objects, JSON web service and file-system repository.
* **PlanningPoker.Service** implements DTOs shared between server and client.
* **PlanningPoker.Web** is host application. It starts ASP.NET Core hosting, dependency injection and loads configuration.
* **PlanningPoker.Client** is Blazor SPA client. It containes 2 pages, Blazor components and communication with the server.
* **PlanningPoker.Azure** contains modified host of domain objects used on Windows Azure platform. Additionally it implements communication between cloud instances using Service Bus.
* **PlanningPoker.Domain.Test** contains unit-tests of domain classes.
* **PlanningPoker.Test** contains unit-tests of PlanningPoker project classes.
* **PlanningPoker.Client.Test** contains unit-tests of client application.
* **PlanningPoker.Azure.Test** contains unit-tests of classes in PlanningPoker.Azure project.
* **PlanningPoker.E2ETest** contains end-2-end tests of full system. Tests are implemented using Selenium and tests mostly Chrome browser and partially Firefox.

### Attributions

This application including source code can be used under [MIT License](License.txt).

* [Planning Poker](https://www.mountaingoatsoftware.com/agile/planning-poker/license) is a registered trademark of Mountain Goat Software, LLC.
* [Rock, Paper, Scissors, Lizard, Spock](http://www.samkass.com/theories/RPSSL.html) is licensed under [CC BY-NC 3.0](https://creativecommons.org/licenses/by-nc/3.0/).
* [Open Iconic](https://useiconic.com/open) is licensed under [MIT](https://opensource.org/licenses/MIT) and [OFL](https://scripts.sil.org/cms/scripts/page.php?item_id=OFL_web).

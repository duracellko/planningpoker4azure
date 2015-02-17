# Planning Poker 4 Azure

Planning Poker web application allows distributed teams to play planning poker using just their web browser. The application does not require any registration at all. Scrum Master just creates a team and all other members connect to the team. Observer role is supported too. The observer can watch the game, but cannot play planning poker. This is perfect role for product owner.

The application can be deployed to Internet Information Server (on-premise) or to cloud on [Windows Azure](http://www.windowsazure.com).

Another goal of this project is to describe, how to develop similar applications (application with real-time communication), so they are prepared for Azure. Cloud services uses ServiceBus to communicate to each other. The application is written in C# and uses ASP.NET MVC 5.

Live @ http://planningpoker.duracellko.net/

More information about planning poker can be found at Wikipedia: http://en.wikipedia.org/wiki/Planning_poker

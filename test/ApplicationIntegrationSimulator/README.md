# Planning Poker Application Integration simulator
Planning Poker can integrate with other applications like Azure DevOps. This is static web site that simulates 3rd-party application that integrates with the Planning Poker.

# How to use
Please use [dotnet-serve](https://www.nuget.org/packages/dotnet-serve/) tool to server this static website.

# Integration details
Planning Poker provides 2 service points for integration with other applications.

## Opening Planning Poker
An application can open Planning Poker with provided team name and user name and give instructions to automatically join the team. When the team is not started yet, Planning Poker asks user to confirm that he/she is the Scrum Master and that the team should be created. Whne the team already exists, then user automatically joins the team.

This works by providing following parameters in URL query string. All parameters are mandatory.
- **AutoConnect** - when the value is `true`, then user automatically joins the team. This is considered, only when other parameters are provided too.
- **CallbackUri** - URI of the application for integration. This URI is used to send the estimation back to the application.
- **CallbackReference** - A custom string that the application can use to identify callback. It can be a work item ID to link the estimate to the work item.

Team name and user name are provided in URL path in format: Index/{TeamName}/{UserName}

Example of complete URL:
https://planningpoker.duracellko.net/Index/MyTeam/Alice?AutoConnect=True&CallbackUri=https%3A%2F%2Fdev.azure.com%2F&CallbackReference=2

## Sending estimation result to an application
When Planning Poker is opened with a callback specified, then estimation result summary (average, median, sum) has buttons to send it back to the application. This is done using [window.postMessage()](https://developer.mozilla.org/docs/Web/API/Window/postMessage) function. **CallbackUri** is used as as **target** for the `postMessage`. And the message is a JSON object with 2 properties:
- **Reference** - the value provided by application in CallbackReference.
- **Estimation** - the calculated estimation in the Planning Poker.

Example of the message:
```json
{
    "reference": "22",
    "estimation": 5.2
}
```

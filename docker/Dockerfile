FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app
COPY ./app/ ./

USER $APP_UID

ENV ASPNETCORE_URLS=http://*:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Duracellko.PlanningPoker.Web.dll"]
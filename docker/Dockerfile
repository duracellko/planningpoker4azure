FROM mcr.microsoft.com/dotnet/aspnet:6.0

RUN apt-get update && \
    apt-get install -y curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY ./app/ ./

RUN useradd --home /home/app app  && \
    mkdir /home/app && \
    chown -R app /home/app && \
    chmod -R u=rwx,g=rx,o= /home/app &&\
    chmod -R u=rwx,g=rx,o=rx /app

USER app

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://*:8080

EXPOSE 8080

HEALTHCHECK CMD curl -f -m 15 http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Duracellko.PlanningPoker.Web.dll"]
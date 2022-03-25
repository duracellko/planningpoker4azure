FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /app
COPY ./app/ ./

RUN useradd --home /home/app app  && \
    mkdir /home/app && \
    chown -R app /home/app && \
    chmod -R u=rwx,g=rx,o= /home/app &&\
    chmod -R u=rwx,g=rx,o=rx /app

USER app

ENV ASPNETCORE_URLS=http://*:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Duracellko.PlanningPoker.Web.dll"]
# syntax=docker/dockerfile:1.7-labs
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

ARG GH_USERNAME
ARG GH_TOKEN

WORKDIR /app
COPY --parents ./src/**/*.csproj .
COPY ./*.sln .
COPY ./nuget.config .
RUN dotnet nuget update source github.in-duck-tor -u $GH_USERNAME -p $GH_TOKEN --store-password-in-clear-text
RUN dotnet restore --runtime linux-x64 

COPY . .
RUN dotnet build -c Release --no-restore ./src/InDuckTor.Account.Worker/InDuckTor.Account.Worker.csproj
RUN dotnet publish -c Release -o ./publish/ --no-restore ./src/InDuckTor.Account.Worker/InDuckTor.Account.Worker.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
VOLUME /app/certs
WORKDIR /app
COPY --from=build /app/publish .
ENV OTEL_RESOURCE_ATTRIBUTES="service.name=account-worker"
ENV ASPNETCORE_URLS="http://*:80"
ENTRYPOINT ["dotnet", "InDuckTor.Account.Worker.dll"]
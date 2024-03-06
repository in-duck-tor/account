FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

ARG GH_USERNAME 
ARG GH_TOKEN

WORKDIR /app 
COPY . .

RUN dotnet nuget update source github.in-duck-tor -u $GH_USERNAME -p $GH_TOKEN --store-password-in-clear-text
RUN dotnet restore --runtime linux-x64 
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o ./publish/ --no-restore 

FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "InDuckTor.Account.WebApi.dll"]
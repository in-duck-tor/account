FROM mcr.microsoft.com/dotnet/aspnet:8.0 as build

WORKDIR /app 
COPY . .

RUN dotnet nuget add source https://nuget.pkg.github.com/in-duck-tor/index.json -n github.in-duck-tor -u $GH_USERNAME -p $GH_TOKEN 
RUN dotnet restore dotnet restore --runtime linux-x64 
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o ./publish/ --no-restore 

FROM build as runtime
WORKDIR /app
COPY /app/publish .
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "InDuckTor.Account.WebApi.dll"]
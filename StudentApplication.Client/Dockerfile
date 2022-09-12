FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["StudentApplication.Client/StudentApplication.Client.csproj", "StudentApplication.Client/"]
RUN dotnet restore "StudentApplication.Client/StudentApplication.Client.csproj"
COPY . .
WORKDIR "/src/StudentApplication.Client"
RUN dotnet build "StudentApplication.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentApplication.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentApplication.Client.dll"]

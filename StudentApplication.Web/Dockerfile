FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["StudentApplication.Web/StudentApplication.Web.csproj", "StudentApplication.Web/"]
RUN dotnet restore "StudentApplication.Web/StudentApplication.Web.csproj"
COPY . .
WORKDIR "/src/StudentApplication.Web"
RUN dotnet build "StudentApplication.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentApplication.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentApplication.Web.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ElasticsearchUI/ElasticsearchUI.csproj", "ElasticsearchUI/"]
RUN dotnet restore "ElasticsearchUI/ElasticsearchUI.csproj"
COPY . .
WORKDIR "/src/ElasticsearchUI"
RUN dotnet build "ElasticsearchUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ElasticsearchUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ElasticsearchUI.dll"]

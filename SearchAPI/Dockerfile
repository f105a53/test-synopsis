FROM microsoft/dotnet:2.2-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["SearchAPI/SearchAPI.csproj", "SearchAPI/"]
RUN dotnet restore "SearchAPI/SearchAPI.csproj"
COPY . .
WORKDIR "/src/SearchAPI"
RUN dotnet build "SearchAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SearchAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SearchAPI.dll"]
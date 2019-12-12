FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["PreviewService.Console/PreviewService.Console.csproj", "PreviewService.Console/"]
COPY ["PreviewService.Core/PreviewService.Core.csproj", "PreviewService.Core/"]
RUN dotnet restore "PreviewService.Console/PreviewService.Console.csproj"
COPY . .
WORKDIR "/src/PreviewService.Console"
RUN dotnet build "PreviewService.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PreviewService.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PreviewService.Console.dll"]
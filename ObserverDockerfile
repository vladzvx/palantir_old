FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ObserverBot.Service/ObserverBot.Service.csproj", "ObserverBot.Service/"]
COPY ["Bot.Core/Bot.Core.csproj", "Bot.Core/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["SearchBot.Service/SearchBot.Service.csproj", "SearchBot.Service/"]
RUN dotnet restore "ObserverBot.Service/ObserverBot.Service.csproj"
COPY . .
WORKDIR "/src/ObserverBot.Service"
RUN dotnet build "ObserverBot.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ObserverBot.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ObserverBot.Service.dll"]
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Bot.Service/Bot.Service.csproj", "Bot.Service/"]
COPY ["Bot.Core/Bot.Core.csproj", "Bot.Core/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Bot.Service/Bot.Service.csproj"
COPY . .
WORKDIR "/src/Bot.Service"
RUN dotnet build "Bot.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bot.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bot.Service.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["Malia/Malia.csproj", "Malia/"]
WORKDIR /src/Malia

RUN dotnet restore "Malia.csproj"

COPY . .

RUN dotnet publish "Malia.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Malia.dll"]
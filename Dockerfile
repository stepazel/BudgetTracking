FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BudgetTrackingApp/BudgetTrackingApp.fsproj", "BudgetTrackingApp/"]
RUN dotnet restore "BudgetTrackingApp/BudgetTrackingApp.fsproj"
COPY . .
WORKDIR "/src/BudgetTrackingApp"
RUN dotnet build "BudgetTrackingApp.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BudgetTrackingApp.fsproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BudgetTrackingApp.dll"]

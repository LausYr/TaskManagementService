FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskManagementService.EventConsumer/TaskManagementService.EventConsumer.csproj", "TaskManagementService.EventConsumer/"]
COPY ["TaskManagementService.Infrastructure/TaskManagementService.Infrastructure.csproj", "TaskManagementService.Infrastructure/"]
COPY ["TaskManagementService.Domain/TaskManagementService.Domain.csproj", "TaskManagementService.Domain/"]
RUN dotnet restore "TaskManagementService.EventConsumer/TaskManagementService.EventConsumer.csproj"
COPY . .
WORKDIR "/src/TaskManagementService.EventConsumer"
RUN dotnet build "TaskManagementService.EventConsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagementService.EventConsumer.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagementService.EventConsumer.dll"]
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskManagementService.API/TaskManagementService.API.csproj", "TaskManagementService.API/"]
COPY ["TaskManagementService.Application/TaskManagementService.Application.csproj", "TaskManagementService.Application/"]
COPY ["TaskManagementService.Domain/TaskManagementService.Domain.csproj", "TaskManagementService.Domain/"]
COPY ["TaskManagementService.Infrastructure/TaskManagementService.Infrastructure.csproj", "TaskManagementService.Infrastructure/"]
RUN dotnet restore "TaskManagementService.API/TaskManagementService.API.csproj"
COPY . .
WORKDIR "/src/TaskManagementService.API"
RUN dotnet build "TaskManagementService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagementService.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "TaskManagementService.API.dll"]
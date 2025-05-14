FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskManagementService.Listener/TaskManagementService.Listener.csproj", "TaskManagementService.Listener/"]
COPY ["TaskManagementService.Application/TaskManagementService.Application.csproj", "TaskManagementService.Application/"]
COPY ["TaskManagementService.Domain/TaskManagementService.Domain.csproj", "TaskManagementService.Domain/"]
RUN dotnet restore "TaskManagementService.Listener/TaskManagementService.Listener.csproj"
COPY . .
WORKDIR "/src/TaskManagementService.Listener"
RUN dotnet build "TaskManagementService.Listener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagementService.Listener.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "TaskManagementService.Listener.dll"]
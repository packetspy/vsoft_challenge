FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything at once
COPY . .

# Restore and build
RUN dotnet restore
RUN dotnet build -c Release -o /app/build
RUN dotnet publish "src/TaskManagement.Api/TaskManagement.Api.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
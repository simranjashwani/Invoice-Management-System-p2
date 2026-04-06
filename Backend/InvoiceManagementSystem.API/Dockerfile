# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "InvoiceManagementSystem.API/InvoiceManagementSystem.API.csproj"
RUN dotnet publish "InvoiceManagementSystem.API/InvoiceManagementSystem.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

EXPOSE 5000
EXPOSE 5001

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "InvoiceManagementSystem.API.dll"]
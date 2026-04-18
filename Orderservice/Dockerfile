# Stage 1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

#copy project file and restore
COPY ["OrderService.csproj", "./"]
RUN dotnet restore "OrderService.csproj"

#copy everything else and build
COPY . .
RUN dotnet publish "OrderService.csproj" -c Release  -o /app/publish /p:UseAppHost=false

# Stage 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# expose port 8082 inside the container
EXPOSE 8082

# start the app
ENTRYPOINT ["dotnet", "OrderService.dll"]
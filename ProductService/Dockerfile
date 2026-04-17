# Stage 1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

#copy project file and restore
COPY ["ProductService.csproj", "./"]
RUN dotnet restore "ProductService.csproj"

#copy everything else and build
COPY . .
RUN dotnet publish "ProductService.csproj" -c Release  -o /app/publish /p:UseAppHost=false

# Stage 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# expose port 8080 inside the container
EXPOSE 8081

# start the app
ENTRYPOINT ["dotnet", "ProductService.dll"]
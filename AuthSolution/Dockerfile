# Build stage for MAUI app not supported in Docker - provide backend-run-only container
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# This container is only for hosting the Api backend; MAUI app runs on devices
COPY --from=api-build /app/publish ./
ENTRYPOINT ["dotnet", "Api.dll"]

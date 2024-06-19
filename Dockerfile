FROM mcr.microsoft.com/dotnet/aspnet:8.0

EXPOSE 9005
ENV ASPNETCORE_URLS http://+:9005

WORKDIR /app
COPY "release" .

ENTRYPOINT ["dotnet", "OrchardCore.Cms.Web.dll"]
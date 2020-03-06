#
#multi-stage target: dev
#
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS dev

ENV ASPNETCORE_URLS=http://0.0.0.0:4304 \
    ASPNETCORE_ENVIRONMENT=DEVELOPMENT

COPY . /app
WORKDIR /app

RUN dotnet publish -c Release -o /app/dist

CMD ["dotnet", "run"]

#
#multi-stage target: prod
#
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS prod
ARG commit
ENV COMMIT=$commit
COPY --from=dev /app/dist /app

WORKDIR /app
ENV ASPNETCORE_URLS=http://*:80
EXPOSE 80

CMD ["dotnet", "S3.Vm.Console.dll"]

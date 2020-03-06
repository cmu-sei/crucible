#
#multi-stage target: dev
#
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS dev

ENV ASPNETCORE_URLS=http://0.0.0.0:4302 \
    ASPNETCORE_ENVIRONMENT=DEVELOPMENT

# Must be copied above both steamfitter and stackstorm. Will not accept ../stackstorm.api for copying.
COPY . /app
WORKDIR /app/Steamfitter.Api

RUN dotnet publish -c Release -o /app/dist

CMD ["dotnet", "run"]

#
#multi-stage target: prod
#
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS prod
COPY --from=dev /app/dist /app

WORKDIR /app
ENV ASPNETCORE_URLS=http://*:80
EXPOSE 80
CMD [ "dotnet", "Steamfitter.Api.dll" ]
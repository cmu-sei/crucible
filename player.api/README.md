## Docker
this application has been updated to the official Microsoft docker sdk image `mcr.microsoft.com/dotnet/core/sdk` 

### sample `docker-compose.yml`

```yml
version: "3.6"

services:
  player-api:
    image: INTERNAL_NUGET_SERVER/cwd-docker/crucible-bamboo/player-api:latest
    # Overrides the default entrypoint to update certificates.
    entrypoint: bash -c "update-ca-certificates && dotnet S3.Player.Api.dll"
    volumes:
      - sei-ca:/usr/local/share/ca-certificates # Mounts NFS for ca Certificates
    configs: 
      - source: player-api-settings
        target: /app/appsettings.Production.json
volumes:
  sei-ca:
    driver_opts:
      type: "nfs"
      o: "addr=<NFS IP>,nolock,soft,rw" # Replace <NFS IP> 
      device: ":/mnt/data/certificates/sei-ca"

```

### sample `docker-stack.yml` (swarm) includes traefik reverse proxy labels
```yml
version: "3.6"

services:
  player-api:
    image: INTERNAL_NUGET_SERVER/cwd-docker/crucible-bamboo/player-api:latest
    # Overrides the default entrypoint to update certificates.
    entrypoint: bash -c "update-ca-certificates && dotnet S3.Player.Api.dll"
    deploy:
      replicas: 1
      labels:
        - "traefik.enable=true"
        - "traefik.backend=player-api"
        - "traefik.port=80"
        - "traefik.docker.network=traefik-net"
        - "traefik.frontend.rule=Host:<Hostname>" # Replace <Hostname>
        - "traefik.frontend.entrypoints=http,https"
    networks:
      - utilities
      - traefik-net
    volumes:
      - sei-ca:/usr/local/share/ca-certificates
    configs: 
      - source: player-api-settings
        target: /app/appsettings.Production.json
volumes:
  sei-ca:
    driver_opts:
      type: "nfs"
      o: "addr=<NFS IP>,nolock,soft,rw" # Replace <NFS IP>
      device: ":/mnt/data/certificates/sei-ca"
networks:
  utilities:
    external: true
  traefik-net:
    external: true

configs:
  player-api-settings:
    file: ./player-api-anvil-dev-settings.json
```

### SSL Considerations
The official microsoft docker image is based on Debian. SSL CA trusts and their entry scripts need to be updated to use `update-ca-certificates` please see [update-trusts.sh](entry.d/update-trusts.sh)


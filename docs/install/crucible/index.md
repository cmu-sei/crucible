# Installing Crucible

## Resources

* [Documentation on Crucible](https://github.com/cmu-sei/crucible)

> This an example script that installs the Crucible stack.

```bash
function crucible () {
    if [ "${HAS_POSTGRES}" != "true" ]; then
        echo "Postgresql Required"
        exit 1
    fi

    envsubst < values/crucible/player.values.yaml | helm upgrade -i player sei/player -f -

    envsubst < values/crucible/alloy.values.yaml | helm upgrade -i alloy sei/alloy -f -

    envsubst < values/crucible/caster.values.yaml | helm upgrade -i caster sei/caster -f -

    envsubst < values/crucible/blueprint.values.yaml | helm upgrade -i blueprint sei/blueprint -f -

    envsubst < values/crucible/cite.values.yaml | helm upgrade -i cite sei/cite -f -

    envsubst < values/crucible/gallery.values.yaml | helm upgrade -i gallery sei/gallery -f -

    # get MOID for steamfitter
    echo "Attempting to get vsphere cluster"
    MOID=$(pwsh -c 'Connect-VIServer -server $env:VSPHERE_SERVER -user $env:VSPHERE_USER -password $env:VSPHERE_PASS | Out-Null; Get-Cluster -Name $env:VSPHERE_CLUSTER | select -ExpandProperty id')

    MOID=$(echo "${MOID}" | rev | cut -d '-' -f 1,2 | rev)

    if [[ -n ${MOID} ]]; then
        sed -i "s/VmTaskProcessing__ApiParameters__clusters:.*/VmTaskProcessing__ApiParameters__clusters: ${MOID}/" "steamfitter.values.yaml"
        echo "vsphere cluster set"
    fi

    envsubst < values/crucible/steamfitter.values.yaml | helm upgrade -i steamfitter sei/steamfitter -f -

    envsubst < values/crucible/mongodb.values.yaml | helm upgrade -i mongodb bitnami/mongodb -f -

    envsubst < values/crucible/stackstorm-ha.values.yaml | helm upgrade -i stackstorm stackstorm/stackstorm-ha -f - --wait --timeout 10m
}
```

## Player

### Resources on Player

* [Player Helm Chart](https://github.com/cmu-sei/helm-charts/tree/main/charts/player)
* [Player API](https://github.com/cmu-sei/Player.Api)
* [Player UI](https://github.com/cmu-sei/Player.Ui)
* [Console UI](https://github.com/cmu-sei/Console.Ui)
* [VM API](https://github.com/cmu-sei/Vm.Api)
* [VM UI](https://github.com/cmu-sei/Vm.Ui)

This installation of Player utilizes Helm and the Helm chart located [here](https://github.com/cmu-sei/helm-charts/tree/main/charts/player). Within this Helm chart, there are five different services being installed: player-api, player-ui, vm-api, vm-ui, and console-ui.

### Player API

This section will dive into the player-api section of the values yaml file.

#### Player API Ingress

```yaml
  ingress:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
        - path: /player/(hubs|swagger|api)
          pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

As shown above, Player can be path based. The path also includes the endpoints `hubs`, `swagger`, and `api`. This also sets the values of `proxy_read_timeout` and `proxy_send_timeout` in nginx using ingress-nginx annotations. For documentation on these and other annotations, please reference this [documentation](https://github.com/nginxinc/kubernetes-ingress/tree/v3.1.0/examples/ingress-resources/custom-annotations). Finally, change `$DOMAIN` to whichever domain you are using. This is also set in the `env` file.

#### Player Custom Certificates

```yaml
  # If this deployment needs to trust non-public certificates,
  # create a configMap with the needed certificates and specify
  # the configMap name here
  certificateMap: "appliance-root-ca"
```

You are able to include custom certificates by using `certificateMap` within the yaml file. All you need to do is create a configMap with the certificates you would like to place in the container. **You must name the certificates in the configmap with the correct extension.**

#### Player Storage

```yaml
  # storage - either an existing pvc, the size for a new pvc, or emptyDir
  # this is used to store uploaded files
  storage:
    existing: ""
    size: ""
    mode: ReadWriteOnce
    class: longhorn
```

Longhorn is currently being used in the given values yaml file. Please change these values if you are using a different Persistent Volume and Persistent Volume Claim.

#### Player Environment Values

```yaml
env:
    # Proxy Settings - Set these in your values file if you are behind a proxy.
    # http_proxy: proxy.example.com:9000
    # https_proxy: proxy.example.com:9000
    # HTTP_PROXY: proxy.example.com:9000
    # HTTPS_PROXY: proxy.example.com:9000
    # NO_PROXY: .local
    # no_proxy: .local

    ## If hosting in virtual directory, specify path base
    PathBase: "/player"

    Logging__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"

    # CORS policy settings.
    # The first entry should be the URL to player
    # The second entry should be the URL to VM App
    # Subsequent entries can be other integrated apps, such as OSTicket
    CorsPolicy__Origins__0: "https://$DOMAIN"
    CorsPolicy__AllowAnyMethod: true
    CorsPolicy__AllowAnyHeader: true
    # Connection String to database
    # database requires the 'uuid-ossp' extension installed
    ConnectionStrings__PostgreSQL: "Server=postgresql;Port=5432;Database=player_api;Username=$POSTGRES_USER;Password=$POSTGRES_PASS;"

    # OAuth2 Identity Client for Application
    Authorization__Authority: https://$DOMAIN/identity
    Authorization__AuthorizationUrl: https://$DOMAIN/connect/authorize
    Authorization__TokenUrl: https://$DOMAIN/connect/token
    Authorization__AuthorizationScope: "player-api"
    Authorization__ClientId: player-api-dev
    Authorization__ClientName: "Player API"

    # Basic seed data to jumpstart deployement
    # TODO - Document Seed Data
    SeedData__SystemAdminIds__0: dee684c5-2eaf-401a-915b-d3d4320fe5d5
    SeedData__SystemAdminIds__1: 32c11441-7eec-47eb-a915-607c4f2529f4 
```

These are included in the given values yaml file. All of these values can be customized and/or changed for your needed configuration. These values are also in the GitHub repository listed as [appsettings.json](https://github.com/cmu-sei/Player.Api/blob/development/Player.Api/appsettings.json) and also more values are listed on the [player-api Helm chart](https://github.com/cmu-sei/helm-charts/blob/main/charts/player/charts/player-api/values.yaml) within the Player Helm chart.

Most of the environment variables listed in the yaml file are straight forward and only need to be changed to fit your configuration such as `ConnectionStrings__PostgreSQL`, `Authorization__Authority`, and `PathBase: "/player"` if you're not using path-based routing.

The important settings are in the actual chart itself and are not in the default values file:

```yaml
  FileUpload__basePath: '/fileupload'
  FileUpload__maxSize: '64000000'
  FileUpload__allowedExtensions__0: '.pdf'
  FileUpload__allowedExtensions__1: '.png'
  FileUpload__allowedExtensions__2: '.jpg'
  FileUpload__allowedExtensions__3: '.jpeg'
  FileUpload__allowedExtensions__4: '.doc'
  FileUpload__allowedExtensions__5: '.docx'
  FileUpload__allowedExtensions__6: '.gif'
  FileUpload__allowedExtensions__7: '.txt'
```

Listed above are the `FileUpload` settings. You are able to edited the allowed extensions of files that are allowed to be uploaded here. For example, you either remove an entry or add one by adding `FileUpload__allowedExtensions__7: '.iso'` with your included extension. The previous example adds the ability to upload `.iso` files. It's also important to note the `FileUpload__maxSize`. This setting can be changed here but you will also have to increase the `proxy body size` on your Player ingress.

There are also values to change the seed data for the initial install:

```yaml
  SeedData__Permissions__0__Key: SystemAdmin
  SeedData__Permissions__0__Value: 'true'
  SeedData__Permissions__0__Description: 'Can do anything'
  SeedData__Permissions__0__ReadOnly: true
  SeedData__Permissions__1__Key: ViewAdmin
  SeedData__Permissions__1__Value: 'true'
  SeedData__Permissions__1__Description: 'Can edit an View, Add/Remove Teams/Members, etc'
  SeedData__Permissions__1__ReadOnly: true

  SeedData__SystemAdminIds__0: ''
```

These are the defaults and I would not change these settings. Instead, you can add additional seed data by following the same name convention of incrementing the integer within the key.

### Player UI

#### Player UI Ingress

```yaml
  ingress:
    enabled: true    
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: "/player(/|$)(.*)"
            pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

Similar to Player API except with the additional endpoints. Instead, this Ingress uses regex to help with the path-based routing.

#### Player Environment Variables and Settings

```yaml
  env: 
    ## basehref is path to the app
    APP_BASEHREF: "player"

  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client and
  # some basic configuration
  settings: |-
    {
      "ApiUrl": "https://$DOMAIN/player",
      "OIDCSettings": {
        "authority": "https://$DOMAIN/identity",
        "client_id": "player-ui",
        "redirect_uri": "https://$DOMAIN/player/auth-callback",
        "post_logout_redirect_uri": "https://$DOMAIN/player",
        "response_type": "code",
        "scope": "openid profile player-api",
        "automaticSilentRenew": true,
        "silent_redirect_uri": "https://$DOMAIN/player/auth-callback-silent"
      },
      "NotificationsSettings": {
        "url": "https://$DOMAIN/player/hubs",
        "number_to_display": 4
      },
      "AppTitle": "Player",
      "AppTopBarText": "Player",
      "AppTopBarHexColor": "#5F8DB5",
      "AppTopBarHexTextColor": "#FFFFFF",
      "UseLocalAuthStorage": true
    }
```

The only environment variable is `APP_BASEHREF` which is only needed if using path-based routing.

Settings are a json value to set specific settings on the Player UI. These settings are also listed [here](https://github.com/cmu-sei/Player.Ui/blob/09889a4ba69d0fb4d17bb756e358904cdb4e862c/src/assets/config/settings.json) for reference. These are all customizable to your wanted configurations but the above are recommended.

### VM API

#### VM API Ingress

```yaml
  ingress:
    enabled: true  
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: /vm/(notifications|hubs|api|swagger)
            pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

Similar to Player API, the Ingress uses path-based routing and adds additional endpoints.

#### VM API Console Ingress

```yaml
  # VM-API deployment adds a second ingress
  # - This ingress is used as a proxy for getting a websocket
  #   console connection to vCenter hosts.
  # - TLS and Host URLs need configured, but the snippet should be left alone
  # NOTES:
  # - This is only used if RewriteHost__RewriteHost below is true, otherwise
  #   connections will go directly from the UI to the vCenter hosts themselves
  # - The host value here corresponds to RewriteHost__RewriteHostUrl below
  consoleIngress:
    deployConsoleProxy: true  
    className: ""
    name: player-connect
    annotations: 
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: "86400"
      nginx.ingress.kubernetes.io/proxy-send-timeout: "86400"
      nginx.ingress.kubernetes.io/server-snippet: |
        location /ticket {
            proxy_pass https://$arg_vmhost$uri;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_request_buffering off;
            proxy_buffering off;
            proxy_ssl_session_reuse on;
        }
    hosts:
      - host: $DOMAIN
        paths:
          - path: "/vm/connect(/|$(.*)"
            pathType: prefix
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

As in the notes at the top of the Ingress, this is used to abstract the connection from VM API to the vCenter hosts. **This is highly recommended but not required.**

#### VM API Custom Certificates

```yaml
  # If this deployment needs to trust non-public certificates,
  # create a configMap with the needed certifcates and specify
  # the configMap name here
  certificateMap: "appliance-root-ca"
```

Uses a configmap to install custom certificates. The configmap should contain the key with the name of the certificate with the correct extension and the value should be the certificate itself.

#### VM API Environment Variables

```yaml
  # Config app settings with environment vars.
  # Those most likely needing values are listed. For others,
  # see https://github.com/cmu-sei/crucible/blob/master/vm.api/S3.VM.Api/appsettings.json
  env:
    # Proxy Settings
    # http_proxy: proxy.example.com:9000
    # https_proxy: proxy.example.com:9000
    # HTTP_PROXY: proxy.example.com:9000
    # HTTPS_PROXY: proxy.example.com:9000
    # NO_PROXY: .local
    # no_proxy: .local

    ## If hosting in virtual directory, specify path base
    PathBase: "/vm"

    Logging__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"

    # Connection String to database
    # database requires the 'uuid-ossp' extension installed
    ConnectionStrings__PostgreSQL: "Server=postgresql;Port=5432;Database=vm_api;Username=$POSTGRES_USER;Password=$POSTGRES_PASS;"

    # CORS policy settings.
    # The first entry should be the URL to VM App
    # The second entry should be the URL to Console App
    CorsPolicy__Origins__0: "https://$DOMAIN"
    CorsPolicy__Origins__1: "https://localhost:4303"
    CorsPolicy__AllowAnyMethod: true
    CorsPolicy__AllowAnyHeader: true

    # OAuth2 Identity Client for Application
    Authorization__Authority: https://$DOMAIN/identity
    Authorization__AuthorizationUrl: https://$DOMAIN/identity/connect/authorize
    Authorization__TokenUrl: https://$DOMAIN/identity/connect/token
    Authorization__AuthorizationScope: "vm-api player-api"
    Authorization__ClientId: vm-api-dev
    Authorization__ClientName: "VM API"

    # OAuth2 Identity Client /w Password
    IdentityClient__TokenUrl: https://$DOMAIN/identity/connect/token
    IdentityClient__ClientId: "player-vm-admin"
    IdentityClient__Scope: "player-api vm-api"
    IdentityClient__Username: "crucible-admin@$DOMAIN"
    IdentityClient__Password: "$CRUCIBLE_ADMIN_PASS" 

    # Crucible Player URL
    ClientSettings__urls__playerApi: "https://$DOMAIN/player/api"

    # VCenter settings
    #
    # A privileged vCenter used is required to read and write files
    #
    # A datastore needs to be created for Player to store files.  This is
    # typically an NFS share in the format:  <DATASTORE>/player/
    #
    # - DsName denotes the DataStore name
    # - BaseFolder is the folder inside the DataStore to use
    Vsphere__Host: "$VSPHERE_SERVER"
    Vsphere__Username: "$VSPHERE_USER"
    Vsphere__Password: "$VSPHERE_PASS"
    Vsphere__DsName: "$VSPHERE_DATASTORE"
    Vsphere__BaseFolder: "/player"

    # Rewrite Host settings
    # See "consoleIngress" section above for usage
    RewriteHost__RewriteHost: true
    RewriteHost__RewriteHostUrl: "$DOMAIN/vm/connect"
    RewriteHost__RewriteHostQueryParam: "vmhost"
```

Above are the environment variables from the example values file and are all recommended and customizable to your configuration. These settings also match with the Identity example located in this documentation.

There is one setting not listed in the values yaml:

```yaml
  IsoUpload__BasePath: '/app/isos/player'
  IsoUpload_MaxFileSize: 6000000000
```

Just wanted to note, similar to Player API upload max file size, this setting has to also reflect in the body size setting on the Ingress.

There are additional settings located [here](https://github.com/cmu-sei/helm-charts/blob/main/charts/player/charts/vm-api/values.yaml) at the Helm chart but the defaults are recommended.

### VM UI

#### VM UI Ingress

```yaml
  ingress:
    enabled: true  
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: "/vm(/|$)(.*)"
            pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### VM UI Environment Variables and Settings

```yaml
  env: 
    ## basehref is path to the app
    APP_BASEHREF: "/vm"

  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client
  settings: |-
    {
      "ApiUrl": "https://$DOMAIN/vm/api",
      "DeployApiUrl": "",
      "ApiPlayerUrl": "https://$DOMAIN/player/api",
      "WelderUrl": "",
      "UserFollowUrl": "https://$DOMAIN/vm/console/user/{userId}/view/{viewId}/console",
      "OIDCSettings": {
          "authority": "https://$DOMAIN/identity",
          "client_id": "vm-ui",
          "redirect_uri": "https://$DOMAIN/vm/auth-callback",
          "post_logout_redirect_uri": "https://$DOMAIN/vm",
          "response_type": "code",
          "scope": "openid profile player-api vm-api",
          "automaticSilentRenew": true,
          "silent_redirect_uri": "https://$DOMAIN/vm/auth-callback-silent"
      },
      "UseLocalAuthStorage": true
    }
```

Similar to Player UI. Additional settings listed [here](https://github.com/cmu-sei/Vm.Ui/blob/development/src/assets/config/settings.json) but defaults from this example are recommended and customizable to your configuration.

### Console UI

#### Console UI Ingress

```yaml
  ingress:
    enabled: true  
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: "/console(/|$)(.*)"
            pathType: Prefix
    tls:
      - secretName: ""
        hosts:
         - $DOMAIN
```

#### Console Environment Variables and Settings

```yaml
  env: 
    ## basehref is path to the app
    APP_BASEHREF: "/console"

  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client and some basic settings
  settings: |-
    {
      "ConsoleApiUrl": "https://$DOMAIN/vm/api",
      "OIDCSettings": {
        "authority": "https://$DOMAIN/identity",
        "client_id": "vm-console-ui",
        "redirect_uri": "https://$DOMAIN/console/auth-callback",
        "post_logout_redirect_uri": "https://$DOMAIN/console",
        "response_type": "code",
        "scope": "openid profile player-api vm-api",
        "automaticSilentRenew": true,
        "silent_redirect_uri": "https://$DOMAIN/console/auth-callback-silent"
      },
      "UseLocalAuthStorage": true,
      "VmResolutionOptions": [
        { "width": 2560, "height": 1600 },
        { "width": 1920, "height": 1440 },
        { "width": 1920, "height": 1200 },
        { "width": 1600, "height": 1200 },
        { "width": 1400, "height": 1050 },
        { "width": 1280, "height": 1024 },
        { "width": 1440, "height": 900 },
        { "width": 1280, "height": 960 },
        { "width": 1366, "height": 768 },
        { "width": 1280, "height": 800 },
        { "width": 1280, "height": 720 },
        { "width": 1024, "height": 768 },
        { "width": 800, "height": 600 }
      ]
    }
```

Additional settings [here](https://github.com/cmu-sei/Console.Ui/blob/development/src/assets/config/settings.json).

## Alloy

### Resources on Alloy

* [Alloy Helm Chart](https://github.com/cmu-sei/helm-charts/tree/main/charts/alloy)
* [Alloy API](https://github.com/cmu-sei/Alloy.Api)
* [Alloy UI](https://github.com/cmu-sei/Alloy.Ui)

### Alloy API

#### Alloy API Ingress

```yaml
  ingress:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: 'true'
    hosts:
      - host: $DOMAIN
        paths:
          - path: /alloy/(api|swagger/hubs)
            pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
          - $DOMAIN
```

#### Alloy API Custom Certificates

```yaml
  # If this deployment needs to trust non-public certificates,
  # create a configMap with the needed certificates and specify
  # the configMap name here
  certificateMap: 'appliance-root-ca'
```

#### Alloy API Environment Variables

```yaml
  # Config app settings with environment vars.
  # Those most likely needing values are listed. For others,
  # see https://github.com/cmu-sei/crucible/blob/master/alloy.api/Alloy.Api/appsettings.json
  env:
    # Proxy Settings
    # http_proxy: proxy.example.com:9000
    # https_proxy: proxy.example.com:9000
    # HTTP_PROXY: proxy.example.com:9000
    # HTTPS_PROXY: proxy.example.com:9000
    # NO_PROXY: .local
    # no_proxy: .local
    
    ## If hosting in virtual directory, specify path base
    PathBase: "/alloy"

    Logging__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"

    # CORS policy settings.
    # The first entry should be the URL to Alloy
    CorsPolicy__Origins__0: https://$DOMAIN
    CorsPolicy__AllowAnyMethod: true
    CorsPolicy__AllowAnyHeader: true

    # Connection String to database
    # database requires the 'uuid-ossp' extension installed
    ConnectionStrings__PostgreSQL: 'Server=postgresql;Port=5432;Database=alloy_api;Username=$POSTGRES_USER;Password=$POSTGRES_PASS;'

    # OAuth2 Identity Client for Application
    Authorization__Authority: https://$DOMAIN/identity
    Authorization__AuthorizationUrl: https://$DOMAIN/identity/connect/authorize
    Authorization__TokenUrl: https://$DOMAIN/identity/connect/token
    Authorization__AuthorizationScope: 'alloy-api player-api caster-api steamfitter-api vm-api'
    Authorization__ClientId: alloy-api-dev
    Authorization__ClientName: 'Alloy API'

    # OAuth2 Identity Client /w Password
    ResourceOwnerAuthorization__Authority: https://$DOMAIN/identity
    ResourceOwnerAuthorization__ClientId: alloy-api
    ResourceOwnerAuthorization__UserName: crucible-admin@$DOMAIN
    ResourceOwnerAuthorization__Password: $GLOBAL_ADMIN_PASS
    ResourceOwnerAuthorization__Scope: 'alloy-api player-api caster-api steamfitter-api vm-api'

    # Crucible Application URLs
    ClientSettings__urls__playerApi: https://$DOMAIN/player/api
    ClientSettings__urls__casterApi: https://$DOMAIN/caster/api
    ClientSettings__urls__steamfitterApi: https://$DOMAIN/steamfitter/api
```

Additional settings [here](https://github.com/cmu-sei/helm-charts/blob/main/charts/alloy/charts/alloy-api/values.yaml) at the Helm chart, similar to [appsettings.json](https://github.com/cmu-sei/Alloy.Api/blob/development/Alloy.Api/appsettings.json) on GitHub. Above are the recommended defaults but are configurable to your installation.

### Alloy UI

#### Alloy UI Ingress

```yaml
  # Ingress configuration example for NGINX
  # TLS and Host URLs need configured
  ingress:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: "/alloy(/|$)(.*)"
            pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
          - $DOMAIN
```

#### Alloy UI Environment Variables and Settings

```yaml
  env: 
    ## basehref is path to the app
    APP_BASEHREF: "/alloy"

  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client and
  # some basic configuration
  # NOTE:  PlayerUIAddress is the URL to the Crucible - Player application
  settings: |-
    {
      "ApiUrl": "https://$DOMAIN/alloy",
      "OIDCSettings": {
        "authority": "https://$DOMAIN/identity/",
        "client_id": "alloy-ui",
        "redirect_uri": "https://$DOMAIN/alloy/auth-callback",
        "post_logout_redirect_uri": "https://$DOMAIN/alloy",
        "response_type": "code",
        "scope": "openid profile alloy-api player-api caster-api steamfitter-api vm-api",
        "automaticSilentRenew": true,
        "silent_redirect_uri": "https://$DOMAIN/alloy/auth-callback-silent"
      },
      "AppTitle": "Alloy",
      "AppTopBarText": "Alloy",
      "AppTopBarHexColor": "#719F94",
      "AppTopBarHexTextColor": "#FFFFFF",
      "PlayerUIAddress": "https://$DOMAIN/player",
      "PollingIntervalMS": "3500",
      "UseLocalAuthStorage": true
    }
```

Similar to the other UI applications, [settings.json](https://github.com/cmu-sei/Alloy.Ui/blob/development/src/assets/config/settings.json) is customizable to your configuration. These defaults are recommended.

## Caster

### Caster API

#### Caster API Ingress

```yaml
  ingress:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: /caster/(api|swagger|hubs)
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### Caster API and Terraform

Terraform is installed on each deployment of Caster API. Here's the script that installs Terraform:

```bash
{{- define "terraform-installation" }}
# Skip installation
if [ "${SKIP_TERRAFORM_INSTALLATION,,}" == "true" ]; then
    exit 0
fi

if [ ! -d $Terraform__BinaryPath ]; then

    # Install Unzip
    apt-get update && apt-get install -y unzip wget

    # Create Terraform directories
    mkdir -p "$Terraform__RootWorkingDirectory"
    mkdir -p "$Terraform__PluginDirectory"
    mkdir -p "$Terraform__BinaryPath"
    mkdir -p "$Terraform__PluginCache"

    # Get current Terraform version
    TERRAFORM_VERSION=$(curl -s https://api.github.com/repos/hashicorp/terraform/releases/latest | jq -r .name)

    # Make Terraform version directory
    mkdir -p "$Terraform__BinaryPath/${TERRAFORM_VERSION:1}"
    mkdir -p "$Terraform__BinaryPath/${Terraform__DefaultVersion}"

    # Download and Unzip Terraform Latest
    cd "$Terraform__BinaryPath/${TERRAFORM_VERSION:1}"
    curl -s -O "https://releases.hashicorp.com/terraform/${TERRAFORM_VERSION:1}/terraform_${TERRAFORM_VERSION:1}_linux_amd64.zip"
    unzip "terraform_${TERRAFORM_VERSION:1}_linux_amd64.zip"
    rm "terraform_${TERRAFORM_VERSION:1}_linux_amd64.zip"

    # Download and Unzip Terraform Default Version
    cd "$Terraform__BinaryPath/${Terraform__DefaultVersion}"
    curl -s -O "https://releases.hashicorp.com/terraform/${Terraform__DefaultVersion}/terraform_${Terraform__DefaultVersion}_linux_amd64.zip"
    unzip "terraform_${Terraform__DefaultVersion}_linux_amd64.zip"
    rm "terraform_${Terraform__DefaultVersion}_linux_amd64.zip"
else
    echo "Terraform already installed."
fi

{{- end }}
```

This is located in the Helm chart [here](https://github.com/cmu-sei/helm-charts/blob/main/charts/caster/charts/caster-api/templates/terraform-installation.tpl). **Please note that Internet connectivity is required if you are installing Terraform on Caster this way.**

If this is being installed on infrastructure that doesn't have Internet, you will have to download and move Terraform manually to Caster along with your plugins and providers. If this is the case, please set the environment variable `SKIP_TERRAFORM_INSTALLATION` to `true` to skip the Terraform install.

```yaml
  # Use a .terraformrc file to overwrite standard Terraform configuration
  # https://www.terraform.io/docs/cli/config/config-file.html
  # NOTE:  If enabled,  Terraform__PluginDirectory environment variable must be set to empty explicitly
  terraformrc:
    enabled: true
    value: |
      plugin_cache_dir = "/terraform/plugin-cache"
```

Above is an example `terraformrc` file being imported into the pod. This is customizable to your configuration.

#### Caster API Storage

```yaml
  # storage - either an existing pvc, the size for a new pvc, or emptyDir
  storage:
    existing: ""
    size: "2Gi"
    mode: ReadWriteOnce
    class: longhorn
```

In this example, we are using Longhorn. Please change this if you are using an existing Persistent Volume Claim.

#### Caster API Certificates

```yaml
  # If this deployment needs to trust non-public certificates,
  # create a configMap with the needed certificates and specify
  # the configMap name here
  certificateMap: "appliance-root-ca"
```

If you need to install certificates onto the Caster API pod, create a configmap, set the key to the certificate name including the extension and the value to the certificate. Put the name of your newly created configmap above in the values yaml.

#### Caster API Environment Variables

```yaml
  # Config app settings with environment vars.
  # Those most likely needing values are listed. For others,
  # see https://github.com/cmu-sei/crucible/blob/master/caster.api/src/Caster.Api/appsettings.json
  env:
    # Proxy Settings
    # http_proxy: proxy.example.com:9000
    # https_proxy: proxy.example.com:9000
    # HTTP_PROXY: proxy.example.com:9000
    # HTTPS_PROXY: proxy.example.com:9000
    # NO_PROXY: .local
    # no_proxy: .local

    ## If hosting in virtual directory, specify path base
    PathBase: "/caster"

    Logging__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"

    # This deployment comes built in with a script to install Terraform and the necessary
    # plugins to run Caster properly.  Internet access is required for this script to run properly.
    # It's recommended that this should remain false.  Please see the file "terraform-installation.tpl"
    # for more information on the installation process.
    SKIP_TERRAFORM_INSTALLATION: false

    # VSphere settings:
    # TODO - Document VSphere user role requirements
    VSPHERE_SERVER: $VSPHERE_SERVER
    VSPHERE_USER: $VSPHERE_USER
    VSPHERE_PASSWORD: $VSPHERE_PASSWORD
    VSPHERE_ALLOW_UNVERIFIED_SSL: true

    # === Terraform Crucible Provider Section ===
    # These variables only need filled in if you are using the following provider:
    # https://registry.terraform.io/providers/cmu-sei/crucible/latest

    # An Identity Service account with Caster Admin privileges
    SEI_CRUCIBLE_USERNAME: 'crucible-admin@$DOMAIN'
    SEI_CRUCIBLE_PASSWORD: '$CRUCIBLE_ADMIN_PASS'

    # URL to the Identity Server Auth endpoint
    SEI_CRUCIBLE_AUTH_URL: https://$DOMAIN/identity/connect/authorize
    # URL to the Identity Server Token endpoint
    SEI_CRUCIBLE_TOK_URL: https://$DOMAIN/identity/connect/token

    # Identity Client information
    # If you installed the Identity Helm chart, this data was already or can be seeded
    SEI_CRUCIBLE_CLIENT_ID: player-api
    SEI_CRUCIBLE_CLIENT_SECRET: '578bca574cad40ea9d84e44c12426a6c'

    # URLs to Player API and VM API
    SEI_CRUCIBLE_VM_API_URL: https://$DOMAIN/vm/api/
    SEI_CRUCIBLE_PLAYER_API_URL: https://$DOMAIN/player/api

    # === End Terraform Crucible Provider Section ===

    # === Terraform Identity Provider Section ===
    # These variables only need filled in if you are using the following provider:
    # https://registry.terraform.io/providers/cmu-sei/identity/latest

    # URL to the Identity Server Auth endpoint
    SEI_IDENTITY_TOK_URL: https://$DOMAIN/identity/connect/token
    # URL to the Identity Server API endpoint
    SEI_IDENTITY_API_URL: https://$DOMAIN/identity/api/
    
    # Identity Client information
    # If you installed the Identity Helm chart, this data was already or can be seeded
    SEI_IDENTITY_CLIENT_ID: caster-admin  
    SEI_IDENTITY_CLIENT_SECRET: '26fa08a2f77a4ad6ac2b40a9ffe4a735'

    # === End Terraform Identity Provider Section ===

    # === Terraform Azure Provider Section ===
    # These variables only need filled in if you are using the following provider:
    # https://registry.terraform.io/providers/hashicorp/azurerm/latest

    # Remaining documentation provided by the plugin
    # NOTE:  Use the certificateMap key in this chart to add certificates, which will be placed in:
    #        /usr/local/share/ca-certificates
    ARM_CLIENT_CERTIFICATE_PATH: ''
    ARM_CLIENT_ID: ''
    ARM_ENVIRONMENT: ''
    ARM_SKIP_PROVIDER_REGISTRATION: ''
    ARM_SUBSCRIPTION_ID: ''
    ARM_TENANT_ID: ''

    # See here for more information regarding AllowedHosts
    # https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hostfiltering.hostfilteringoptions.allowedhosts?view=aspnetcore-3.1
    AllowedHosts: "*"

    # CORS policy settings.
    # The first entry should be the URL to Caster
    CorsPolicy__Origins__0: "https://$DOMAIN"
    CorsPolicy__AllowAnyMethod: true
    CorsPolicy__AllowAnyHeader: true

    # Connection String to database
    # database requires the 'uuid-ossp' extension installed
    ConnectionStrings__PostgreSQL: "Server=postgresql;Port=5432;Database=caster_api;Username=$POSTGRES_USER;Password=$POSTGRES_PASS;"

    # OAuth2 Identity Client for Application
    Authorization__Authority: https://$DOMAIN/identity
    Authorization__AuthorizationUrl: https://$DOMAIN/identity/connect/authorize
    Authorization__TokenUrl: https://$DOMAIN/identity/connect/token
    Authorization__AuthorizationScope: "caster-api"
    Authorization__ClientId: caster-api-dev

    # OAuth2 Identity Client /w Password
    Client__TokenUrl: https://$DOMAIN/identity/connect/token
    Client__ClientId: caster-admin
    Client__UserName: crucible-admin@$DOMAIN
    Client__Password: $CRUCIBLE_ADMIN_PASS
    Client__Scope: "player-api vm-api"

    # Crucible Player URLs
    Player__VmApiUrl: "https://$DOMAIN/vm/api"
    Player__VmConsoleUrl: "https://$DOMAIN/console/vm/{id}/console"

    # Terraform Information
    # - DefaultVersion - The default version to be used.
    # - GitlabApiUrl - URL to the deployed Gitlab instance
    Terraform__BinaryPath: /terraform/binaries
    Terraform__RootWorkingDirectory: /terraform/root
    Terraform__PluginCache: /terraform/plugin-cache
    Terraform__DefaultVersion: "0.14.0"
    Terraform__GitlabApiUrl: "http://gitlab-webservice-default:8080/api/v4/"
    Terraform__GitlabToken: "wMa2RPQP_ZR3fxc5zQtv"
    Terraform__GitlabGroupId: 4

    # Configurable save lengths for Caster untagged versions
    FileVersions__DaysToSaveAllUntaggedVersions: 7
    FileVersions__DaysToSaveDailyUntaggedVersions: 31

    # Basic seed data to jumpstart deployement
    # The seed data for users that are in Identity.
    # These users were probably created on the creation of your Identity instance. 
    # If not, please use Identity to fill these values.
    SeedData__Users__0__id: "dee684c5-2eaf-401a-915b-d3d4320fe5d5"
    SeedData__Users__0__name:  "administrator@$DOMAIN"
    SeedData__Users__1__id:  "32c11441-7eec-47eb-a915-607c4f2529f4"
    SeedData__Users__1__name:  "crucible-admin@$DOMAIN"
    SeedData__UserPermissions__0__UserId:  "dee684c5-2eaf-401a-915b-d3d4320fe5d5"
    SeedData__UserPermissions__0__PermissionId:  "00000000-0000-0000-0000-000000000001"
    SeedData__UserPermissions__1__UserId:  "32c11441-7eec-47eb-a915-607c4f2529f4"
    SeedData__UserPermissions__1__PermissionId:  "00000000-0000-0000-0000-000000000001"
```

Above is the example environment variables in the values yaml file for Caster. These settings that are set are recommended but customizable to your configuration. There are additional settings in the Helm chart located [here](https://github.com/cmu-sei/helm-charts/blob/main/charts/caster/charts/caster-api/values.yaml) and the [appsettings.json](https://github.com/cmu-sei/Caster.Api/blob/development/src/Caster.Api/appsettings.json) file from GitHub shows additional settings.

**Caster API also pairs with Gitlab. Please install Gitlab and make sure the above settings match your Gitlab instance. These values can change.**

### Caster UI

#### Caster UI Ingress

```yaml
  # Ingress configuration example for NGINX
  # TLS and Host URLs need configured
  ingress:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: "/caster(/|$)(.*)"
            pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### Caster UI Environment Variables and Settings

```yaml
  env: 
    ## basehref is path to the app
    APP_BASEHREF: "/caster"

  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client
  settings: |-
    {
      "ApiUrl": "https://$DOMAIN/caster",
      "OIDCSettings": {
        "authority": "https://$DOMAIN/identity/",
        "client_id": "caster-ui",
        "redirect_uri": "https://$DOMAIN/caster/auth-callback",
        "post_logout_redirect_uri": "https://$DOMAIN/caster",
        "response_type": "code",
        "scope": "openid profile email caster-api",
        "automaticSilentRenew": true,
        "silent_redirect_uri": "https://$DOMAIN/caster/auth-callback-silent"
      },
      "UseLocalAuthStorage": true,
      "AppTopBarHexColor": "#E9831C",
      "AppTopBarHexTextColor": "#FFFFFF",
      "AppTopBarText": "Caster",
      "Hotkeys": {
        "PROJECT_NEW": {
          "keys": "meta.p",
          "group": "",
          "description": "New Project"
        },
        "ENTER": {
          "keys": "enter",
          "group": "Global",
          "description": "Default 'confirm'",
          "allowIn": ["INPUT"]
        },
        "ESCAPE": {
          "keys": "escape",
          "group": "Global",
          "description": "Default 'cancel'",
          "allowIn": ["INPUT", "TEXTAREA"]
        },
        "FILE_LOCK_TOGGLE": {
          "keys": "control.l",
          "group": "Editor",
          "description": "Unlock / Lock a file",
          "allowIn": ["INPUT", "TEXTAREA"]
        },
        "FILE_SAVE": {
          "keys": "control.s",
          "group": "Editor",
          "description": "Save a file",
          "allowIn": ["INPUT", "TEXTAREA"]
        }
      }
    }
```

These are the settings from the example values file. [Here](https://github.com/cmu-sei/Caster.Ui/blob/development/src/assets/config/settings.json) is the location of the settings.json file on Caster UI's GitHub repository.

These settings are recommended but are customizable to your configurations.

## Steamfitter

### Stackstorm

### Steamfitter API

#### Steamfitter API Ingress

```yaml
  ingress:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: /steamfitter/(api|swagger|hubs)
            pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### Steamfitter API Certificates

```yaml
  # If this deployment needs to trust non-public certificates,
  # create a configMap with the needed certificates and specify
  # the configMap name here
  certificateMap: "appliance-root-ca"
```

#### Steamfitter API Environment Variables

```yaml
  # Config app settings with environment vars.
  # Those most likely needing values are listed. For others,
  # see https://github.com/cmu-sei/crucible/blob/master/steamfitter.api/Steamfitter.Api/appsettings.json
  env:
    # Proxy Settings
    # https_proxy: proxy.example.com:9000
    # http_proxy: proxy.example.com:9000
    # HTTP_PROXY: proxy.example.com:9000
    # HTTPS_PROXY: proxy.example.com:9000
    # NO_PROXY: .local
    # no_proxy: .local

    ## If hosting in virtual directory, specify path base
    PathBase: "/steamfitter"

    Logging__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"

    # Connection String to database
    # database requires the 'uuid-ossp' extension installed
    ConnectionStrings__PostgreSQL: "Server=postgresql;Port=5432;Database=steamfitter_api;Username=$POSTGRES_USERpostgres;Password=$POSTGRES_PASS;"

    # CORS policy settings.
    # The first entry should be the URL to Steamfitter
    CorsPolicy__Origins__0: https://$DOMAIN

    # OAuth2 Identity Client for Application
    Authorization__Authority: https://$DOMAIN/identity
    Authorization__AuthorizationUrl: https://$DOMAIN/identity/connect/authorize
    Authorization__TokenUrl: https://$DOMAIN/identity/connect/token
    Authorization__AuthorizationScope: "player-api steamfitter-api vm-api"
    Authorization__ClientId: steamfitter-api-dev
    Authorization__ClientName: "Steamfitter API"

    # OAuth2 Identity Client /w Password
    ResourceOwnerAuthorization__Authority: https://$DOMAIN/identity
    ResourceOwnerAuthorization__ClientId: steamfitter-api
    ResourceOwnerAuthorization__UserName: crucible-admin@$DOMAIN
    ResourceOwnerAuthorization__Password: $CRUCIBLE_ADMIN_PASS
    ResourceOwnerAuthorization__Scope: "vm-api"

    # Crucible URLs
    ClientSettings__urls__playerApi: https://$DOMAIN/player/api
    ClientSettings__urls__vmApi: https://$DOMAIN/vm/api

    # Stackstorm Configuration
    # TODO - Document Stackstorm dependencies
    VmTaskProcessing__ApiType: st2
    VmTaskProcessing__ApiUsername: "administrator"
    VmTaskProcessing__ApiPassword: "$GLOBAL_ADMIN_PASS"
    VmTaskProcessing__ApiBaseUrl: "https://$DOMAIN/stackstorm"
    VmTaskProcessing__ApiParameters__clusters: ""

    # Basic seed data to jumpstart deployement
    # TODO - Document Seed data
    SeedData__Users__0__id: "dee684c5-2eaf-401a-915b-d3d4320fe5d5"
    SeedData__Users__0__name:  "administrator@$DOMAIN"
    SeedData__Users__1__id: "32c11441-7eec-47eb-a915-607c4f2529f4"
    SeedData__Users__1__name:  "crucible-admin@$DOMAIN"
    

    SeedData__UserPermissions__0__UserId: "dee684c5-2eaf-401a-915b-d3d4320fe5d5"
    SeedData__UserPermissions__0__PermissionId: "00000000-0000-0000-0000-000000000001"
    SeedData__UserPermissions__1__UserId: "32c11441-7eec-47eb-a915-607c4f2529f4"
    SeedData__UserPermissions__1__PermissionId: "00000000-0000-0000-0000-000000000001"
```

### Steamfitter UI

#### Steamfitter UI Ingress

```yaml
  # Ingress configuration example for NGINX
  # TLS and Host URLs need configured
  ingress:
    enabled: true
    className: ""
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: $DOMAIN
        paths:
          - path: "/steamfitter(/|$)(.*)"
            pathType: Prefix
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### Steamfitter UI Environment Variables and Settings

```yaml
  env: 
    ## basehref is path to the app
    APP_BASEHREF: "/steamfitter"

  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client
  settings: |-
    {
      "ApiUrl": "https://$DOMAIN/steamfitter",
      "VmApiUrl": "https://$DOMAIN/vm",
      "ApiPlayerUrl": "https://$DOMAIN/player",
      "OIDCSettings": {
          "authority": "https://$DOMAIN/identity",
          "client_id": "steamfitter-ui",
          "redirect_uri": "https://$DOMAIN/steamfitter/auth-callback",
          "post_logout_redirect_uri": "https://$DOMAIN/steamfitter",
          "response_type": "code",
          "scope": "openid profile steamfitter-api vm-api player-api",
          "automaticSilentRenew": true,
          "silent_redirect_uri": "https://$DOMAIN/steamfitter/auth-callback-silent"
      },
      "AppTitle": "Steamfitter",
      "AppTopBarHexColor": "#EF3A47",
      "AppTopBarHexTextColor": "#FFFFFF",
      "UseLocalAuthStorage": true
    }
```

## Blueprint

### Blueprint API

#### Blueprint API Ingress

```yaml
# Ingress configuration example for NGINX
# TLS and Host URLs need configured
  ingress:
    enabled: true
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: blueprint.$DOMAIN
        paths:
          - path: /(api|swagger|hubs)
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### Blueprint API Environment Variables

```yaml
  env:
    # CORS policy settings.
    # The first entry should be the URL to Blueprint
    CorsPolicy__Origins__0: https://blueprint.$DOMAIN

    # Connection String to database
    # database requires the 'uuid-ossp' extension installed
    ConnectionStrings__PostgreSQL: "Server=postgresql;Port=5432;Database=blueprint;Username=$POSTGRES_USER;Password=$POSTGRES_PASS;SSL Mode=Prefer;Trust Server Certificate=true;"

    # OAuth2 Identity Client for Application
    Authorization__Authority: https://$DOMAIN/identity
    Authorization__AuthorizationUrl: https://$DOMAIN/identity/connect/authorize
    Authorization__TokenUrl: https://$DOMAIN/identity/connect/token
    Authorization__AuthorizationScope: "blueprint-api cite-api gallery-api player-api vm-api steamfitter-api"
    Authorization__ClientId: blueprint-api
    Authorization__ClientName: "Blueprint API"
    ClientSettings__CiteApiUrl: "https://$DOMAIN/cite"
    ClientSettings__GalleryApiUrl: "https://$DOMAIN/gallery"
    ClientSettings__PlayerApiUrl: "https://$DOMAIN/player"
    ClientSettings__SteamfitterApiUrl: "https://$DOMAIN/steamfitter"

    Logging__Debug__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Debug__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Debug__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__FormatterName: "simple"
```

### Blueprint UI

#### Blueprint UI Ingress

```yaml
  # Ingress configuration example for NGINX
  # TLS and Host URLs need configured
  ingress:
    enabled: true
    annotations:
      kubernetes.io/ingress.class: nginx
    hosts:
      - host: blueprint.$DOMAIN
        paths:
          - path: /
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### Blueprint UI Environment Variables and Settings

```yaml
  env:
  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client and
  # some basic configuration
  settings: |
    {
      "ApiUrl": "https://blueprint.$DOMAIN",
      "OIDCSettings": {
        "authority": "https://$DOMAIN/identity/",
        "client_id": "blueprint-ui",
        "redirect_uri": "https://blueprint.$DOMAIN/auth-callback",
        "post_logout_redirect_uri": "https://blueprint.$DOMAIN",
        "response_type": "code",
        "scope": "openid profile blueprint-api cite-api gallery-api player-api vm-api steamfitter-api",
        "automaticSilentRenew": true,
        "silent_redirect_uri": "https://blueprint.$DOMAIN/auth-callback-silent"
      },
      "AppTitle": "Blueprint",
      "AppTopBarHexColor": "#2d69b4",
      "AppTopBarHexTextColor": "#FFFFFF",
      "AppTopBarText": "Blueprint  -  Collaborative MSEL Creation",
      "AppTopBarImage": "/assets/img/pencil-ruler-white.png",
      "UseLocalAuthStorage": false
    }
```

These settings are recommended but can be customizable to your configuration.

## Gallery

### Gallery API

#### Gallery API Ingress

```yaml
# Ingress configuration example for NGINX
# TLS and Host URLs need configured
  ingress:
    enabled: true
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: "true"
    hosts:
      - host: gallery.$DOMAIN
        paths:
          - path: /(api|swagger|hubs)
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### Gallery API Environment Variables

```yaml
  env:
    # CORS policy settings.
    # The first entry should be the URL to Gallery
    CorsPolicy__Origins__0: https://gallery.$DOMAIN
    CorsPolicy__Origins__1: https://cite.$DOMAIN

    # Connection String to database
    # database requires the 'uuid-ossp' extension installed
    ConnectionStrings__PostgreSQL: "Server=postgresql;Port=5432;Database=gallery;Username=$POSTGRES_USER;Password=$POSTGRES_PASS;SSL Mode=Prefer;Trust Server Certificate=true;"
    # OAuth2 Identity Client for Application
    Authorization__Authority: https://$DOMAIN/identity
    Authorization__AuthorizationUrl: https://$DOMAIN/identity/connect/authorize
    Authorization__TokenUrl: https://$DOMAIN/identity/connect/token
    Authorization__AuthorizationScope: "gallery-api"
    Authorization__ClientId: gallery-api
    Authorization__ClientName: "Gallery API"

    # OAuth2 Identity Client /w Password
    ResourceOwnerAuthorization__Authority: https://$DOMAIN/identity
    ResourceOwnerAuthorization__ClientId: gallery-admin
    ResourceOwnerAuthorization__UserName: "administrator@$DOMAIN"
    ResourceOwnerAuthorization__Password: "$GLOBAL_ADMIN_PASS"
    ResourceOwnerAuthorization__Scope: "steamfitter-api player-api vm-api"

    ClientSettings__SteamfitterApiUrl: https://$DOMAIN/steamfitter
    ClientSettings__IsEmailActive: "false"

    Logging__Debug__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Debug__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Debug__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__FormatterName: "simple"
```

These settings are recommended but customizable to your configuration. The `appsettings.json` for the application is located [here](https://github.com/cmu-sei/Gallery.Api/blob/development/Gallery.Api/appsettings.json).

### Gallery UI

#### Gallery UI Ingress

```yaml
  # Ingress configuration example for NGINX
  # TLS and Host URLs need configured
  ingress:
    enabled: true
    annotations:
      kubernetes.io/ingress.class: nginx
    hosts:
      - host: gallery.$DOMAIN
        paths:
          - path: /
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
         - $DOMAIN
```

#### Gallery UI Environment Variables and Settings

```yaml
  env:
  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client and
  # some basic configuration
  settings: |
    {
      "ApiUrl": "https://gallery.$DOMAIN",
      "OIDCSettings": {
        "authority": "https://$DOMAIN/identity/",
        "client_id": "gallery-ui",
        "redirect_uri": "https://gallery.$DOMAIN/auth-callback",
        "post_logout_redirect_uri": "https://gallery.$DOMAIN",
        "response_type": "code",
        "scope": "openid profile gallery-api",
        "automaticSilentRenew": true,
        "silent_redirect_uri": "https://gallery.$DOMAIN/auth-callback-silent"
      },
      "AppTitle": "Gallery",
      "AppTopBarHexColor": "#2d69b4",
      "AppTopBarHexTextColor": "#FFFFFF",
      "AppTopBarText": "Gallery  -  Keeping you in the know!",
      "AppTopBarImage": "/assets/img/monitor-dashboard-white.png",
      "UseLocalAuthStorage": false,
      "IsEmailActive": false
    }
```

These are the recommended settings but can be customized to your configuration. The settings.json file is located [here](https://github.com/cmu-sei/Gallery.Ui/blob/development/src/assets/config/settings.json) on the GitHub repository.

## CITE

### CITE API

#### CITE API Ingress

```yaml
  # Ingress configuration example for NGINX
  # TLS and Host URLs need configured
  ingress:
    enabled: true
    annotations:
      kubernetes.io/ingress.class: nginx
      nginx.ingress.kubernetes.io/proxy-read-timeout: '86400'
      nginx.ingress.kubernetes.io/proxy-send-timeout: '86400'
      nginx.ingress.kubernetes.io/use-regex: 'true'
    hosts:
      - host: cite.$DOMAIN
        paths:
          - path: /(api|swagger|hubs)
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
          - $DOMAIN
```

#### CITE Environment Variables

```yaml
  env:
    # CORS policy settings.
    # The first entry should be the URL to CITE
    CorsPolicy__Origins__0: https://cite.$DOMAIN
    CorsPolicy__Origins__1: https://gallery.$DOMAIN

    # Connection String to database
    ConnectionStrings__PostgreSQL: "Server=postgresql;Port=5432;Database=cite;Username=$POSTGRES_USER;Password=$POSTGRES_PASS;SSL Mode=Prefer;Trust Server Certificate=true;"
    Database__Provider: PostgreSQL
    Database__SeedFile: conf/seed.json
    Database__AutoMigrate: true
    Database__DevModeRecreate: false
    Database__OfficialScoreTeamTypeName: "Official Score Contributor"

    # OAuth2 Identity Client for Application
    Authorization__Authority: https://$DOMAIN/identity
    Authorization__AuthorizationUrl: https://$DOMAIN/identity/connect/authorize
    Authorization__TokenUrl: https://$DOMAIN/identity/connect/token
    Authorization__AuthorizationScope: 'cite-api'
    Authorization__ClientId: cite-api
    Authorization__ClientName: 'CITE API'

    # OAuth2 Identity Client /w Password
    ResourceOwnerAuthorization__Authority: https://$DOMAIN/identity
    ResourceOwnerAuthorization__ClientId: cite-admin
    ResourceOwnerAuthorization__UserName: "administrator@$DOMAIN"
    ResourceOwnerAuthorization__Password: "$GLOBAL_ADMIN_PASS"
    ResourceOwnerAuthorization__Scope: "gallery-api"

    ClientSettings__GalleryApiUrl: https://gallery.$DOMAIN/api

    Logging__Debug__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Debug__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Debug__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__System: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Default: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__LogLevel__Microsoft: "$CRUCIBLE_LOG_LEVEL"
    Logging__Console__FormatterName: "simple"
```

These are the recommended settings but can be customized to your configuration. The `appsettings.json` is located [here](https://github.com/cmu-sei/CITE.Api/blob/development/Cite.Api/appsettings.json) on the GitHub repository.

#### CITE API Seed Data

<details markdown="1">

<summary> CITE Seed Data </summary>
```yaml
  conf:
    seed: |
        {
          "Permissions": [
            {
              "Id": "2f82cb36-4c0a-4b60-857f-c4f2a9e70817",
              "Key": "SystemAdmin",
              "Value": "true",
              "Description": "Has Full Rights.  Can do everything.",
              "ReadOnly": true
            },
            {
              "Id": "c881417e-02f1-4232-b06b-723901120e20",
              "Key": "ContentDeveloper",
              "Value": "true",
              "Description": "Can create/edit/delete an Exercise/Directory/Workspace/File/Module",
              "ReadOnly": true
            },
            {
              "Id": "e0317506-fc9a-412d-a4bf-ac5ec915490e",
              "Key": "CanSubmit",
              "Value": "true",
              "Description": "Can submit a score",
              "ReadOnly": true
            },
            {
              "Id": "d1311f27-816d-4853-a092-5f888fa05742",
              "Key": "CanModify",
              "Value": "true",
              "Description": "Can modify a submission.",
              "ReadOnly": true
            },
            {
              "Id": "382dfc33-6fc2-4e3a-a03d-ac37aef4cde1",
              "Key": "CanIncrementMove",
              "Value": "true",
              "Description": "Can increment the current evaluation move",
              "ReadOnly": true
            }
          ],
          "Users": [
            {
              "Id": "dee684c5-2eaf-401a-915b-d3d4320fe5d5",
              "Name": "Admin"
            }
          ],
          "UserPermissions": [
            {
              "UserId": "dee684c5-2eaf-401a-915b-d3d4320fe5d5",
              "PermissionId": "2f82cb36-4c0a-4b60-857f-c4f2a9e70817"
            }
          ],
          "TeamTypes": [
            {
              "Id": "28c9d58d-b273-48b1-a1f5-ac1048c55810",
              "Name": "Official Score Contributor"
            },
            {
              "Id": "e5a06ded-8895-47e7-84d7-52c4cc0487a8",
              "Name": "Individual Organization"
            },
            {
              "Id": "680bc3a8-9267-4c75-a0a2-ddbe8505921c",
              "Name": "Other"
            }
          ],
          "ScoringModels": [
            {
              "Id": "121c225d-796a-448c-a5cd-95c7a9436d51",
              "Description": "CISA NCISS",
              "Status": 20,
              "CalculationEquation": "100.0 > 100.0 * ({sum} - {minPossible}) / ({maxPossible} - {minPossible}) > 0.0"
            }
          ],
          "ScoringCategories": [
            {
              "Id": "fd802f0e-8953-4616-90e5-9011b66855c6",
              "DisplayOrder": 1,
              "Description": "Functional Impact",
              "AllowMultipleChoices": false,
              "CalculationEquation": "{max}",
              "IsModifierRequired": false,
              "ScoringWeight": 6,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51"
            },
            {
              "Id": "214e19ea-592f-43a5-9f78-15e2b9a4b98c",
              "DisplayOrder": 2,
              "Description": "Observed Activity",
              "AllowMultipleChoices": false,
              "CalculationEquation": "{max}",
              "IsModifierRequired": false,
              "ScoringWeight": 5,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51"
            },
            {
              "Id": "228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "DisplayOrder": 3,
              "Description": "Location of Observed Activity",
              "AllowMultipleChoices": false,
              "CalculationEquation": "{max}",
              "IsModifierRequired": false,
              "ScoringWeight": 4,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51"
            },
            {
              "Id": "b0613639-520a-4451-8f39-a6adaab36428",
              "DisplayOrder": 4,
              "Description": "Actor Characterization",
              "AllowMultipleChoices": false,
              "CalculationEquation": "{max}",
              "IsModifierRequired": false,
              "ScoringWeight": 4,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51"
            },
            {
              "Id": "142a9bdd-f814-4965-88a4-0975957909be",
              "DisplayOrder": 5,
              "Description": "Information Impact",
              "AllowMultipleChoices": false,
              "CalculationEquation": "{max}",
              "IsModifierRequired": false,
              "ScoringWeight": 2,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51"
            },
            {
              "Id": "efc30a53-f4cc-453b-9fc5-de0f4e039030",
              "DisplayOrder": 6,
              "Description": "Recoverability",
              "AllowMultipleChoices": false,
              "CalculationEquation": "{max}",
              "IsModifierRequired": false,
              "ScoringWeight": 4,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51"
            },
            {
              "Id": "576a66d6-04c7-4367-90f6-f58536a073b6",
              "DisplayOrder": 7,
              "Description": "Cross Sector Dependency",
              "AllowMultipleChoices": false,
              "CalculationEquation": "{max}",
              "IsModifierRequired": false,
              "ScoringWeight": 3,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51"
            },
            {
              "Id": "e6ca79b0-c2f1-4dd8-8ec8-7128866403db",
              "DisplayOrder": 8,
              "Description": "Potential Impact",
              "AllowMultipleChoices": false,
              "CalculationEquation": "{max}",
              "IsModifierRequired": false,
              "ScoringWeight": 6,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51"
            }
          ],
          "ScoringOptions": [
            {
              "Id":"405ba324-cc5c-42f6-b32d-e2b33a7b18c6",
              "DisplayOrder":7,
              "Description":"Core Credential Compromise",
              "Value":80,
              "ScoringCategoryId":"142a9bdd-f814-4965-88a4-0975957909be",
              "IsModifier":false
            },
            {
              "Id":"4283bec4-424a-4ef4-bf00-00c60beaae3a",
              "DisplayOrder":1,
              "Description":"No Impact",
              "Value":0,
              "ScoringCategoryId":"142a9bdd-f814-4965-88a4-0975957909be",
              "IsModifier":false
            },
            {
              "Id":"17ec6dce-ac94-4df8-b5c0-c73a9fd79c34",
              "DisplayOrder":2,
              "Description":"Suspected But Not Identified",
              "Value":10,
              "ScoringCategoryId":"142a9bdd-f814-4965-88a4-0975957909be",
              "IsModifier":false
            },
            {
              "Id":"69672674-8e1a-462e-8be5-9e978f2b11d8",
              "DisplayOrder":3,
              "Description":"Privacy Data Loss",
              "Value":20,
              "ScoringCategoryId":"142a9bdd-f814-4965-88a4-0975957909be",
              "IsModifier":false
            },
            {
              "Id":"5324faeb-1c7c-4faf-8cef-3c64b6cf54bf",
              "DisplayOrder":4,
              "Description":"Proprietory Information Loss",
              "Value":50,
              "ScoringCategoryId":"142a9bdd-f814-4965-88a4-0975957909be",
              "IsModifier":false
            },
            {
              "Id":"93e92f2c-400d-46e2-9e95-78806f4e685f",
              "DisplayOrder":5,
              "Description":"Destruction of Non-Critical System",
              "Value":60,
              "ScoringCategoryId":"142a9bdd-f814-4965-88a4-0975957909be",
              "IsModifier":false
            },
            {
              "Id":"fd57f735-d3cf-4e01-93e2-e9df6aa504d0",
              "DisplayOrder":6,
              "Description":"Critical Systems Data Breach",
              "Value":70,
              "ScoringCategoryId":"142a9bdd-f814-4965-88a4-0975957909be",
              "IsModifier":false
            },
            {
              "Id":"f77fa091-a5be-478e-8ffc-cc38fa604696",
              "DisplayOrder":8,
              "Description":"Destruction of Critical System",
              "Value":100,
              "ScoringCategoryId":"142a9bdd-f814-4965-88a4-0975957909be",
              "IsModifier":false
            },
            {
              "Id":"84466ff8-32b6-4762-8196-45d4b0a471eb",
              "DisplayOrder":1,
              "Description":"Regular",
              "Value":20,
              "ScoringCategoryId":"efc30a53-f4cc-453b-9fc5-de0f4e039030",
              "IsModifier":false
            },
            {
              "Id":"89186330-53f7-4bf4-92df-48affea7c2ac",
              "DisplayOrder":2,
              "Description":"Supplemented",
              "Value":40,
              "ScoringCategoryId":"efc30a53-f4cc-453b-9fc5-de0f4e039030",
              "IsModifier":false
            },
            {
              "Id":"e8f34a17-a3d6-4362-9896-431b206f9967",
              "DisplayOrder":3,
              "Description":"Extended",
              "Value":60,
              "ScoringCategoryId":"efc30a53-f4cc-453b-9fc5-de0f4e039030",
              "IsModifier":false
            },
            {
              "Id":"6c4ea021-75da-4563-aa24-72c6a01e40f7",
              "DisplayOrder":4,
              "Description":"Not Recoverable",
              "Value":100,
              "ScoringCategoryId":"efc30a53-f4cc-453b-9fc5-de0f4e039030",
              "IsModifier":false
            },
            {
              "Id":"0fe24883-6293-4888-80b0-7c95cb1d61b6",
              "DisplayOrder":1,
              "Description":"Agriculture and Food",
              "Value":20,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"8c1bf36c-43cb-4758-9510-9a3560fa1439",
              "DisplayOrder":2,
              "Description":"Banking and Finance",
              "Value":35,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"179070c2-7ffb-4f85-a5fe-c8ea5a4078fc",
              "DisplayOrder":3,
              "Description":"Chemical",
              "Value":20,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"807473de-031e-43c5-8372-d5ff75b00ea0",
              "DisplayOrder":4,
              "Description":"Commercial Facilities",
              "Value":30,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"fc7610e4-cb00-4c98-b146-8bc1c7949c00",
              "DisplayOrder":5,
              "Description":"Communications",
              "Value":75,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"30615aed-8cee-4b28-b699-39b64ccd8ac8",
              "DisplayOrder":6,
              "Description":"Critical Manufacturing",
              "Value":50,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"15cb980e-241e-4225-8cd6-233857e0db9b",
              "DisplayOrder":7,
              "Description":"Dams",
              "Value":25,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"3982529b-12d6-4625-b3c8-ba9a78a1ae48",
              "DisplayOrder":8,
              "Description":"Defense Industrial Base",
              "Value":35,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"2432edc9-7435-4470-9576-39494eadc3ab",
              "DisplayOrder":9,
              "Description":"Emergency Services",
              "Value":25,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"cc3001b6-5801-4784-9caf-d71eb2e86a48",
              "DisplayOrder":10,
              "Description":"Energy",
              "Value":100,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"86125a17-5964-4807-b8af-9074c4c5770a",
              "DisplayOrder":11,
              "Description":"Government Facilities",
              "Value":40,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"d2ab232b-8514-4d85-8b4e-366edaa95c73",
              "DisplayOrder":12,
              "Description":"Healthcare and Public Health",
              "Value":30,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"c25cf1a8-eae8-4fea-a8d1-108e0c1024b6",
              "DisplayOrder":13,
              "Description":"Information Technology",
              "Value":80,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"c3476d27-5bf3-4fc8-9d48-f4715c44caac",
              "DisplayOrder":14,
              "Description":"Nuclear Reactors, Materials and Waste",
              "Value":15,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"2804c4cc-a513-4ed5-a85c-313ed839ab77",
              "DisplayOrder":15,
              "Description":"Transportation Systems",
              "Value":75,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"637d084c-a982-4add-87e1-f7cdd48debfc",
              "DisplayOrder":16,
              "Description":"Water",
              "Value":60,
              "ScoringCategoryId":"576a66d6-04c7-4367-90f6-f58536a073b6",
              "IsModifier":false
            },
            {
              "Id":"4426b287-4cb7-4961-8a23-1f330f1c717a",
              "DisplayOrder":1,
              "Description":"Minimal",
              "Value":0,
              "ScoringCategoryId":"e6ca79b0-c2f1-4dd8-8ec8-7128866403db",
              "IsModifier":false
            },
            {
              "Id":"aa241610-f88d-4f67-99c6-0ce0c0f00f5d",
              "DisplayOrder":2,
              "Description":"Low",
              "Value":25,
              "ScoringCategoryId":"e6ca79b0-c2f1-4dd8-8ec8-7128866403db",
              "IsModifier":false
            },
            {
              "Id":"a14c873c-657c-4266-87c8-dde9671564b9",
              "DisplayOrder":3,
              "Description":"Moderate",
              "Value":50,
              "ScoringCategoryId":"e6ca79b0-c2f1-4dd8-8ec8-7128866403db",
              "IsModifier":false
            },
            {
              "Id":"87b679ce-6a9f-4af7-94ce-27bac3dd0981",
              "DisplayOrder":4,
              "Description":"High",
              "Value":75,
              "ScoringCategoryId":"e6ca79b0-c2f1-4dd8-8ec8-7128866403db",
              "IsModifier":false
            },
            {
              "Id":"3da64169-22a0-4ebe-a746-097e45a080c3",
              "DisplayOrder":5,
              "Description":"Severe",
              "Value":100,
              "ScoringCategoryId":"e6ca79b0-c2f1-4dd8-8ec8-7128866403db",
              "IsModifier":false
            },
            {
              "Id":"543c86c3-2966-4e8a-a835-b1c5d38ca994",
              "DisplayOrder":1,
              "Description":"No Impact",
              "Value":0,
              "ScoringCategoryId":"fd802f0e-8953-4616-90e5-9011b66855c6",
              "IsModifier":false
            },
            {
              "Id":"714c3a30-3bb8-43a9-8dac-e149725dabd1",
              "DisplayOrder":2,
              "Description":"No Impact to Services",
              "Value":20,
              "ScoringCategoryId":"fd802f0e-8953-4616-90e5-9011b66855c6",
              "IsModifier":false
            },
            {
              "Id":"121492df-fb55-4045-a7dd-afe1b8d09a86",
              "DisplayOrder":5,
              "Description":"Significant Impact to Non-Critical Services",
              "Value":50,
              "ScoringCategoryId":"fd802f0e-8953-4616-90e5-9011b66855c6",
              "IsModifier":false
            },
            {
              "Id":"7839ddb2-f9fe-4f15-b40d-d96bf8fcea1d",
              "DisplayOrder":6,
              "Description":"Denial of Non-Critical Services",
              "Value":60,
              "ScoringCategoryId":"fd802f0e-8953-4616-90e5-9011b66855c6",
              "IsModifier":false
            },
            {
              "Id":"b75d25bb-07e7-4be2-9811-f53e979433af",
              "DisplayOrder":7,
              "Description":"Significant Impact to Critical Services",
              "Value":70,
              "ScoringCategoryId":"fd802f0e-8953-4616-90e5-9011b66855c6",
              "IsModifier":false
            },
            {
              "Id":"be367a6c-f58c-4773-accd-aedc4845de6e",
              "DisplayOrder":8,
              "Description":"Denial of Critical Services or Loss of Control",
              "Value":100,
              "ScoringCategoryId":"fd802f0e-8953-4616-90e5-9011b66855c6",
              "IsModifier":false
            },
            {
              "Id":"dd56b32a-ce86-456f-8d87-3fc96c3f55d3",
              "DisplayOrder":4,
              "Description":"Minimal Impact to Critical Services",
              "Value":40,
              "ScoringCategoryId":"fd802f0e-8953-4616-90e5-9011b66855c6",
              "IsModifier":false
            },
            {
              "Id":"e0af432e-9b90-414a-84d6-b5635eff2330",
              "DisplayOrder":3,
              "Description":"Minimal Impact to Non-Critical Services",
              "Value":35,
              "ScoringCategoryId":"fd802f0e-8953-4616-90e5-9011b66855c6",
              "IsModifier":false
            },
            {
              "Id":"60562ba5-1658-46e8-9902-ce8e02593d64",
              "DisplayOrder":1,
              "Description":"None",
              "Value":0,
              "ScoringCategoryId":"214e19ea-592f-43a5-9f78-15e2b9a4b98c",
              "IsModifier":false
            },
            {
              "Id":"7cc977f5-9859-4082-a1e1-1916ab49191f",
              "DisplayOrder":2,
              "Description":"Prepare",
              "Value":40,
              "ScoringCategoryId":"214e19ea-592f-43a5-9f78-15e2b9a4b98c",
              "IsModifier":false
            },
            {
              "Id":"615e11bc-23f6-40f7-9ec9-124c5dfb5594",
              "DisplayOrder":3,
              "Description":"Engage",
              "Value":70,
              "ScoringCategoryId":"214e19ea-592f-43a5-9f78-15e2b9a4b98c",
              "IsModifier":false
            },
            {
              "Id":"00ebebf1-d054-41ee-ba1f-3d8d287a8bdf",
              "DisplayOrder":4,
              "Description":"Presence",
              "Value":80,
              "ScoringCategoryId":"214e19ea-592f-43a5-9f78-15e2b9a4b98c",
              "IsModifier":false
            },
            {
              "Id":"0665c53b-b6a7-423f-ae2e-3e3a6bcddff2",
              "DisplayOrder":5,
              "Description":"Effect",
              "Value":100,
              "ScoringCategoryId":"214e19ea-592f-43a5-9f78-15e2b9a4b98c",
              "IsModifier":false
            },
            {
              "Id":"86aa3992-01a2-4a3f-8b84-632643b389c4",
              "DisplayOrder":1,
              "Description":"Level 1 - Business DMZ",
              "Value":30,
              "ScoringCategoryId":"228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "IsModifier":false
            },
            {
              "Id":"49c648bf-fece-4bef-bf5d-725552ebf364",
              "DisplayOrder":2,
              "Description":"Level 2 - Business Network",
              "Value":40,
              "ScoringCategoryId":"228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "IsModifier":false
            },
            {
              "Id":"8fd26e09-fc53-4c8c-b039-eff2485f1d04",
              "DisplayOrder":3,
              "Description":"Unknown",
              "Value":50,
              "ScoringCategoryId":"228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "IsModifier":false
            },
            {
              "Id":"a404fd29-d68a-4121-b8f4-f2728a173a8d",
              "DisplayOrder":5,
              "Description":"Level 4 - Critical System DMZ",
              "Value":70,
              "ScoringCategoryId":"228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "IsModifier":false
            },
            {
              "Id":"248a4221-5dec-48c9-9b42-2563246b3eb6",
              "DisplayOrder":6,
              "Description":"Level 5 - Critical System Management",
              "Value":80,
              "ScoringCategoryId":"228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "IsModifier":false
            },
            {
              "Id":"f0b994b7-a7d3-4d35-8043-7a96969e4aaa",
              "DisplayOrder":7,
              "Description":"Level 6 - Critical Systems",
              "Value":90,
              "ScoringCategoryId":"228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "IsModifier":false
            },
            {
              "Id":"1525d46a-82c0-4b28-a8a7-3b1d4aae44d4",
              "DisplayOrder":8,
              "Description":"Level 7 - Safety Systems",
              "Value":100,
              "ScoringCategoryId":"228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "IsModifier":false
            },
            {
              "Id":"87fa7750-84c7-4a32-a823-012ba747e75a",
              "DisplayOrder":4,
              "Description":"Level 3 - Business Network Management",
              "Value":60,
              "ScoringCategoryId":"228ccaa0-3579-4535-bb32-8fcc48b26c4e",
              "IsModifier":false
            },
            {
              "Id":"73465387-cb00-4264-86d1-dd1856f523c6",
              "DisplayOrder":1,
              "Description":"Hacktivists",
              "Value":20,
              "ScoringCategoryId":"b0613639-520a-4451-8f39-a6adaab36428",
              "IsModifier":false
            },
            {
              "Id":"7b6bdd44-2426-489c-9dda-8171c5cd8046",
              "DisplayOrder":2,
              "Description":"Unwitting Insider",
              "Value":30,
              "ScoringCategoryId":"b0613639-520a-4451-8f39-a6adaab36428",
              "IsModifier":false
            },
            {
              "Id":"143aa02e-f399-41d0-95d4-01dd2e3ba30b",
              "DisplayOrder":3,
              "Description":"Criminal",
              "Value":40,
              "ScoringCategoryId":"b0613639-520a-4451-8f39-a6adaab36428",
              "IsModifier":false
            },
            {
              "Id":"a6ac5110-7e8d-49d2-b0eb-3f714c8b7f40",
              "DisplayOrder":4,
              "Description":"Unknown",
              "Value":50,
              "ScoringCategoryId":"b0613639-520a-4451-8f39-a6adaab36428",
              "IsModifier":false
            },
            {
              "Id":"4bc7e5da-5d45-4711-821f-017cfb7aace6",
              "DisplayOrder":5,
              "Description":"Witting Insider",
              "Value":65,
              "ScoringCategoryId":"b0613639-520a-4451-8f39-a6adaab36428",
              "IsModifier":false
            },
            {
              "Id":"563fea15-4f29-4b6a-bffc-72877a21d5a2",
              "DisplayOrder":6,
              "Description":"APT",
              "Value":80,
              "ScoringCategoryId":"b0613639-520a-4451-8f39-a6adaab36428",
              "IsModifier":false
            },
            {
              "Id":"ef4322d8-c222-4040-a2dc-20de2a174a11",
              "DisplayOrder":7,
              "Description":"APT (Targeted)",
              "Value":100,
              "ScoringCategoryId":"b0613639-520a-4451-8f39-a6adaab36428",
              "IsModifier":false
            }
          ],
          "Evaluations": [
            {
              "Id": "b92f00e7-8b7d-491f-afdb-389a0332b3b3",
              "Description": "NCISS Demonstration",
              "Status": 20,
              "CurrentMoveNumber": 0,
              "ScoringModelId": "121c225d-796a-448c-a5cd-95c7a9436d51",
              "GalleryExhibitId": "c72ab146-ad44-44cd-877e-d03872516c30"
            }
          ],
          "Teams": [
            {
              "Id": "54644c9c-8cf7-40e6-b645-9b2f83cb8314",
              "Name": "CMU - Carnegie Mellon University",
              "ShortName": "CMU",
              "TeamTypeId": "28c9d58d-b273-48b1-a1f5-ac1048c55810",
              "EvaluationId": "b92f00e7-8b7d-491f-afdb-389a0332b3b3"
            },
            {
              "Id": "d208de1e-3792-4a87-ac97-212d1cc5faca",
              "Name": "Test",
              "ShortName": "Test",
              "TeamTypeId": "28c9d58d-b273-48b1-a1f5-ac1048c55810",
              "EvaluationId":"b92f00e7-8b7d-491f-afdb-389a0332b3b3"
            }
          ],
          "TeamUsers": [
            {
              "Id": "e31d500d-f0a9-4e43-9526-5fd2a6c4bd7b",
              "UserId": "dee684c5-2eaf-401a-915b-d3d4320fe5d5",
              "TeamId": "54644c9c-8cf7-40e6-b645-9b2f83cb8314"
            }
          ],
          "Moves": [
            {
              "Id": "b07d52c9-4d8d-4bca-a6c7-a5c02709f872",
              "Description": "The exercise will begin at 0900 EST",
              "MoveNumber": 0,
              "SituationTime": "2021-12-03T14:38:00Z",
              "SituationDescription": "Please score the current incident using the National Cyber Incident Scoring System.",
              "EvaluationId": "b92f00e7-8b7d-491f-afdb-389a0332b3b3"
            }
          ],
          "Actions": [
            {
              "Id": "a3c54936-d61c-4d53-bbb6-c29621ac5047",
              "Description": "Check the Gallery",
              "EvaluationId":"b92f00e7-8b7d-491f-afdb-389a0332b3b3",
              "TeamId":"54644c9c-8cf7-40e6-b645-9b2f83cb8314",
              "MoveNumber": 0,
              "InjectNumber": 0
            }
          ],
          "Roles": [
            {
              "Id": "59266893-644e-4159-bb85-8bd01f64d8c8",
              "Name": "Team Lead",
              "EvaluationId":"b92f00e7-8b7d-491f-afdb-389a0332b3b3",
              "TeamId": "54644c9c-8cf7-40e6-b645-9b2f83cb8314"
            }
          ],
          "RoleUsers": [
            {
              "Id": "fb1e1e65-23c5-4c9a-8cea-b0b54fb29d4f",
              "UserId":"dee684c5-2eaf-401a-915b-d3d4320fe5d5",
              "RoleId": "59266893-644e-4159-bb85-8bd01f64d8c8"
            }
          ]
        }
```
</details>

This seed data is used as an example in order to show an exercise. This data can be configured and changed to your needs. We do recommend seeding the users and permissions. The data populated in the users section should reflect any data seeded into Identity regarding the GUID for the user.

### CITE UI

#### CITE UI Ingress

```yaml
  # Ingress configuration example for NGINX
  # TLS and Host URLs need configured
  ingress:
    enabled: true
    annotations:
      kubernetes.io/ingress.class: nginx
    hosts:
      - host: cite.$DOMAIN
        paths:
          - path: /
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
          - $DOMAIN
```

#### Cite UI Settings

```yaml
  # Config app settings with a JSON file.
  # These values correspond to an OpenID connect client and
  # some basic configuration
  settings: '{
    "ApiUrl": "https://cite.$DOMAIN",
    "GalleryApiUrl": "https://gallery.$DOMAIN",
    "OIDCSettings": {
    "authority": "https://$DOMAIN/identity/",
    "client_id": "cite-ui",
    "redirect_uri": "https://cite.$DOMAIN/auth-callback",
    "post_logout_redirect_uri": "https://cite.$DOMAIN",
    "response_type": "code",
    "scope": "openid profile cite-api gallery-api",
    "automaticSilentRenew": true,
    "silent_redirect_uri": "https://cite.$DOMAIN/auth-callback-silent"
    },
    "AppTitle": "CITE",
    "AppTopBarHexColor": "#2d69b4",
    "AppTopBarHexTextColor": "#FFFFFF",
    "AppTopBarText": "CITE  -  Collaborative Incident Threat Evaluator",
    "UseLocalAuthStorage": false,
    "DefaultScoringModelId": "d4b4e80c-0ce6-4601-9820-6802e70504b4",
    "DefaultEvaluationId": "2f82cb19-4c0a-4b60-857f-c4f2a9e70819",
    "DefaultTeamId": "cfe007a7-2dcf-40a9-b157-f3dee2005c7d"
    }'
```

These settings are recommended but it's important to note that the GUID's that are at the bottom of the settings correlate to the seed data given to the API. **Please make sure these values match.**

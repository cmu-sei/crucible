# Installing Identity

## Resources

* [identity.values.yaml](./identity.values.yaml)
* [appsettings.conf](https://github.com/cmu-sei/Identity/blob/master/src/IdentityServer/appsettings.conf)
* [Identity Helm Chart](https://github.com/cmu-sei/helm-charts/blob/main/charts/identity)
* [Identity Server 4 Documentation](https://identityserver4.readthedocs.io/)

## Identity API and UI Ingress

Identity API:

```yaml
  ingress:
    enabled: true
    annotations: {}
    hosts:
      - host: $DOMAIN
        paths:
          - path: /identity
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
          - $DOMAIN
```

Identity UI:

```yaml
  ingress:
    enabled: true
    annotations: {}
    hosts:
      - host: $DOMAIN
        paths:
          - path: /identity/ui
            pathType: ImplementationSpecific
    tls:
      - secretName: appliance-cert
        hosts:
          - $DOMAIN
```

Identity can be path based shown by the above examples of the API and UI ingress. This also takes your certificates stored as a tls secret in the key `secretName` under `tls`.

## Identity Environmental Variables Settings

Within the **identity.values.yaml** file example, there's a section in identity-api that holds values that have to be set for a successful installation. These are customizable for your particular install. Below is the [appsettings.conf](https://github.com/cmu-sei/Identity/blob/master/src/IdentityServer/appsettings.conf) file that show all the options for Identity.

<details markdown="1">

<summary> Identity appsettings.conf file from GitHub </summary>

```bash
####################
## AppSettings
## Defaults are commented out. Uncomment to change.
## Scroll to bottom for example of appsettings.Development.conf
####################

## Set lifetime of identity auth cookie
# Authorization__CookieLifetimeMinutes = 600
# Authorization__CookieSlidingExpiration = false

####################
## Database
####################

## Supported providers: InMemory, PostgreSQL, SqlServer
# Database__Provider = InMemory
# Database__ConnectionString = IdentityServer

## File containing any seed data.  See docs/ImportingData.md
# Database__SeedFile =

####################
## Branding
####################

# Branding__ApplicationName = Foundry ID
# Branding__Title = OpenID Connect
# Branding__LogoUrl =

## If deployed in virtual directory, set path base
# Branding__PathBase =

## Disable the Swagger OpenApi host by setting to false
# Branding__IncludeSwagger = true

## Set the url of the identity-ui app.
## Production (usually, if hosted with this app): ~/ui
## Development (usually, if default ng serve): http://localhost:4200
# Branding__UiHost =

####################
## Caching
####################

## When running multiple replicas of this app, you should also
## run redis for shared caching.
# Cache__RedisUrl =

## This app's key prefix for the redis instance (e.g: idsrv)
# Cache__Key =

## If not using redis, provide a path to a shared folder for
## data protection keys (for cookie signing, etc).
# Cache__SharedFolder =

####################
## Logging
####################
# Logging__Console__DisableColors = false
# Logging__LogLevel__Default = Information

####################
## Identity.Accounts
####################

## Seed the admin account (blank guid is okay)
# Account__AdminEmail =
# Account__AdminPassword =
# Account__AdminGuid =

## Seed an Override Code (nice for initial 2FA login)
# Account__OverrideCode =

# Account__Password__ComplexityExpression = (?=^.{8,}$)(?=.*\\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[`~!@#$%^&*\\(\\)\\-_=+\\[\\]\\{\\}\\\\|;:'\",<\\.>/?\\t]).*$
# Account__Password__ComplexityText = At least 8 characters containing uppercase and lowercase letters, numbers, and symbols
# Account__Password__History = 0
# Account__Password__Age = 0
# Account__Password__ResetTokenExpirationMinutes = 60

## Multiple domains are delimited with space or pipe |
# Account__Registration__AllowedDomains =
# Account__Registration__AllowManual = false
# Account__Registration__StoreName = true
# Account__Registration__StoreEmail = true
# Account__Registration__AutoUniqueUsernames = true

## Use to allow additional usernames (email address)
# Account__Registration__AllowMultipleUsernames = false

## Use to bypass domain restrictions on *additional* emails
## for already registered users
# Account__Registration__AllowAnyDomainUsernames = false

# Account__Authentication__AllowAutoLogin = true
# Account__Authentication__AllowCredentialLogin = true
# Account__Authentication__Require2FA = true
# Account__Authentication__LockThreshold = 0
# Account__Authentication__AllowRememberLogin = true
# Account__Authentication__RememberMeLoginDays = 30
# Account__Authentication__ExpireAfterDays = 0

## Display string of acceptable certs
# Account__Authentication__CertificateIssuers =

## Certificate to sign tokens.  If blank, a key is generated. (But won't be persisted if in a container.)
# Account__Authentication__SigningCertificate =
# Account__Authentication__SigningCertificatePassword =

## Header values for certificate data received from reverse proxy (i.e. nginx)
## ** These are NOT defaults. You must include your values.  Nginx values are shown.
# Account__Authentication__ClientCertHeader = X-ARR-ClientCert
# Account__Authentication__ClientCertSubjectHeaders__0 = ssl-client-subject-dn
# Account__Authentication__ClientCertIssuerHeaders__0 = ssl-client-issuer-dn
# Account__Authentication__ClientCertSerialHeaders__0 = ssl-client-serial
# Account__Authentication__ClientCertVerifyHeaders__0 = ssl-client-verify

## location of customized html for insertion into the referenced page
# Account__Authentication__NoticeFile = wwwroot/html/notice.html
# Account__Authentication__TroubleFile = wwwroot/html/trouble.html

## Allow any authenticated user visibilty of user profiles
# Account__Profile__ForcePublic = false

## Url for constructing avatars. If no ImageServerUrl,
## defaults to this app's url with the ImagePath value.
# Account__Profile__ImageServerUrl =
# Account__Profile__ImagePath = /javatar

# Account__Profile__ProfileImagePath = p
# Account__Profile__OrganizationImagePath = o
# Account__Profile__OrganizationUnitImagePath = u
# Account__Profile__UseDefaultAvatar = false
# Account__Profile__DefaultLogo = default.png

## This is only used if certificates get passed to this app for validation.
## Generally, it's recommended to offload cert validation to the ssl terminator
## (i.e. nginx)
# Account__CertValidation__IssuerCertificatesPath = certs
# Account__CertValidation__CheckRevocationOnline = false
# Account__CertValidation__CheckChainRevocation = false
# Account__CertValidation__VerificationTimeoutSeconds = 0

####################
## AppMail
####################
## The application sends mail, primarily for retrieval of 2fa and verification codes.
## It talks to an AppMailRelay host.

## Url to the AppMailRelay endpoint
# AppMail__Url =

## Api Key valid at AppMailRelay endpoint
# AppMail__Key =

## mailto address for sender (if different than AppMailRelay default sender)
# AppMail__From =

####################
## JAvatar
####################
# JAvatar__RoutePrefix = /javatar

####################
## Headers
####################
# Headers__LogHeaders = false
# Headers__Cors__Origins__0 =
# Headers__Cors__Methods__0 =
# Headers__Cors__Headers__0 =
# Headers__Cors__AllowCredentials = false

## If running behind a reverse proxy, be sure to pass "forward" headers
## TargetHeaders = All tells the app to expect x-forwarded-host, x-forwarded-proto and x-forwarded-for.
## Or pass a comma delimited subset of those.  Only the first two of those are required.
## https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-3.1
# Headers__Forwarding__TargetHeaders = None
# Headers__Forwarding__KnownNetworks = 10.0.0.0/8 172.16.0.0/12 192.168.0.0/24 ::ffff:a00:0/104 ::ffff:ac10:0/108 ::ffff:c0a8:0/120
# Headers__Forwarding__KnownProxies =
# Headers__Forwarding__ForwardLimit = 1
# Headers__Forwarding__ForwardedForHeaderName =
# Headers__Security__ContentSecurity = default-src 'self'; frame-ancestors 'self'
# Headers__Security__XContentType = nosniff
# Headers__Security__XFrame = SAMEORIGIN

###################
## Example for appsettings.Development.conf
###################

# Branding__UiHost = http://localhost:4200

# Database__Provider = PostgreSQL
# Database__ConnectionString = Server=localhost;Database=idtest_db

# Account__Profile__ImageServerUrl = http://localhost:5000/javatar

# Headers__Cors__Origins__0 = http://localhost:4200
# Headers__Cors__Methods__0 = *
# Headers__Cors__Headers__0 = *
# Headers__Cors__AllowCredentials = true
# Headers__Security__ContentSecurity = default-src 'self'; frame-ancestors 'self'

# Logging__LogLevel__Microsoft.Hosting.Lifetime = Information
# Logging__LogLevel__Microsoft = Warning
```

</details>

There are particular ones used in the values file included:

```yaml
env:
    # Supported providers: InMemory, PostgreSQL, SqlServer
    Database__Provider: PostgreSQL
    Database__ConnectionString: Server=;Port=5432;Database=identity;Username=$POSTGRES_USER;Password=$POSTGRES_PASS;SSL Mode=Prefer;Trust Server Certificate=true;
    Database__SeedFile: conf/seed.json
    Branding__ApplicationName: Foundry Identity
    Branding__UiHost: /identity/ui
    Branding__PathBase: /identity
    Account__Registration__AllowManual: true
    Account__Registration__AllowedDomains: $DOMAIN
    Cache__Key: idsrv
    Cache__RedisUrl: ''
    Cache__SharedFolder: ''
    Logging__Console__DisableColors: true
    Account__AdminEmail: 'administrator@$DOMAIN'
    Account__AdminPassword: $GLOBAL_ADMIN_PASS
    Account__AdminGuid: 'dee684c5-2eaf-401a-915b-d3d4320fe5d5'
    Account__OverrideCode: ''
    Account__Authentication__SigningCertificate: conf/signer.pfx
    Account__Authentication__SigningCertificatePassword: foundry
    Account__Authentication__NoticeFile: 'conf/notice.html'
    AppMail__Url:
    AppMail__Key:
    AppMail__From:
    Headers__Cors__AllowAnyOrigin: true
    Headers__Cors__AllowAnyMethod: true
    Headers__Cors__AllowAnyHeader: true
    Headers__Forwarding__TargetHeaders: All
    Headers__Forwarding__KnownNetworks: '10.0.0.0/8 172.16.0.0/12 192.168.0.0/16 ::ffff:a00:0/104 ::ffff:ac10:0/108 ::ffff:c0a8:0/112'
    Headers__Security__ContentSecurity: "default-src 'self'; style-src 'self' http://$DOMAIN/ 'unsafe-inline'; script-src 'self' http://$DOMAIN/ 'unsafe-inline' 'unsafe-eval'"

```

Please note this is all customizable. These are similar but not exact from what we use in our installations. For example, if you did not want to use 2FA, remove the key and value `Account__OverrideCode: '123456'` and include `Account__Authentication__Require2FA = true` in the env section.

## HTML Pages for Notice, Terms and Trouble

Below is part of the configuration of Identity. These are mounted in the container for use post deployment. The notice, terms, and trouble keys take HTML as a variable. Issuers take a pem as a variable. [The chart being used for these settings is located here for more information.](https://github.com/cmu-sei/helm-charts/blob/main/charts/identity/charts/identity-api/templates/configmap.yaml)

```yaml
  conf:
    issuers: ''
    notice: |
      <div>
        <h4>
            Cookie Policy
        </h4>
        <p class="text-justify">
            This site uses browser cookies for authentication. When you login, we add a cookie that gets sent back to this
            server with each request for the duration of your session. That's so you don't have to login to view
            <i>every</i> page.
        </p>

        <h4>
            Privacy Policy
        </h4>
        <p class="text-justify">
            We store your email address, your name and organization name, if you provide them. This information is
            available to staff for the purpose of analyzing participation and communicating with participants.
        </p>

        <h4>
            Terms of Service
        </h4>
        <p class="text-justify">
            This site is administered by Carnegie Mellon University. Use is "AS IS", and Carnegie Mellon University
            accepts no liability from your use of it.
        </p>
      </div>

    terms: ''
    trouble: ''
```

## Identity Seed Data

The seed data is important to expedite the install of Identity. This includes the creation of users, API resources, clients and the settings for the clients. Below is an example of seed data being used in installations. [Here is the documentation for various settings included in the seed data.](https://identityserver4.readthedocs.io/) It is important to note that the administrator account is created by the environment variables but the seed data is used to create additional accounts when Identity is spun up.

<details markdown="1">

<summary>Sample Seed Data</summary>

```json
 seed: |
      {
        "Users": [
          {
            "Username": "crucible-admin@$DOMAIN",
            "Password": "",
            "GlobalId": ""
          }
        ],
        "ApiResources": [
          {
            "Name": "caster-api",
            "DisplayName": "Caster API",
            "Enabled": true
          },
          {
            "Name": "player-api",
            "DisplayName": "Player API",
            "Enabled": true
          },
          {
            "Name": "vm-api",
            "DisplayName": "VM API",
            "Enabled": true
          },
          {
            "Name": "alloy-api",
            "DisplayName": "Alloy API",
            "Enabled": true
          },
          {
            "Name": "steamfitter-api",
            "DisplayName": "Steamfitter API",
            "Enabled": true
          },
          {
            "Name": "gallery-api",
            "DisplayName": "Gallery API",
            "Enabled": true
          },
          {
            "Name": "blueprint-api",
            "DisplayName": "Blueprint API",
            "Enabled": true
          },
          {
            "Name": "CITE-api",
            "DisplayName": "CITE API",
            "Enabled": true
          },
        ],
        "DefaultClientFlags": "AllowRememberConsent, AlwaysIncludeUserClaimsInIdToken, UpdateAccessTokenClaimsOnRefresh, EnableLocalLogin",
        "Clients": [
          {
            "Name": "bootstrap-client",
            "DisplayName": "Bootstrap",
            "Enabled": true,
            "SeedGrant": "password",
            "SeedScopes": "openid profile topomojo-api identity-api identity-api-privileged",
            "SeedSecret": "foundry"
          },
          {
            "Name": "caster-api",
            "DisplayName": "Caster Api",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "caster-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/caster" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/caster/api/oauth2-redirect.html" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/caster-api" }
            ]
          },
          {
            "Name": "caster-admin",
            "DisplayName": "Caster Admin",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "password client_credentials",
            "SeedScopes": "player-api vm-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/caster" }
            ]
          },
          {
            "Name": "caster-ui",
            "DisplayName": "Caster",
            "Enabled": true,
            "SeedFlags" : "Published",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile email caster-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/caster" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/caster/auth-callback" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/caster/auth-callback-silent" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/caster" }
            ]
          },
          {
            "Name": "gitlab",
            "DisplayName": "Gitlab",
            "Enabled": true,
            "SeedFlags" : "Published",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile email",
            "SeedSecret": "",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://gitlab.$DOMAIN" },
              { "Type": "RedirectUri", "Value": "https://gitlab.$DOMAIN/users/auth/identity/callback" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://gitlab.$DOMAIN" }
            ]
          },
          {
            "Name": "player-api",
            "DisplayName": "Player API",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "player-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/player" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/player/api/oauth2-redirect.html" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/player/api" }
            ]
          },
          {
            "Name": "player-admin",
            "DisplayName": "Player Admin",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "password client_credentials",
            "SeedScopes": "player-api vm-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/player" }
            ]
          },
          {
            "Name": "player-ui",
            "DisplayName": "Player",
            "Enabled": true,
            "SeedFlags" : "Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile player-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/player" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/player/auth-callback" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/player/auth-callback-silent" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/player" }
            ]
          },
          {
            "Name": "vm-api",
            "DisplayName": "VM API",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile vm-api player-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/vm" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/vm/api/oauth2-redirect.html" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/vm/api" }
            ]
          },
          {
            "Name": "vm-ui",
            "DisplayName": "VM",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile vm-api player-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/vm" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/vm/auth-callback" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/vm/auth-callback-silent" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/vm" }
            ]
          },
          {
            "Name": "vm-console-ui",
            "DisplayName": "VM Console UI",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile vm-api player-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/console" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/console/auth-callback" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/console/auth-callback-silent" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/console" }
            ]
          },
          {
            "Name": "alloy-api",
            "DisplayName": "Alloy API",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "alloy-api player-api vm-api caster-api steamfitter-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/alloy/api" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/alloy/api/oauth2-redirect.html" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/alloy/api" }
            ]
          },
          {
            "Name": "alloy-admin",
            "DisplayName": "Alloy Admin",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "password client_credentials",
            "SeedScopes": "alloy-api player-api caster-api steamfitter-api vm-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/alloy" }
            ]
          },
          {
            "Name": "alloy-ui",
            "DisplayName": "Alloy",
            "Enabled": true,
            "SeedFlags" : "Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile alloy-api player-api caster-api steamfitter-api vm-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/alloy" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/alloy/auth-callback" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/alloy/auth-callback-silent" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/alloy" }
            ]
          },
          {
            "Name": "steamfitter-api",
            "DisplayName": "Steamfitter API",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "steamfitter-api vm-api player-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/steamfitter/api" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/steamfitter/api/oauth2-redirect.html" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/steamfitter/api" }
            ]
          },
          {
            "Name": "steamfitter-admin",
            "DisplayName": "Steamfitterr Admin",
            "Enabled": true,
            "SeedFlags" : "RequirePkce",
            "SeedGrant": "password client_credentials",
            "SeedScopes": "player-api steamfitter-api vm-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/steamfitter" }
            ]
          },
          {
            "Name": "steamfitter-ui",
            "DisplayName": "Steamfitter",
            "Enabled": true,
            "SeedFlags" : "Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile steamfitter-api vm-api player-api",
            "Urls": [
              { "Type": "ClientUri", "Value": "https://$DOMAIN/steamfitter" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/steamfitter/auth-callback" },
              { "Type": "RedirectUri", "Value": "https://$DOMAIN/steamfitter/auth-callback-silent" },
              { "Type": "PostLogoutRedirectUri", "Value": "https://$DOMAIN/steamfitter" }
            ]
          },
          {
            "Name": "cite-api",
            "DisplayName": "CITE API",
            "Enabled": true,
            "SeedFlags": "AllowAccessTokensViaBrowser, Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid cite-api",
            "Urls": [
              {
                "Type": "ClientUri",
                "Value": "https://$DOMAIN/cite"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/cite/api/oauth2-redirect.html"
              },
              {
                "Type": "PostLogoutRedirectUri",
                "Value": "https://$DOMAIN/cite/api"
              }
            ]
          },
          {
            "Name": "cite-ui",
            "DisplayName": "CITE UI",
            "Enabled": true,
            "SeedFlags": "AllowAccessTokensViaBrowser, RequireConsent, Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile cite-api",
            "Urls": [
              {
                "Type": "ClientUri",
                "Value": "https://$DOMAIN/cite"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/cite/auth-callback"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/cite/auth-callback-silent"
              },
              {
                "Type": "PostLogoutRedirectUri",
                "Value": "https://$DOMAIN/cite"
              },
              {
                "Type": "CORSUri",
                "Value": "https://$DOMAIN/cite"
              }
            ]
          },
          {
            "Name": "gallery-api",
            "DisplayName": "Gallery API",
            "Enabled": true,
            "SeedFlags": "AllowAccessTokensViaBrowser, Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid gallery-api",
            "Urls": [
              {
                "Type": "ClientUri",
                "Value": "https://$DOMAIN/gallery"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/gallery/api/oauth2-redirect.html"
              },
              {
                "Type": "PostLogoutRedirectUri",
                "Value": "https://$DOMAIN/gallery/api"
              }
            ]
          },
          {
            "Name": "gallery-ui",
            "DisplayName": "Gallery UI",
            "Enabled": true,
            "SeedFlags": "AllowAccessTokensViaBrowser, RequireConsent, Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile gallery-api",
            "Urls": [
              {
                "Type": "ClientUri",
                "Value": "https://$DOMAIN/gallery"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/gallery/auth-callback"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/gallery/auth-callback-silent"
              },
              {
                "Type": "PostLogoutRedirectUri",
                "Value": "https://$DOMAIN/gallery"
              },
              {
                "Type": "CORSUri",
                "Value": "https://$DOMAIN/gallery"
              }
            ]
          },
          {
            "Name": "blueprint-api",
            "DisplayName": "Blueprint API",
            "Enabled": true,
            "SeedFlags": "AllowAccessTokensViaBrowser, Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid blueprint-api",
            "Urls": [
              {
                "Type": "ClientUri",
                "Value": "https://$DOMAIN/blueprint"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/blueprint/api/oauth2-redirect.html"
              },
              {
                "Type": "PostLogoutRedirectUri",
                "Value": "https://$DOMAIN/blueprint/api"
              }
            ]
          },
          {
            "Name": "blueprint-ui",
            "DisplayName": "Blueprint UI",
            "Enabled": true,
            "SeedFlags": "AllowAccessTokensViaBrowser, RequireConsent, Published, RequirePkce",
            "SeedGrant": "authorization_code",
            "SeedScopes": "openid profile blueprint-api",
            "Urls": [
              {
                "Type": "ClientUri",
                "Value": "https://$DOMAIN/blueprint"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/blueprint/auth-callback"
              },
              {
                "Type": "RedirectUri",
                "Value": "https://$DOMAIN/blueprint/auth-callback-silent"
              },
              {
                "Type": "PostLogoutRedirectUri",
                "Value": "https://$DOMAIN/blueprint"
              },
              {
                "Type": "CORSUri",
                "Value": "https://$DOMAIN/blueprint"
              }
            ]
          },
        ]
      }
```

</details>

## Certificate to Sign Tokens

```yaml
signer: MIIKnwIBAzCCClUGCSqGSIb3DQ...
```

This certificate is created with bash command using the [openssl binary](https://github.com/openssl/openssl/releases):

```bash
openssl pkcs12 -export -out host.pfx -inkey host-key.pem -in host.pem \
               -passin pass:secret -passout pass:secret
```

This is used to sign tokens within Identity. If this is left blank, a key will be created but **it will not persist if Identity would redeploy.** The certificate script will create the key and place it into **identity.values.yaml** using sed.

## Using Helm to install Identity

> This is an example script that uses the env file to install Identity.

```bash
function identity () {
    if [ "${HAS_POSTGRES}" != "true" ]; then
        echo "Postgresql Required"
        exit 1
    fi
    sed -ri "s|(signer:) \"\"|\1 $(base64 -w0 certificates/host.pfx)|" values/foundry/identity.values.yaml
    envsubst < values/foundry/identity.values.yaml | helm upgrade -i identity sei/identity -f -
    echo "Administrator pass: $GLOBAL_ADMIN_PASS"
    echo "2Factor Bypass: "
}
```

Finally, to install Identity, the only requirement is to already have ingress-nginx and postgresql installed in your Kubernetes environment. Once you have completed the `identity.values.yaml` file to your customizations, use helm to install:

```bash
helm upgrade -i identity sei/identity -f identity.values.yaml
```

> This this script, I use upgrade instead of install. The `-i` flag will install if `identity` does not already exist but will update/upgrade if it does. This makes it repeatable for future upgrades and/or changes.

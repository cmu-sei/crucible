# autorest config
#
# Make sure you have installed autorest
#     $ npm install -g autorest
#
# To create the swagger.json file, run the following command from ..\Steamfitter.Api folder
#     $ dotnet swagger tofile --output ..\Steamfitter.Api.Client\swagger.json bin\Debug\netcoreapp3.1\steamfitter.api.dll v1
#
# To generate the api client code ...
#   from this folder (must contain this file, the csproj file and the swagger.json file) run the following command:
#     $ autorest
#
# To create the nuget package ...
#   then, create the nuget package by running one of the following (with/without designating a version):
#     $ dotnet pack
#     $ dotnet pack /p:version=1.2.3-sps273
#
# To push the package to the nuget server, run the following command:
#     $ dotnet nuget push bin/Debug/steamfitter.api.client.<version>.nupkg -s https://nuget.cwd.local/v3/index.json -k !EatsShootsandLeaves!

# The following line is "magic text" that must be included in this file
> see https://aka.ms/autorest

``` yaml

input-file: swagger.json

csharp:
  namespace: Steamfitter.Api
  add-credentials: false
  override-client-name: SteamfitterApiClient
  output-folder: ./code
  # stop the simplifier from making Task conflict:
  skip-simplifier-on-namespace: 
    - System.Threading.Tasks

```
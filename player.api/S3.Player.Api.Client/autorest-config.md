# autorest config
#
# To create the swagger.json file, run the following command from ..\S3.Player.Api folder
#     $ dotnet swagger tofile --output ..\S3.Player.Api.Client\swagger.json bin\Debug\netcoreapp2.1\s3.player.api.dll v1
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
# To push the package to the nuget server, run the followign command:
#     $ dotnet nuget push bin/Debug/s3.player.api.client.<version>.nupkg -s https://nuget.cwd.local/v3/index.json -k !EatsShootsandLeaves!

# The following line is "magic text" that must be included in this file
> see https://aka.ms/autorest

``` yaml

input-file: swagger.json

csharp:
  namespace: S3.Player.Api
  add-credentials: false
  override-client-name: S3PlayerApiClient
  output-folder: ./code

```

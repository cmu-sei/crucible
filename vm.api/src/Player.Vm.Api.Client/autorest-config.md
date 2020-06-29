# autorest config

# To create the swagger.json file, run the following command from ..\S3.VM.Api folder
#     $ dotnet swagger tofile --output ..\S3.VM.Api.Client\swagger.json bin\Debug\netcoreapp2.1\s3.vm.api.dll v1
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


> see https://aka.ms/autorest

``` yaml

input-file: swagger.json

csharp:
  namespace: S3.VM.Api
  add-credentials: false
  override-client-name: S3VmApiClient
  output-folder: ./code

```
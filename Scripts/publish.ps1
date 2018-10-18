#REQUIRES -Version 5.1
Set-StrictMode -Version Latest

$sortingHatRoot = "$PSScriptRoot/.."
&dotnet publish $sortingHatRoot/SortingHat.Console/SortingHat.CLI.csproj --configuration Release --runtime win-x64 --output ../publish
&dotnet publish $sortingHatRoot/SortingHat.Plugin.Exif/SortingHat.Plugin.Exif.csproj --configuration Release --runtime win-x64 --output ../publish/plugins
&dotnet publish $sortingHatRoot/SortingHat.Plugin.FileType/SortingHat.Plugin.FileType.csproj --configuration Release --runtime win-x64 --output ../publish/plugins

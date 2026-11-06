#REQUIRES -Version 5.1
Set-StrictMode -Version Latest

$sortingHatRoot = "$PSScriptRoot/.."
$publish = "$sortingHatRoot/publish"
&dotnet publish $sortingHatRoot/SortingHat.Console/SortingHat.CLI.csproj --configuration Release --runtime win-x64 --output $publish
&dotnet publish $sortingHatRoot/SortingHat.Plugin.Exif/SortingHat.Plugin.Exif.csproj --configuration Release --runtime win-x64 --output $publish/plugins
&dotnet publish $sortingHatRoot/SortingHat.Plugin.FileType/SortingHat.Plugin.FileType.csproj --configuration Release --runtime win-x64 --output $publish/plugins
&dotnet publish $sortingHatRoot/SortingHat.Plugin.ExtractRelevantText/SortingHat.Plugin.ExtractRelevantText.csproj --configuration Release --runtime win-x64 --output $publish/plugins

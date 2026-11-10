#REQUIRES -Version 5.1
Set-StrictMode -Version Latest

$sortingHatRoot = "$PSScriptRoot/.."
$publish = "$sortingHatRoot/publish"
&dotnet publish $sortingHatRoot/SortingHat.Console/SortingHat.CLI.csproj --configuration Release --runtime win-x64 --output $publish

# the desktop app next to hat: both read the same hat-config.json and load the same plugins
&dotnet publish $sortingHatRoot/SortingHat.Desktop/SortingHat.Desktop.csproj --configuration Release --runtime win-x64 --output $publish

# every plugin in its own folder: plugins/<name>/<name>.dll, the layout hat loads
Get-ChildItem -Path $sortingHatRoot -Directory -Filter 'SortingHat.Plugin.*' | ForEach-Object {
    &dotnet publish "$($_.FullName)/$($_.Name).csproj" --configuration Release --runtime win-x64 --output "$publish/plugins/$($_.Name)"
}

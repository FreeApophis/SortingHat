&dotnet publish SortingHat.Console/SortingHat.CLI.csproj --configuration Release --runtime win-x64 --output ../publish
&dotnet publish SortingHat.Plugin.Exif/SortingHat.Plugin.Exif.csproj --configuration Release --runtime win-x64 --output ../publish/plugins
&dotnet publish SortingHat.Plugin.FileType/SortingHat.Plugin.FileType.csproj --configuration Release --runtime win-x64 --output ../publish/plugins

&dotnet publish SortingHat.Console/SortingHat.CLI.csproj --configuration Release --runtime win-x64 --output publish /p:PublishSingleFile=true

New-Item -Path "publish" -Name "plugins" -ItemType "directory" -Force

&dotnet publish SortingHat.Plugin.Exif/SortingHat.Plugin.Exif.csproj --configuration Release --runtime win-x64
Copy-Item "SortingHat.Plugin.Exif\bin\Release\netstandard2.1\win-x64\publish\SortingHat.Plugin.Exif.dll" -Destination "publish\plugins"
Copy-Item "SortingHat.Plugin.Exif\bin\Release\netstandard2.1\win-x64\publish\MetadataExtractor.dll" -Destination "publish\plugins"

&dotnet publish SortingHat.Plugin.FileType/SortingHat.Plugin.FileType.csproj --configuration Release --runtime win-x64
Copy-Item "SortingHat.Plugin.FileType\bin\Release\netstandard2.1\win-x64\publish\SortingHat.Plugin.FileType.dll" -Destination "publish\plugins"

&dotnet publish SortingHat.Plugin.ExtractRelevantText/SortingHat.Plugin.ExtractRelevantText.csproj --configuration Release --runtime win-x64
Copy-Item "SortingHat.Plugin.ExtractRelevantText\bin\Release\netstandard2.1\win-x64\publish\SortingHat.Plugin.ExtractRelevantText.dll" -Destination "publish\plugins"

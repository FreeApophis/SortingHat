&dotnet publish SortingHat.Plugin.ExtractRelevantText/SortingHat.Plugin.ExtractRelevantText.csproj --configuration Release --runtime win-x64
Copy-Item "SortingHat.Plugin.ExtractRelevantText\bin\Release\netstandard2.1\win-x64\publish\SortingHat.Plugin.ExtractRelevantText.dll" -Destination "publish\plugins"

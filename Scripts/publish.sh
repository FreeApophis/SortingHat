#!/usr/bin/env bash
set -e

sorting_hat_root="$(dirname $(readlink -f $0))/.."
dotnet publish $sorting_hat_root/SortingHat.Console/SortingHat.CLI.csproj --configuration Release --output ../publish
dotnet publish $sorting_hat_root/SortingHat.Plugin.Exif/SortingHat.Plugin.Exif.csproj --configuration Release --output ../publish/plugins
dotnet publish $sorting_hat_root/SortingHat.Plugin.FileType/SortingHat.Plugin.FileType.csproj --configuration Release --output ../publish/plugins

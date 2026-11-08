#!/usr/bin/env bash
set -e

sorting_hat_root="$(dirname $(readlink -f $0))/.."
publish="$sorting_hat_root/publish"
dotnet publish $sorting_hat_root/SortingHat.Console/SortingHat.CLI.csproj --configuration Release --output $publish

# every plugin in its own folder: plugins/<name>/<name>.dll, the layout hat loads
for plugin in "$sorting_hat_root"/SortingHat.Plugin.*/; do
    name="$(basename "$plugin")"
    dotnet publish "$plugin/$name.csproj" --configuration Release --output "$publish/plugins/$name"
done

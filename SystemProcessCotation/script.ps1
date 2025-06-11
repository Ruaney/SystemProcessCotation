dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

& ".\bin\Release\net9.0\win-x64\publish\SystemProcessCotation.exe" $args[0] $args[1] $args[2]
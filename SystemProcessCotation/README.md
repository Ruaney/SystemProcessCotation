to build the project
 dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

to run
 bin\Release\net9.0\win-x64\publish\SystemProcessCotation.exe PETR4 22.67 22.59

 .\script.ps1 PETR4 22.67 22.59

 Set-ExecutionPolicy Unrestricted
 Set-ExecutionPolicy Restricted
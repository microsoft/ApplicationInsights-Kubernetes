@ECHO OFF
SET CONFIG=%1
IF '%CONFIG%'=='' SET CONFIG=DEBUG
dotnet test %~dp0\..\tests\UnitTests\UnitTests.csproj --no-build --logger "trx" -c %CONFIG%
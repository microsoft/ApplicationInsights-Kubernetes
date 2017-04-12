@ECHO OFF
SET CONFIG=%1
SET LOG_FILE_NAME=%2
IF '%CONFIG%'=='' SET CONFIG=DEBUG
IF '%LOG_FILE_NAME%'=='' SET LOG_FILE_NAME=TestResults.trx
dotnet test %~dp0\..\tests\UnitTests\UnitTests.csproj --no-build --logger "trx;LogFileName=%LOG_FILE_NAME%" -c %CONFIG%
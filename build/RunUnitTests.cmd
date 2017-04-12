@ECHO OFF
dotnet test %~dp0\..\tests\UnitTests\UnitTests.csproj --no-build --logger "trx"
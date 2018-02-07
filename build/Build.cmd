@ECHO OFF
SETLOCAL
SET CONFIG=%1
SET REBUILD=%2
IF '%CONFIG%' == '' SET CONFIG=Debug

ECHO Target Configuration: %CONFIG%.
FOR /F "TOKENS=1* DELIMS= " %%A IN ('DATE/T') DO SET CDATE=%%B
FOR /F "TOKENS=1,2 eol=/ DELIMS=/ " %%A IN ('DATE/T') DO SET mm=%%B
FOR /F "TOKENS=1,2 DELIMS=/ eol=/" %%A IN ('echo %CDATE%') DO SET dd=%%B
FOR /F "TOKENS=2,3 DELIMS=/ " %%A IN ('echo %CDATE%') DO SET yyyy=%%B
FOR /F "TOKENS=1-2 delims=/:" %%a in ("%TIME%") DO SET mytime=%%a%%b
SET CURRENT_DATE_TIME=%yyyy%%mm%%dd%%mytime%
ECHO Version:%CURRENT_DATE_TIME%

dotnet build %~dp0\..\ApplicationInsights.Kubernetes.sln

:HELP
GOTO :EXIT

:EXIT
ENDLOCAL
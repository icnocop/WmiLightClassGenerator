@echo off
REM Regenerates the WMI wrapper classes in the Generated folder.
REM Run this script from the sample project directory.
dotnet run --project ..\..\src\WmiLightClassGenerator -- wmi-classes.json

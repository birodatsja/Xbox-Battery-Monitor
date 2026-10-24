@echo off
setlocal
cd /d "%~dp0"
title Building Xbox Battery Monitor

where dotnet >nul 2>nul
if errorlevel 1 goto nodotnet

rem Close a running copy so the old exe can be replaced
taskkill /f /im XboxBatteryMonitor.exe >nul 2>nul

echo Building Xbox Battery Monitor...
echo.
dotnet publish "XboxBatteryMonitor.csproj" -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o "%~dp0_publish"
if errorlevel 1 goto failed

if exist "%~dp0xbox battery" rmdir /s /q "%~dp0xbox battery"
mkdir "%~dp0xbox battery"
move /y "%~dp0_publish\XboxBatteryMonitor.exe" "%~dp0xbox battery\" >nul
if errorlevel 1 goto failed

rem Clean up temporary build folders
rmdir /s /q "%~dp0_publish" >nul 2>nul
rmdir /s /q "%~dp0bin" >nul 2>nul
rmdir /s /q "%~dp0obj" >nul 2>nul

echo.
echo Build finished.
echo Your app is in the "xbox battery" folder: XboxBatteryMonitor.exe
echo.
pause
exit /b 0

:nodotnet
echo.
echo The .NET 8 SDK was not found.
echo Install it from https://dotnet.microsoft.com/download/dotnet/8.0 and run this file again.
echo.
pause
exit /b 1

:failed
echo.
echo Build failed. Check the messages above.
echo.
pause
exit /b 1

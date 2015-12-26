@echo off
REM -------------------------------------------------------------------------
REM - Build script for IotWeb
REM -
REM - Rebuilds all projects and creates the NuGet package. Assumes Windows 8
REM - or better. The following tools must be on the path - Python, MSbuild
REM - and NuGet.
REM -------------------------------------------------------------------------

REM Make sure 'msbuild' is available
where msbuild.exe >nul 2>nul
if %errorlevel%==1 (
  @echo msbuild.exe not found in path.
  exit 1
  )

REM Make sure 'python' is available
where python.exe >nul 2>nul
if %errorlevel%==1 (
  @echo python.exe not found in path.
  exit 1
  )

REM Make sure 'nuget' is available
where nuget.exe >nul 2>nul
if %errorlevel%==1 (
  @echo nuget.exe not found in path.
  exit 1
  )

REM Update versions


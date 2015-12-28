@echo off
REM -------------------------------------------------------------------------
REM - Build script for IotWeb
REM -
REM - Rebuilds all projects and creates the NuGet package. Assumes Windows 8
REM - or better. The following tools must be on the path - Python, MSbuild
REM - and NuGet.
REM -------------------------------------------------------------------------

REM Get the solution directory
set OLDDIR=%CD%
set TOOLDIR=%~dp0
cd %TOOLDIR%..

REM Make sure 'msbuild' is available
where msbuild.exe >nul 2>nul
if %errorlevel%==1 (
  @echo msbuild.exe not found in path.
  goto done
  )

REM Make sure 'python' is available
where python.exe >nul 2>nul
if %errorlevel%==1 (
  @echo python.exe not found in path.
  goto done
  )

REM Make sure 'nuget' is available
if not exist %TOOLDIR%\nuget.exe (
  @echo nuget.exe is not in the tools directory
  goto done
  )

REM Update versions
@echo Updating versions
python %TOOLDIR%\bumpver.py

REM Clean and build a release target
@echo Cleaning
msbuild /t:Clean /p:Configuration=Release >%TOOLDIR%/clean.log
if not %errorlevel%==0 (
  @echo Clean failed. See 'clean.log' for details
  goto done
  )

@echo Building release version
msbuild /t:Build /p:Configuration=Release >%TOOLDIR%/build.log
if not %errorlevel%==0 (
  @echo Build failed. See 'build.log' for details
  goto done
  )

REM Make the NuGet package
@echo Building NuGet package
%TOOLDIR%/nuget.exe pack

REM Finish up
:done
cd %OLDDIR%

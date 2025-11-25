@echo off
setlocal

:: Settings
set PROJECT=LolTime.App
set RELEASE_DIR=Releases
set VERSION=1.0.0

:: Check for vpk
where vpk >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] Velopack CLI (vpk) is not found.
    echo Please install it using: dotnet tool install -g Velopack
    exit /b 1
)

echo [1/3] Cleaning...
dotnet clean %PROJECT%
if %errorlevel% neq 0 exit /b %errorlevel%

echo [2/3] Publishing...
dotnet publish %PROJECT% -c Release -r win-x64 -o publish --self-contained true
if %errorlevel% neq 0 exit /b %errorlevel%

echo [3/3] Packing with Velopack...
vpk pack -u LolTime -v %VERSION% -p publish -e LolTime.App.exe -o %RELEASE_DIR%
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [SUCCESS] Release created in %RELEASE_DIR% folder!
echo.
pause


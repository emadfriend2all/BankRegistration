@echo off
echo Building RegistrationPortal for IIS deployment...

echo Step 1: Building Angular client for production...
cd RegistrationPortal.Client
call npm run build -- --configuration production
if %ERRORLEVEL% NEQ 0 (
    echo Angular build failed!
    exit /b 1
)

echo Step 2: Building .NET server for production...
cd ..\RegistrationPortal.Server
dotnet publish -c Release -o ..\PublishedApp
if %ERRORLEVEL% NEQ 0 (
    echo .NET publish failed!
    exit /b 1
)

echo Step 3: Flattening browser folder structure...
cd ..\PublishedApp
if exist "wwwroot\browser" (
    echo Moving files from browser folder to wwwroot root...
    robocopy "wwwroot\browser" "wwwroot" /E /MOVE
    rmdir /s /q "wwwroot\browser"
    echo Browser folder flattened successfully
) else (
    echo No browser folder found - files are already in correct location
)

echo Build completed successfully!
echo Published application is in: PublishedApp folder
echo Check: PublishedApp\wwwroot\index.html should exist
pause

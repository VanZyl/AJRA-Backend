@echo off
setlocal enabledelayedexpansion

:: Ask for image name
set IMAGE_NAME=ajra-backend

:: Ask for version
set /p IMAGE_VERSION="Enter the version (e.g., v1.0): "

:: Set local registry IP and port
set REGISTRY_IP=localhost
set REGISTRY_PORT=6600

:: Full image name with tag
set IMAGE_TAG=%IMAGE_NAME%:%IMAGE_VERSION%
set REGISTRY_IMAGE=%REGISTRY_IP%:%REGISTRY_PORT%/%IMAGE_TAG%

echo Tagging image as %REGISTRY_IMAGE%...
docker tag %IMAGE_TAG% %REGISTRY_IMAGE%

echo Pushing image to local registry...
docker push %REGISTRY_IMAGE%

echo Push completed successfully!
pause

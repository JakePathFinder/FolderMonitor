@echo off
ECHO. Loading variables from .env
for /f "delims=" %%a in (.env) do set "%%a"

@echo on
mkdir -f ./output
dotnet dev-certs https -ep ./output/varonisdevcert.pfx -p %CERTIFICATE_PWD% --trust --verbose
timeout 30
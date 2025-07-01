@echo off
echo Compilando proyectos...

cd PdfReaderService.Api
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo Error compilando API
    pause
    exit /b 1
)
cd ..

cd PdfReaderService.WindowsService
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output bin\Release\net8.0-windows\win-x64\publish\
if %errorlevel% neq 0 (
    echo Error compilando Servicio de Windows
    pause
    exit /b 1
)
cd ..

echo CompilaciÃ³n completada exitosamente!
pause

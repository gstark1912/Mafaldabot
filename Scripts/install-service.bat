@echo off
echo Instalando servicio PDF Reader...

sc create "PdfReaderService" binPath= "%~dp0..\PdfReaderService.WindowsService\bin\Release\net8.0-windows\win-x64\publish\PdfReaderService.WindowsService.exe" start= auto DisplayName= "PDF Reader Daily Service"

if %errorlevel% == 0 (
    echo Servicio instalado correctamente.
    echo Iniciando servicio...
    sc start "PdfReaderService"
    if %errorlevel% == 0 (
        echo Servicio iniciado correctamente.
    ) else (
        echo Error al iniciar el servicio.
    )
) else (
    echo Error al instalar el servicio.
)

pause

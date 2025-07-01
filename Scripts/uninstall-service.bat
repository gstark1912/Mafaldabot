@echo off
echo Deteniendo y desinstalando servicio PDF Reader...

sc stop "PdfReaderService"
sc delete "PdfReaderService"

if %errorlevel% == 0 (
    echo Servicio desinstalado correctamente.
) else (
    echo Error al desinstalar el servicio.
)

pause

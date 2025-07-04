@echo off
echo Registrando aplicacion en el inicio de Windows...

set "appPath=%~dp0PdfReaderScheduler.exe"
set "startupFolder=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup"
set "shortcutPath=%startupFolder%\PdfReaderScheduler.lnk"

powershell -Command "& {$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%shortcutPath%'); $Shortcut.TargetPath = '%appPath%'; $Shortcut.WorkingDirectory = '%~dp0'; $Shortcut.Save()}"

if %errorlevel% equ 0 (
    echo Aplicacion registrada exitosamente en el inicio de Windows
    echo Acceso directo creado en: %shortcutPath%
) else (
    echo Error al registrar la aplicacion
)

echo.
echo Presiona cualquier tecla para continuar...
pause > nul
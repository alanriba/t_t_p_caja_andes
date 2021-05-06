::
::Instalación del servicio e Inicialización
::
@echo off
cls
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo :::::::::::::INSTALANDO Y LEVANTANDO SERVER PIPE ::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo.
TIMEOUT /t 5 /NOBREAK
::Inicio los servicios
net start PipeComponent
echo ::::Proceso Finalizado:::
if ERRORLEVEL 1 goto error
echo Procesos Finalizados. [Creado por: Rodrigo Rojas S.]
exit
:error
echo Ha ocurrido un problema
pause
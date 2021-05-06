::
::Desinstalación del servicio
::
@echo off
cls
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo :::::::::::::INSTALANDO Y LEVANTANDO SERVER PIPE ::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo.
::Desinstalación del servicio
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /u %~dp0PipeComponent.exe
echo ::::Proceso Finalizado:::
if ERRORLEVEL 1 goto error
echo Procesos Finalizados. [Creado por: Rodrigo Rojas S.]
exit
:error
echo Ha ocurrido un problema
pause
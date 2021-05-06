@echo off 
::Dejar todo en utf-8 -> chcp 65001 
chcp 65001 
title Servicio Pipe

:inicio
color F0 
cls
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo ::::::::::::::::::::::::::SERVICE PIPE:::::::::::::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
echo.
echo [1] Instalar
echo [2] Desinstalar
echo [3] Iniciar Servicio
echo [4] Modo Consola
echo [5] Salir
echo.

set /p var=Seleccione una opcion [1-5]: 
if "%var%"=="1" goto op1
if "%var%"=="2" goto op2
if "%var%"=="3" goto op3
if "%var%"=="4" goto op4
if "%var%"=="5" goto salir

::Mensaje de error, validación cuando se selecciona una opción fuera de rango
echo. El numero "%var%" no es una opcion valida, por favor intente de nuevo.
echo.
pause
echo.
goto inicio

:op1
    echo.
    echo. Instalación del servicio
    echo.
        ::Comando Opcion 1
        color F0
        START /b install.cmd
    echo.
    pause 
    goto inicio

:op2
    echo.
    echo. Desinstalación del servicio
    echo.
        ::Comando Opcion 2
        color F0
        START /b uninstall.cmd
    echo.
    pause
    goto inicio

:op3
    echo.
    echo. Inicialización del servicio
    echo.
        ::Comando Opcion 3
        color F0
        START /b service.cmd
    echo.
    pause
    goto inicio

:op4
    echo.
    echo. Modo Consola
    echo.
        ::Comando Opcion 4
        color F0
        START PipeComponent.exe -console
    echo.
    pause
    goto inicio

:salir
    exit
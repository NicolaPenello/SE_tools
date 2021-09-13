@echo off

call crc %1%

if "%ERRORLEVEL%" == "0" goto good

:fail
    echo Execution Failed
    echo return value = %ERRORLEVEL%
    goto end

:good
    echo Execution succeeded
    echo Return value = %ERRORLEVEL%
    goto end

:end

::pause.
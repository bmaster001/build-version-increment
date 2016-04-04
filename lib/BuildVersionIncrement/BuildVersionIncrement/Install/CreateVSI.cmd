@echo off

for /f %%t in ('dir VS* /A:D /B') do call :CREATEVSI %%t

GOTO EXIT

:CREATEVSI

SET TARGETVSI=BuildVersionIncrement_%1.VSI
@echo Creating %TARGETVSI% ...

if exist %TARGETVSI% del %TARGETVSI%

pushd %1
..\zip ..\%TARGETVSI% *.*
popd
zip %TARGETVSI% *.dll

GOTO EXIT

:EXIT
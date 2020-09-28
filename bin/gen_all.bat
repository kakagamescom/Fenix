@echo off

set cwdpath=%~dp0
set curpath=%cwdpath%..

set MPCBIN=%cwdpath%/mpc/win/mpc.exe

set CLIENT_PATH=%curpath%/src/Fenix.Gen/bin/Debug/netcoreapp3.1/Fenix.Gen.exe

echo %curpath%/src/Client.App/Gen
echo %curpath%/src/Server.App/Gen

rd /s /Q "%curpath%\src\Client.App\Gen"
rd /s /Q "%curpath%\src\Server.App\Gen"
 
%CLIENT_PATH% -r %curpath%
%CLIENT_PATH% -c %curpath%
%CLIENT_PATH% -s %curpath% 

%MPCBIN% -i %curpath%\src\Client.App\Gen\Message\ -o %curpath%\src\Client.App\Gen\Message\Generated\ClientAppMsg.g.cs -r ClientAppResolver
%MPCBIN% -i %curpath%\src\Fenix.Runtime\Common\ -o %curpath%\src\Client.App\Gen\Message\Generated\FenixRuntimeMsg.g.cs -r FenixRuntimeResolver
%MPCBIN% -i %curpath%\src\Shared\ -o %curpath%\src\Client.App\Gen\Message\Generated\SharedMsg.g.cs -r SharedResolver

%MPCBIN% -i %curpath%\src\Client.App\Gen\Message\ -o %curpath%\src\Server.App\Gen\Message\Generated\ClientAppMsg.g.cs -r ClientAppResolver
%MPCBIN% -i %curpath%\src\Fenix.Runtime\Common\ -o %curpath%\src\Server.App\Gen\Message\Generated\FenixRuntimeMsg.g.cs -r FenixRuntimeResolver
%MPCBIN% -i %curpath%\src\Shared\ -o %curpath%\src\Server.App\Gen\Message\Generated\SharedMsg.g.cs -r SharedResolver


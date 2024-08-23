@echo off
SET SIGNTOOL="C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe"
SET HASH_ALGORITHM=/sha1 7e4d5aa0e0ddedef2955d608543da951b769d715
SET TIMESTAMP_SERVER=/tr http://timestamp.sectigo.com
SET TIMESTAMP_ALGORITHM=/td sha256
SET FILE_DIGEST_ALGORITHM=/fd sha256
SET CERTIFICATE_NAME=/n "ACCO Engineered System, Inc"

echo Please enter the path of the File to sign:
echo (Paste the path with quotes if it contains spaces) 
echo Example: "C:\Visual Studio Files\Signed_MSIs\FileToSign.msi"
echo.
set /p FILE_TO_SIGN=^>_:

%SIGNTOOL% sign %HASH_ALGORITHM% %TIMESTAMP_SERVER% %TIMESTAMP_ALGORITHM% %FILE_DIGEST_ALGORITHM% %CERTIFICATE_NAME% %FILE_TO_SIGN%


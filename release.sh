cd LongdoCardsPOS/bin/Release
/c/Program\ Files/7-Zip/7z a -tzip LongdoCardsPOS.zip LongdoCardsPOS.exe QRCoder.dll WpfControls.dll
scp -P 1222 LongdoCardsPOS.zip tanawat@secure.longdo.com:/home/tanawat/deploy/LongdoCardsPOS.zip
rm LongdoCardsPOS.zip

tar -czf LongdoCardsPOS.tgz -C LongdoCardsPOS/bin/Release LongdoCardsPOS.exe QRCoder.dll WpfControls.dll
scp -P 1222 LongdoCardsPOS.tgz tanawat@secure.longdo.com:/home/tanawat/deploy/LongdoCardsPOS.tgz
rm LongdoCardsPOS.tgz


# install vtunnel as a Windows Service

sc create vtunnel binpath= "c:\step\bond\vtunnel-x86_64-w64-mingw32.exe --dst-port 22 --background" start= auto
sc failure vtunnel reset= 1 actions= restart/1000
sc start vtunnel

# install Bond as a Windows Service

sc create bond binpath= "c:\step\bond\bond.exe" start= auto
sc failure bond reset= 1 actions= restart/1000
sc start bond
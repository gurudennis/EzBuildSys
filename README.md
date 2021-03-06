# EzBuildSys
A Windows-based build system that doesn't suck.

##### EZB.Build.exe
```
Parameters:
    /help            Show this information
    /verb            Build (default), Rebuild or Clean
    /profile         Path to the *.ezb profile. Can be positional (1st argument)
    /interactive     Interactive mode (requires some input)

Examples:
    EZB.Build /verb Clean /profile MyProfile.ezb /interactive
    EZB.Build MyProfile.ezb
```

##### EZB.PackClient.exe
```
Parameters:
    /help            Show this information
    /verb            Pack, Unpack
    /rootPath        Optional root directory
    /pathIn          Input path
    /pathOut         Output path
    /name            Package name
    /version         Package version
    /interactive     Interactive mode (requires some input)

Examples:
    EZB.PackClient /verb Pack /pathIn c:\temp\MyPackageRoot
                   /pathOut c:\temp\{PackageName}_{PackageVersion}.zip
                   /name MyPackage /version 1.2.3.4
    EZB.PackClient /verb Download /pathOut c:\temp\{PackageName}_{PackageVersion}
                   /serverURI http://myserver:8710 /name MyPackage /version latest
```

##### EZB.PackServer.exe
```
Parameters:
    /help            Show this information
    /root            Path to the server storage root. Can be positional (1st argument)
    /iface           Interface on which to listen for requests. Defaults to all.
    /port            Port on which to listen for requests. Defaults to 8710.

Examples:
    EZB.PackServer c:\packageroot
    EZB.PackServer /root c:\packageroot /iface localhost /port 80
```

#### (under development - documentation and more features coming).

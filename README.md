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
Examples:");
    EZB.PackClient /verb Pack /pathIn c:\temp\MyPackageRoot /pathOut c:\temp\MyPackage
```

###### (under development - documentation and more features coming).

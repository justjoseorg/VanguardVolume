#define MyAppName "Vanguard Volume"
#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif
#define MyAppExeName "VanguardVolume.App.exe"

[Setup]
AppId={{7D1D405A-BC25-49F5-9D3A-9CAB8FEC78C6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
SetupIconFile=..\VanguardVolume.App\Assets\vanguard-volume.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
DefaultDirName={localappdata}\Programs\VanguardVolume
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\artifacts\installer
OutputBaseFilename=VanguardVolume-Setup
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma2/ultra64
SolidCompression=yes

[Files]
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Parameters: "--settings"
Name: "{autoprograms}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

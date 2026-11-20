
#define MyAppName "Ryum Control"
#define MyAppVersion "1.0.0"
#define MyAppExeName "GestionApp.exe"

[Setup]
AppId=RyumControl
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}

DefaultDirName={localappdata}\Programs\Ryum Control
DefaultGroupName=Ryum Control

PrivilegesRequired=lowest

SetupIconFile=..\Assets\RyumControl.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

OutputDir=..\artifacts\installer
OutputBaseFilename=RyumControl-Setup-{#MyAppVersion}

Compression=lzma2
SolidCompression=yes
WizardStyle=modern

CloseApplications=yes

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos:"; Flags: unchecked

[Files]
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Ryum Control"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\Ryum Control"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Iniciar Ryum Control"; Flags: nowait postinstall skipifsilent

; 脚本用 Inno Setup 脚本向导生成。
; 查阅文档获取创建 INNO SETUP 脚本文件详细资料!

[Setup]
AppName=国标协议测试-现场端
AppVerName=国标协议测试-现场端 1.0
AppPublisher=CSoft
AppPublisherURL=宇宙联盟
AppSupportURL=宇宙联盟
AppUpdatesURL=宇宙联盟
DefaultDirName={pf}\国标协议测试-现场端
DefaultGroupName=国标协议测试-现场端
AllowNoIcons=yes
Compression=lzma
SolidCompression=yes

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\caoyi\Desktop\GbTest\GbTest\bin\Release\net8.0-windows\publish\win-x86\GbTest.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\caoyi\Desktop\GbTest\GbTest\bin\Release\net8.0-windows\publish\win-x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
; 注意: 不要在任何共享系统文件中使用“Flags: ignoreversion”

[Icons]
Name: "{group}\国标协议测试-现场端"; Filename: "{app}\GbTest.exe"
Name: "{group}\{cm:UninstallProgram,国标协议测试-现场端}"; Filename: "{uninstallexe}"
Name: "{userdesktop}\国标协议测试-现场端"; Filename: "{app}\GbTest.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\国标协议测试-现场端"; Filename: "{app}\GbTest.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\GbTest.exe"; Description: "{cm:LaunchProgram,国标协议测试-现场端}"; Flags: nowait postinstall skipifsilent


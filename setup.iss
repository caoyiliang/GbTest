; �ű��� Inno Setup �ű������ɡ�
; �����ĵ���ȡ���� INNO SETUP �ű��ļ���ϸ����!

[Setup]
AppName=����Э�����-�ֳ���
AppVerName=����Э�����-�ֳ��� 1.0
AppPublisher=CSoft
AppPublisherURL=��������
AppSupportURL=��������
AppUpdatesURL=��������
DefaultDirName={pf}\����Э�����-�ֳ���
DefaultGroupName=����Э�����-�ֳ���
AllowNoIcons=yes
Compression=lzma
SolidCompression=yes

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\caoyi\Desktop\GbTest\GbTest\bin\Release\net8.0-windows\publish\win-x86\GbTest.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\caoyi\Desktop\GbTest\GbTest\bin\Release\net8.0-windows\publish\win-x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
; ע��: ��Ҫ���κι���ϵͳ�ļ���ʹ�á�Flags: ignoreversion��

[Icons]
Name: "{group}\����Э�����-�ֳ���"; Filename: "{app}\GbTest.exe"
Name: "{group}\{cm:UninstallProgram,����Э�����-�ֳ���}"; Filename: "{uninstallexe}"
Name: "{userdesktop}\����Э�����-�ֳ���"; Filename: "{app}\GbTest.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\����Э�����-�ֳ���"; Filename: "{app}\GbTest.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\GbTest.exe"; Description: "{cm:LaunchProgram,����Э�����-�ֳ���}"; Flags: nowait postinstall skipifsilent


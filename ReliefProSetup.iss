; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "ReliefPro"
#define MyAppVersion "1.0"
#define MyAppPublisher "SIMTECH, Inc."
#define MyAppURL "http://www.simtech.com/"
#define MyAppExeName "ReliefProMain.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{5E15850F-CD67-42A2-A864-C45845CF641D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=Setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "E:\ReliefProCode\trunk\ReliefPro\ReliefProMain\bin\Debug\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "E:\ReliefProCode\trunk\ReliefPro\ReliefProMain\bin\Debug\dotNetFx40_Full_x86_x64.exe"; DestDir: "{app}"; Flags: ignoreversion
;mch regedit
Source: "E:\ReliefProCode\trunk\ReliefPro\ReliefProMain\bin\Debug\P2Wrap91.dll"; DestDir: "{sys}"; CopyMode: alwaysskipifsameorolder; Flags: restartreplace regserver
Source: "E:\ReliefProCode\trunk\ReliefPro\ReliefProMain\bin\Debug\P2Wrap92.dll"; DestDir: "{sys}"; CopyMode: alwaysskipifsameorolder; Flags: restartreplace regserver
;Source: "DLL文件所在路径"; DestDir: "{app}"; Flags:ignoreversion regserver

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[code]
//function IsWin64: Boolean; 

function CheckProII():boolean; 
var 
b1:boolean;
b2:boolean;
begin 
b1:= Not RegKeyExists(HKLM, 'HKEY_LOCAL_MACHINE\SOFTWARE\SIMSCI\PRO/II\9.1'); 
b2:= Not RegKeyExists(HKLM, 'HKEY_LOCAL_MACHINE\SOFTWARE\SIMSCI\PRO/II\9.2'); 
Result:=b1 or b2;
end;
 
function CheckDotNet4_0():boolean; 
begin 
Result:=not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\.NETFramework\v4.0.30319'); 
end; 

function CheckVisio():boolean; 
begin 
Result:=not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\visio.exe'); 
end; 

function CheckVisioVersion():boolean; 
var 
v:string; 
b:boolean;  
p:integer;    
begin
b:=RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\visio.exe','Path',v); 
  if b then
  begin
       p:=pos('Office14',v); 
       if p>0 then
       begin    
       Result:=TRUE;
       end
       else
       begin
       Result:=FALSE;
       end;
  end;
end; 


function CheckExcel():boolean; 
begin 
Result:=not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\excel.exe'); 
end; 

function InitializeSetup(): Boolean;
var
     ResultCode:integer; 
     r1:boolean;
     r2:boolean;
     r3:boolean;
     r4:boolean;
begin
r1:=TRUE;
r2:=TRUE;
r3:=TRUE; 
r4:=TRUE; 	
     if CheckProII()=FALSE   then
     begin
           MsgBox('Any ProII is not installed,please install it first！',mbInformation,MB_OK); 
           r1 := FALSE;
     end;
    if CheckVisio() then 
    begin           
         MsgBox('Microsoft visio is not installed,please install it first！',mbInformation,MB_OK); 
         CheckVisioVersion();
         r2 := FALSE; 
    end;
                          
    if CheckExcel() then 
    begin 
         MsgBox('Microsoft Excel is not installed,please install it first！',mbInformation,MB_OK); 
         r3 := FALSE; 
    end; 
    if CheckDotNet4_0() then                                    
    begin 
        ExtractTemporaryFile('dotNetFx40_Full_x86_x64.exe'); 
        Exec(ExpandConstant('{tmp}\dotNetFx40_Full_x86_x64.exe'), '', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode); 
        r4 := TRUE;
    end; 
    Result:=r1 and r2 and r3 and r4;
end;


[registry]
;本段处理程序在注册表中的键值
Root:HKLM;Subkey:SOFTWARE\Microsoft\Windows\CurrentVersion\Run;ValueType: string; ValueName:TEST;ValueData:{app}\MyProg.exe;Flags: uninsdeletevalue


[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


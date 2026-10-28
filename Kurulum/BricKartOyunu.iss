; ─────────────────────────────────────────────────────────────────────────────
; BricKartOyunu kurulum betiği (Inno Setup 6)
; Derleme:  KurulumuOlustur.ps1  (önce Release derler, sonra bu betiği ISCC ile derler)
; Çıktı:    Kurulum\Cikti\BricKartOyunu-Kurulum-<sürüm>.exe
; ─────────────────────────────────────────────────────────────────────────────
#define MyAppName "Briç Kart Oyunu"
; Klasör ve kurulum dosyası adlarında Türkçe karakter/boşluk olmasın diye ayrı bir kimlik:
#define MyAppId "BricKartOyunu"
#define MyAppExe "BricKartOyunu.exe"
#define MyAppPublisher "BricKartOyunu"
#define MyAppVersion GetVersionNumbersString("..\bin\Release\BricKartOyunu.exe")

[Setup]
; Sabit kimlik: aynı kimlikle çıkan yeni kurulum eskisinin üzerine yükler (ayarlar korunur)
AppId={{6F1D4B52-3A7E-4C0B-9E58-2B7D1C9A41E3}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion} (Test Sürümü)
AppPublisher={#MyAppPublisher}
#ifexist "BricKartOyunu.ico"
SetupIconFile=BricKartOyunu.ico
#endif
; Kullanıcı bazlı kurulum: yönetici izni gerekmez. Uygulama seedler.txt gibi dosyaları kendi klasörüne yazdığı için
; Program Files yerine kullanıcının yazabildiği klasöre ({localappdata}\Programs) kurulur.
PrivilegesRequired=lowest
DefaultDirName={autopf}\{#MyAppId}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExe}
OutputDir=Cikti
OutputBaseFilename={#MyAppId}-Kurulum-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Masaüstüne kısayol oluştur"; GroupDescription: "Ek görevler:"

[Files]
; Hata ayıklama/yardım dosyaları ve kullanıcıya ait veri dosyaları pakete girmez (güncellemede kullanıcı verisi ezilmez)
Source: "..\bin\Release\*"; DestDir: "{app}"; Excludes: "*.pdb,*.xml,*.vshost.*,seedler.txt,ortaklik_anlasmasi.json"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "TesterRehberi.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"
Name: "{group}\Test Rehberi"; Filename: "{app}\TesterRehberi.txt"
Name: "{group}\{#MyAppName} Kaldır"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\TesterRehberi.txt"; Description: "Test rehberini oku (önerilir)"; Flags: postinstall shellexec skipifsilent
Filename: "{app}\{#MyAppExe}"; Description: "{#MyAppName} uygulamasını başlat"; Flags: nowait postinstall skipifsilent

[Code]
// .NET Framework 4.8 (Release >= 528040) gerekli. Windows 10 (1903+) ve Windows 11'de hazır gelir.
function NetFramework48Yuklu: Boolean;
var
  Surum: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Surum)
            and (Surum >= 528040);
end;

function InitializeSetup: Boolean;
var
  Hata: Integer;
begin
  Result := True;
  if not NetFramework48Yuklu then
  begin
    if MsgBox('Bu uygulama .NET Framework 4.8 gerektirir ve bilgisayarınızda bulunamadı.' + #13#10 +
              'Microsoft indirme sayfası açılsın mı? (Kurulduktan sonra bu kurulumu tekrar çalıştırın.)',
              mbConfirmation, MB_YESNO) = IDYES then
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet-framework/net48', '', '', SW_SHOWNORMAL, ewNoWait, Hata);
    Result := False;
  end;
end;

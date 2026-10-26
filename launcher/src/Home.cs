using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.Applications;

public sealed class QuietHome : NUIApplication
{
 static readonly string Root=System.IO.Path.GetFullPath(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"..","res","quiet-home"))+"/";
 const string StateFile="/tmp/quiet-home-state.txt";
 const string ConfigPath="/opt/usr/home/owner/data/quiet-home-config.json";
 const string AppsDataPath="/opt/usr/home/owner/data/quiet-home-apps.json";

 sealed class Slot { public View Card,Shadow,FocusRing; public TextLabel Label,Desc; public string Name,Action,Description; public int Row,X,Y,W,H; }
 sealed class CachedArtwork { public ImageView Image; public View Parent; }

 readonly List<Slot> slots=new List<Slot>();
 readonly List<View> scene=new List<View>();
 readonly Dictionary<string,CachedArtwork> artworkCache=new Dictionary<string,CachedArtwork>();
 static readonly int ProcessId = Process.GetCurrentProcess().Id;
 readonly Dictionary<string,ImageView> featureCache=new Dictionary<string,ImageView>();
 readonly Dictionary<string,ImageView> quickCache=new Dictionary<string,ImageView>();
 readonly Dictionary<int,List<int>> rowCache=new Dictionary<int,List<int>>();
 readonly List<int> emptyRow=new List<int>();
 int lastFocusedSlot = -1;
 int artworkReadyCount = 0;
 View artworkPreloadRoot;
 int artworkViewsCreated,pageArtworkCold;
 List<LibraryApp> library=new List<LibraryApp>();
 bool reordering;
 bool waitingForLongPress;
 bool longPressTriggered;
 Tizen.NUI.Timer longPressTimer;
 readonly Color reorderColor = new Color(1.0f, 0.78f, 0.15f, 1.0f);
 int libraryPage;
 string libraryError="";
 TextLabel clock, notice, detail;
 TextLabel[] tabs;
 TextLabel toolbarHint;
 Animation focusAnimation;
 View pageContainer;
 Animation pageAnimation;
 bool fontLoaded;
 Tizen.NUI.Timer clockTimer;
 int page, selected, area=1, lastSource=13;
 string editing="";
 long buildMilliseconds;
 int currentVolume, currentMute;
 readonly Dictionary<string,string> settingValues=new Dictionary<string,string>();
 readonly Dictionary<string,string> settingNames=new Dictionary<string,string>();
 readonly Dictionary<string,bool> settingEnabled=new Dictionary<string,bool>();
 double brightnessMin,brightnessMax=50,brightnessStep=1;
 string lastKey="";
 long lastKeyAt;
 bool darkMode=true;
 bool soundEnabled=true;
 bool animationsEnabled=true;

 static readonly string[] LanguageCodes = Localization.LanguageCodes;
 static readonly string[] LanguageNames = Localization.LanguageNames;
 string CurrentLangCode => (currentLang >= 0 && currentLang < LanguageCodes.Length) ? LanguageCodes[currentLang] : "es";
 int currentLang = 0;

 string[] hdmiNames = new[] { "PlayStation 4 / PC", "Nintendo Switch", "HDMI 3" };
 string[] hdmiDescs = new[] { "Console or PC · HDMI 1", "Game console · HDMI 2", "External device · HDMI 3" };

 static readonly (string name, string presetKey, string heroAsset)[] HdmiPresets = new[] {
  ("PlayStation 4 / PC", "preset_console_pc", "feature-ps4.png"),
  ("PlayStation 5", "preset_game_console", "feature-ps5.svg"),
  ("Nintendo Switch", "preset_game_console", "feature-switch-hero.svg"),
  ("Xbox Series X / S", "preset_game_console", "feature-xbox.svg"),
  ("Gaming PC", "preset_pc", "feature-pc.svg"),
  ("Chromecast / Fire TV", "preset_streaming", "feature-tvbox.svg"),
  ("Blu-ray / DVD", "preset_player", "feature-bluray.svg"),
  ("Cable TV / Box", "preset_tv_box", "feature-tvbox.svg"),
  ("HDMI Device", "preset_device", "feature-hdmi-hero.svg")
 };
 int[] hdmiPresetIndex = new[] { 0, 2, 8 };

 string GetHeroAsset(int index)
 {
  int pIdx = (index >= 0 && index < hdmiPresetIndex.Length) ? hdmiPresetIndex[index] : 0;
  if(pIdx >= 0 && pIdx < HdmiPresets.Length){
   return HdmiPresets[pIdx].heroAsset;
  }
  return "feature-ps4.png";
 }

 string GetHdmiDesc(int index)
 {
  int pIdx = (index >= 0 && index < hdmiPresetIndex.Length) ? hdmiPresetIndex[index] : 0;
  if(pIdx >= 0 && pIdx < HdmiPresets.Length){
   string key = HdmiPresets[pIdx].presetKey;
   return $"{T(key)} · HDMI {index + 1}";
  }
  return $"HDMI {index + 1}";
 }

 CultureInfo GetCurrentCulture() => Localization.GetCulture(CurrentLangCode);
 string GetFormattedDate() => Localization.FormatDate(DateTime.Now, CurrentLangCode);


 // Launcher Settings Modal state
 View modalOverlay;
 int modalRow = 0;
 TextLabel[] modalRowLabels;
 TextLabel[] modalRowValues;
 View[] modalRowViews;

 string T(string key) => Localization.T(key, CurrentLangCode);


 Color ink => darkMode ? new Color(0.98f,0.98f,1.0f,1) : new Color(0.114f,0.114f,0.122f,1);
 Color muted => darkMode ? new Color(0.65f,0.70f,0.80f,1) : new Color(0.431f,0.431f,0.451f,1);
 readonly Color white=new Color(1,1,1,1);
 Color accent => darkMode ? new Color(0.40f,0.60f,1.0f,1) : new Color(0.24f,0.29f,0.72f,1);
 Color bg => darkMode ? new Color(0.082f,0.110f,0.184f,1) : new Color(0.96f,0.96f,0.97f,1);
 Color captionMuted => darkMode ? new Color(0.78f,0.83f,0.92f,1) : new Color(0.28f,0.32f,0.40f,1);
 Color descMuted => darkMode ? new Color(0.62f,0.68f,0.80f,1) : new Color(0.42f,0.46f,0.54f,1);


 protected override void OnCreate()
 {
  base.OnCreate();
  NativeCapabilities.Initialize();
  DebugLog.OnErrorLogged += (msg) => ShowError(msg);
  try{
   fontLoaded=FontClient.Instance.AddCustomFontDirectory(Root+"fonts");
   FontClient.Instance.AddCustomFontDirectory("/usr/share/fonts");
  }catch(Exception ex){
   fontLoaded=false;
   DebugLog.Warn("Font", "FontClient failed loading fonts", ex);
  }
  LoadConfig();
  try{
   AtomicFileWriter.CleanOrphanTmp(AppsDataPath);
   if(File.Exists(AppsDataPath))
    library=AppLibrary.Load(AppsDataPath);
   else
    library=AppLibrary.Load(Root+"apps.json");
  }catch(Exception ex){
   DebugLog.Warn("Library", "Primary apps load failed, trying bundle fallback", ex);
   try{
    library=AppLibrary.Load(Root+"apps.json");
   }catch(Exception ex2){
    libraryError=ex2.Message;
    DebugLog.Error("Library", "Fallback apps load failed", ex2);
    ShowError("Apps error: " + ex2.Message);
   }
  }
  Window.Default.BackgroundColor=bg;
  Window.Default.Title="Opt-quiet-tizen";Window.Default.KeyEvent+=OnKey;
  PreloadArtwork();
  RefreshSource();selected=SourceIndex();BuildPage();
  clockTimer=new Tizen.NUI.Timer(30000);
  clockTimer.Tick+=(s,e)=>{if(clock!=null)clock.Text=GetFormattedDate();return true;};clockTimer.Start();WriteState("created");
 }

 void LoadConfig()
 {
  try{
   var cfg = SettingsStore.Load(ConfigPath, Root + "config.json");
   if (!string.IsNullOrEmpty(cfg.Lang)) {
    int idx = Array.IndexOf(LanguageCodes, cfg.Lang);
    if (idx >= 0) currentLang = idx;
   }
   soundEnabled = cfg.Sound;
   animationsEnabled = cfg.Animations;
   darkMode = cfg.DarkMode;
   hdmiPresetIndex[0] = Math.Clamp(cfg.Preset0, 0, HdmiPresets.Length - 1);
   hdmiPresetIndex[1] = Math.Clamp(cfg.Preset1, 0, HdmiPresets.Length - 1);
   hdmiPresetIndex[2] = Math.Clamp(cfg.Preset2, 0, HdmiPresets.Length - 1);
   for(int h=0; h<3; h++) {
    hdmiNames[h] = HdmiPresets[hdmiPresetIndex[h]].name;
    hdmiDescs[h] = GetHdmiDesc(h);
   }
   if (!string.IsNullOrEmpty(cfg.Hdmi1Name)) hdmiNames[0] = cfg.Hdmi1Name;
   if (!string.IsNullOrEmpty(cfg.Hdmi1Desc)) hdmiDescs[0] = cfg.Hdmi1Desc;
   if (!string.IsNullOrEmpty(cfg.Hdmi2Name)) hdmiNames[1] = cfg.Hdmi2Name;
   if (!string.IsNullOrEmpty(cfg.Hdmi2Desc)) hdmiDescs[1] = cfg.Hdmi2Desc;
   if (!string.IsNullOrEmpty(cfg.Hdmi3Name)) hdmiNames[2] = cfg.Hdmi3Name;
   if (!string.IsNullOrEmpty(cfg.Hdmi3Desc)) hdmiDescs[2] = cfg.Hdmi3Desc;
  }catch(Exception ex){DebugLog.Warn("Config", "LoadConfig failed", ex);}
 }

 void SaveConfig()
 {
  try{
   var cfg = new LauncherSettings {
    Lang = (currentLang >= 0 && currentLang < LanguageCodes.Length) ? LanguageCodes[currentLang] : "es",
    Sound = soundEnabled,
    Animations = animationsEnabled,
    DarkMode = darkMode,
    Preset0 = hdmiPresetIndex[0],
    Preset1 = hdmiPresetIndex[1],
    Preset2 = hdmiPresetIndex[2],
    Hdmi1Name = hdmiNames[0],
    Hdmi1Desc = hdmiDescs[0],
    Hdmi2Name = hdmiNames[1],
    Hdmi2Desc = hdmiDescs[1],
    Hdmi3Name = hdmiNames[2],
    Hdmi3Desc = hdmiDescs[2]
   };
   SettingsStore.Save(cfg, ConfigPath);
  }catch(Exception ex){DebugLog.Error("Config", "SaveConfig failed", ex);ShowError("Failed to save launcher settings.");}
 }

 protected override void OnResume()
 {
  base.OnResume();
  try{
   Window.Default.Raise();
   clockTimer?.Start();
   RefreshSource();
   CancelLongPress();
   longPressTriggered=false;
   waitingForLongPress=false;
   lastKey="";
   lastKeyAt=0;
   if(tabs!=null){
    if(page==0){
     area=1;
     selected=SourceIndex();
     UpdateFocus(false);
    }else if(page==2){
     RefreshSettings();
     UpdateFocus(false);
    }
   }
   WriteState("resumed");
  }catch(Exception ex){
   DebugLog.Error("Lifecycle", "OnResume exception", ex);
  }
 }

 protected override void OnAppControlReceived(AppControlReceivedEventArgs e)
 {
  base.OnAppControlReceived(e);
  try{
   string requested=null;
   if(e!=null&&e.ReceivedAppControl!=null&&e.ReceivedAppControl.ExtraData!=null){
    e.ReceivedAppControl.ExtraData.TryGet("quiet_page",out requested);
   }
   if(requested=="settings")OpenSettings("settings_request");
   else if(requested=="apps"){page=1;area=1;selected=0;BuildPage(true);}
   else Home("home_request");
  }catch(Exception ex){
   DebugLog.Error("AppControl", "OnAppControlReceived exception", ex);
   Home("home_request");
  }
 }

 protected override void OnPause()
 {
  base.OnPause();
  try{
   CancelLongPress();
   longPressTriggered=false;
   waitingForLongPress=false;
   if(reordering)StopReordering();
   editing="";
   lastKey="";
   lastKeyAt=0;
   clockTimer?.Stop();
   WriteState("paused");
  }catch(Exception ex){
   DebugLog.Error("Lifecycle", "OnPause exception", ex);
  }
 }

 void Home(string activity)
 {
  CancelLongPress();
  longPressTriggered=false;
  waitingForLongPress=false;
  lastKey="";
  lastKeyAt=0;
  if(reordering)StopReordering();
  clockTimer?.Start();
  RefreshSource();
  if(tabs!=null){
   if(page==0 && scene.Count>0 && modalOverlay==null){
    // Do NOT destroy and recreate the whole scene!
    // Prevents the 2-frame zoom-in / zoom-out glitch when returning from an app!
    area=1;
    selected=SourceIndex();
    UpdateFocus(false);
    WriteState(activity);
    return;
   }
   page=0;area=1;editing="";selected=SourceIndex();
   BuildPage(false);
   WriteState(activity);
  }
 }

 void OpenSettings(string activity)
 {
  CancelLongPress();
  if(reordering)StopReordering();
  clockTimer?.Start();
  page=2;area=1;selected=0;editing="";
  BuildPage(true);
  WriteState(activity);
 }

 void RefreshSource(){try{int n;if(NativeCapabilities.GetVconfInt("memory/system/source_type",out n)&&n>=13&&n<=16)lastSource=n;}catch(Exception ex){DebugLog.Warn("Source", "RefreshSource failed", ex);}}
 int SourceIndex(){return lastSource==14?1:lastSource==15?2:0;}

 int ScreenWidth => (Window.Default.WindowSize.Width > 0) ? Window.Default.WindowSize.Width : 1920;
 int ScreenHeight => (Window.Default.WindowSize.Height > 0) ? Window.Default.WindowSize.Height : 1080;
 float ScaleX => ScreenWidth / 1920f;
 float ScaleY => ScreenHeight / 1080f;
 float ScaleFont => Math.Min(ScaleX, ScaleY);

 int SclX(int x) => (int)Math.Round(x * ScaleX);
 int SclY(int y) => (int)Math.Round(y * ScaleY);
 float SclF(float pt) => (float)Math.Round(pt * ScaleFont);

 View Add(View v){scene.Add(v);if(pageContainer!=null)pageContainer.Add(v);else Window.Default.Add(v);return v;}
 View AddWindow(View v){scene.Add(v);Window.Default.Add(v);return v;}
 View Panel(int x,int y,int w,int h,Color color,int radius=18){return Add(new View{Position2D=new Position2D(x,y),Size2D=new Size2D(w,h),BackgroundColor=color,CornerRadius=new Vector4(radius,radius,radius,radius)});}
 TextLabel Text(string text,int x,int y,int w,int h,float size,Color color,string face="Medium")
 {return (TextLabel)Add(Label(text,x,y,w,h,size,color,face));}
 TextLabel TextWindow(string text,int x,int y,int w,int h,float size,Color color,string face="Medium")
 {return (TextLabel)AddWindow(Label(text,x,y,w,h,size,color,face));}
 TextLabel Label(string text,int x,int y,int w,int h,float size,Color color,string face="Medium")
 {return new TextLabel{Text=text,Position2D=new Position2D(x,y),Size2D=new Size2D(w,h),PixelSize=size,FontFamily="Quiet Inter "+face,TextColor=color,VerticalAlignment=VerticalAlignment.Center};}

 void BuildPage(bool animate=false)
 {
  if(!animationsEnabled) animate=false;
  var stopwatch=Stopwatch.StartNew();
  if(focusAnimation!=null){focusAnimation.Stop();focusAnimation.Dispose();focusAnimation=null;}
  if(pageAnimation!=null){pageAnimation.Stop();pageAnimation.Dispose();pageAnimation=null;}
  ParkArtwork();pageArtworkCold=0;
  foreach(var v in scene){Window.Default.Remove(v);v.Dispose();}scene.Clear();slots.Clear();
  pageContainer=null;
  Window.Default.BackgroundColor=bg;
  AddWindow(new ImageView{ResourceUrl=Root+(darkMode?"free-background-dark.svg":"free-background.svg"),Size2D=new Size2D(ScreenWidth,ScreenHeight)});
  
  tabs=new TextLabel[3];
  string[] nav={T("inputs"), T("apps"), T("settings")};
  int curTabX = 54;
  for(int i=0; i<3; i++){
   int charCount = nav[i].Length;
   bool isWide = false;
   foreach(char c in nav[i]){
    if(c >= 0x0600) { isWide = true; break; }
   }
   int charW = isWide ? 26 : 18;
   int tabW = Math.Max(SclX(120), charCount * SclX(charW) + SclX(52));
   tabs[i] = TextWindow(nav[i], curTabX, SclY(158), tabW, SclY(68), SclF(30), muted);
   tabs[i].HorizontalAlignment = HorizontalAlignment.Center;
   curTabX += tabW + SclX(16);
  }
  clock=TextWindow(GetFormattedDate(),SclX(1180),SclY(170),SclX(660),SclY(45),SclF(24),muted);clock.HorizontalAlignment=HorizontalAlignment.End;

  pageContainer=new View{
   Size2D=new Size2D(ScreenWidth,ScreenHeight),
   Position=new Position(ScreenWidth/2f,ScreenHeight/2f,0),
   PivotPoint=new Position(0.5f,0.5f,0.5f),
   PositionUsesPivotPoint=true,
   BackgroundColor=new Color(0,0,0,0)
  };
  Window.Default.Add(pageContainer);
  scene.Add(pageContainer);

  string pageTitle=page==0?T("home"):page==1?T("your_apps"):T("quick_settings");
  if(NativeCapabilities.IsLiteVariant)pageTitle+=" (Lite)";
  var title=TextWindow(pageTitle,SclX(76),SclY(55),SclX(1350),SclY(100),SclF(76),ink,"Display");

  if(page==0){
   string heroAsset=GetHeroAsset(0);
   var hero=Feature(hdmiNames[0],"hdmi:13",GetHdmiDesc(0),heroAsset,SclX(80),SclY(262),SclX(1040),SclY(442),"hero");
   hero.Card.Add(Label(hdmiNames[0].ToUpperInvariant(),SclX(44),SclY(52),SclX(550),SclY(40),SclF(24),new Color(.86f,.91f,1,1),"Semibold"));
   hero.Card.Add(Label(T("lets_play"),SclX(40),SclY(124),SclX(560),SclY(100),SclF(72),white,"Display"));
   hero.Card.Add(Label($"{T("switch_to")} HDMI 1",SclX(46),SclY(246),SclX(440),SclY(48),SclF(30),new Color(.88f,.93f,1,1)));
   Feature(hdmiNames[1],"hdmi:14",GetHdmiDesc(1),"feature-switch.png",SclX(1160),SclY(262),SclX(320),SclY(442),"portrait");
   Feature(hdmiNames[2],"hdmi:15",GetHdmiDesc(2),"feature-hdmi.svg",SclX(1520),SclY(262),SclX(320),SclY(442),"portrait");
   Add(new ImageView{ResourceUrl=Root+(darkMode?"glass-shelf-dark.svg":"glass-shelf.svg"),Position2D=new Position2D(0,SclY(784)),Size2D=new Size2D(ScreenWidth,SclY(280))});
   Dock("Netflix","app:org.tizen.netflix-app","netflix",0);
   Dock("YouTube","app:9Ur5IzDKqV.TizenYouTube","youtube",1);
   Dock("Crunchyroll","app:OGLLvqej7u.CrunchyrollWebApp","crunchyroll",2);
   Dock("HBO Max","app:5b8c3eb16b.BeamCTVDev","hbomax",3);
   Dock("Disney+","app:MCmYXNxgcu.DisneyPlus","disney",4);
   Dock("Apple Music","app:org.tizen.apple.apple-music","apple-music",5);
  }else if(page==1){
   BuildLibrary();
  }else{
   RefreshSettings();
   var row0=new List<(string title,string action,string desc,string icon)>();
   if(NativeCapabilities.HasVolume)row0.Add((T("volume"),"volume",T("volume_desc"),"volume"));
   if(NativeCapabilities.HasBrightness)row0.Add((T("brightness"),"brightness",T("brightness_desc"),"brightness"));
   if(NativeCapabilities.HasGameMode)row0.Add((T("game_mode"),"game-mode",T("game_mode_desc"),"game"));
   if(NativeCapabilities.HasSleepTimer)row0.Add((T("sleep_timer"),"sleeptimer",T("sleep_timer_desc"),"sleep"));

   var row1=new List<(string title,string action,string desc,string icon)>();
   if(NativeCapabilities.HasMute)row1.Add((T("sound"),"mute",T("mute_desc"),"mute"));
   if(NativeCapabilities.HasControllers)row1.Add((T("controllers"),"controllers",T("controllers_desc"),"controller"));
   if(NativeCapabilities.HasNetworkSettings)row1.Add((T("network"),"network",T("network_desc"),"network"));
   if(NativeCapabilities.HasAdvancedSettings)row1.Add((T("advanced"),"app:org.tizen.menu",T("advanced_desc"),"settings"));

   if(row0.Count==0&&row1.Count>0){
    row0=row1;
    row1=new List<(string title,string action,string desc,string icon)>();
   }

   for(int i=0;i<row0.Count;i++)SettingsTile(row0[i].title,row0[i].action,row0[i].desc,row0[i].icon,i,0);
   for(int i=0;i<row1.Count;i++)SettingsTile(row1[i].title,row1[i].action,row1[i].desc,row1[i].icon,i,1);
  }

  var animContainer=pageContainer;
  pageContainer=null;

  Toolbar("Wi-Fi","network","wifi",0,"Wi-Fi · Connection setup");
  // Slot 1: Launcher Settings (Wrench icon)
  Toolbar(T("launcher_settings"),"modal:settings","wrench",1,T("launcher_settings_desc"));
  Toolbar(T("settings"),"app:org.tizen.menu","settings",2,T("advanced"));
  Toolbar(darkMode?"Dark Mode":"Light Mode","toggle:theme",darkMode?"moon":"sun",3);
  toolbarHint=TextWindow("",SclX(1400),SclY(170),SclX(440),SclY(45),SclF(24),ink);toolbarHint.HorizontalAlignment=HorizontalAlignment.End;
  detail=TextWindow("",SclX(80),SclY(1030),SclX(1320),SclY(35),SclF(20),muted);
  notice=TextWindow("",SclX(80),SclY(1008),SclX(1760),SclY(34),SclF(22),accent);
  var hint=TextWindow(reordering?T("reorder_hint"):page==1?(AppLibrary.PageCount(library.Count)<=1?T("back_to_home")+" · Select to open":libraryPage+1<AppLibrary.PageCount(library.Count)?"Down for more apps · "+T("back_to_home"):"Up for previous apps · "+T("back_to_home")):T("back_to_input"),SclX(1310),SclY(1030),SclX(530),SclY(35),SclF(20),muted);hint.HorizontalAlignment=HorizontalAlignment.End;

  RebuildRowCache();lastFocusedSlot=-1;
  selected=Math.Max(0,Math.Min(selected,slots.Count-1));UpdateFocus(false);

  if(animate){
   animContainer.Opacity=0.0f;
   animContainer.Scale=new Vector3(0.982f,0.982f,1.0f);
   animContainer.Position=new Position(ScreenWidth/2f,ScreenHeight/2f+SclY(24),0);
   title.Opacity=0.0f;
   title.Position=new Position(SclX(76),SclY(55)+SclY(4),0);
   pageAnimation=new Animation(230);
   pageAnimation.AnimateTo(animContainer,"Opacity",1.0f,new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.AnimateTo(animContainer,"Scale",new Vector3(1.0f,1.0f,1.0f),new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.AnimateTo(animContainer,"Position",new Position(ScreenWidth/2f,ScreenHeight/2f,0),new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.AnimateTo(title,"Opacity",1.0f,new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.AnimateTo(title,"Position",new Position(SclX(76),SclY(55),0),new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.Play();
  }

  buildMilliseconds=stopwatch.ElapsedMilliseconds;WriteState("page_ready");
 }

 void BuildLibrary()
 {
  if(library.Count==0){Text("Apps could not be loaded.",80,300,1500,70,32,ink);return;}
  int totalPages = AppLibrary.PageCount(library.Count);
  libraryPage=Math.Clamp(libraryPage,0,totalPages-1);
  int start=libraryPage*AppLibrary.PageSize,count=AppLibrary.VisibleCount(library.Count,libraryPage);
  for(int i=0;i<count;i++)
  {
   var app=library[start+i];
   int w=SclX(328),h=SclY(184);
   int x=SclX(80)+(i%5)*SclX(358),y=SclY(266)+(i/5)*SclY(245);
   var shadow=Add(new ImageView{ResourceUrl=Root+"shadow-app.svg",Position2D=new Position2D(x-SclX(24),y-SclY(10)),Size2D=new Size2D(w+SclX(48),h+SclY(64))});
   var card=Panel(x+w/2,y+h/2,w,h,HexColor(app.Background),SclX(24));
   card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
   AddAppArtwork(card,app,w,h);
   var ring=new View{Size2D=new Size2D(w,h),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(SclX(24),SclX(24),SclX(24),SclX(24)),BorderlineWidth=4,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
   card.Add(ring);
   slots.Add(new Slot{Card=card,Shadow=shadow,FocusRing=ring,Label=null,Name=app.Name,Action="app:"+app.Id,Description=app.Description,Row=i/5,X=x,Y=y,W=w,H=h});
  }
  Text(library.FindAll(a=>!a.Utility).Count+" apps",SclX(80),SclY(218),SclX(500),SclY(30),SclF(21),muted);
  
  // Pagination Dots / Slider Indicator: Only active if more than 1 page exists!
  if(totalPages > 1){
   int dotSpacing = SclX(22);
   int dotWidth = SclX(10);
   int activeWidth = SclX(26);
   int totalW = (totalPages - 1) * (dotWidth + dotSpacing) + activeWidth;
   int startX = SclX(1840) - totalW;
   for(int p = 0; p < totalPages; p++){
    bool isActive = p == libraryPage;
    int currW = isActive ? activeWidth : dotWidth;
    var dot = Add(new View{
     Position2D = new Position2D(startX, SclY(226)),
     Size2D = new Size2D(currW, SclY(10)),
     BackgroundColor = isActive ? accent : (darkMode ? new Color(1,1,1,0.25f) : new Color(0,0,0,0.25f)),
     CornerRadius = new Vector4(SclX(5), SclX(5), SclX(5), SclX(5))
    });
    startX += currW + SclX(12);
   }
  }
 }

 void PlayTick()
 {
  if(!soundEnabled)return;
  try{
   if(File.Exists("/usr/share/feedback/sound/operation/move.wav"))
    NativeCapabilities.PlayKeySound("/usr/share/feedback/sound/operation/move.wav", 0);
   else if(File.Exists(Root+"click.wav"))
    NativeCapabilities.PlayKeySound(Root+"click.wav", 0);
  }catch(Exception ex){
   DebugLog.Warn("Sound", "PlayTick sound failed", ex);
  }
 }

 Color HexColor(string hex)
 {
  try{int n=Convert.ToInt32(hex.TrimStart('#'),16);return new Color(((n>>16)&255)/255f,((n>>8)&255)/255f,(n&255)/255f,1);}
  catch(Exception ex){DebugLog.Warn("Color", "HexColor failed for " + hex, ex);return white;}
 }

 void AddAppArtwork(View card,LibraryApp app,int w,int h)
 {
  if(!app.Utility){
   var cached=GetArtwork(app,w,h);
   if(cached.Image.LoadingStatus!=ImageView.LoadingStatusType.Ready)pageArtworkCold++;
   cached.Parent.Remove(cached.Image);card.Add(cached.Image);cached.Parent=card;
  }
  if(app.Utility){
   card.Add(new View{Position2D=new Position2D(w/2-SclX(28),h/2-SclY(2)),Size2D=new Size2D(SclX(56),SclY(4)),BackgroundColor=ink});
   card.Add(new View{Position2D=new Position2D(w/2-SclX(2),h/2-SclY(28)),Size2D=new Size2D(SclX(4),SclY(56)),BackgroundColor=ink});
  }
 }

 void PreloadArtwork()
 {
  artworkPreloadRoot=new View{Size2D=new Size2D(ScreenWidth,ScreenHeight),Opacity=0};
  Window.Default.Add(artworkPreloadRoot);
  var preloadedHeroes=new HashSet<string>();
  foreach(var pr in HdmiPresets){
   if(preloadedHeroes.Add(pr.heroAsset))
    PreloadFeature(pr.heroAsset,SclX(1040),SclY(442));
  }
  PreloadFeature("feature-switch.png",SclX(320),SclY(442));
  PreloadFeature("feature-hdmi.svg",SclX(320),SclY(442));
  string[] quickIcons={"volume","brightness","game","sleep","mute","controller","network","settings"};
  foreach(var icon in quickIcons){
   var img=new ImageView{ResourceUrl=Root+"quick-"+icon+".png",Size2D=new Size2D(SclX(410),SclY(286)),DesiredWidth=SclX(820),DesiredHeight=SclY(572),SamplingMode=SamplingModeType.BoxThenLanczos,ReleasePolicy=ReleasePolicyType.Never,CornerRadius=new Vector4(SclX(24),SclX(24),SclX(24),SclX(24))};
   quickCache[icon]=img;
   artworkPreloadRoot.Add(img);
  }
  foreach(var app in library)if(!app.Utility)GetArtwork(app,SclX(328),SclY(184));
  foreach(var id in new[]{"org.tizen.netflix-app","com.samsung.tv.cobalt-yt","org.tizen.primevideo"}){
   var app=library.Find(a=>a.Id==id);if(app!=null)GetArtwork(app,SclX(253),SclY(126));
  }
 }

 CachedArtwork GetArtwork(LibraryApp app,int w,int h)
 {
  string key=app.Id+"@"+w+"x"+h;
  if(artworkCache.TryGetValue(key,out var cached))return cached;
  bool cover=app.Fit=="cover",full=cover||app.Fit=="contain-full";
  int iw=full?w:w-SclX(54),ih=full?h:h-SclY(40);var crop=app.Crop;
  float cropWidth=crop==null?1:crop[2],cropHeight=crop==null?1:crop[3];
  var image=new ImageView{
   Position2D=new Position2D((w-iw)/2,(h-ih)/2),Size2D=new Size2D(iw,ih),
   FittingMode=cover?FittingModeType.ScaleToFill:FittingModeType.ShrinkToFit,
   SamplingMode=SamplingModeType.BoxThenLanczos,
   DesiredWidth=(int)Math.Ceiling(iw*2/cropWidth),DesiredHeight=(int)Math.Ceiling(ih*2/cropHeight),
   ReleasePolicy=ReleasePolicyType.Destroyed,CornerRadius=new Vector4(SclX(24),SclX(24),SclX(24),SclX(24))};
  if(crop!=null)image.PixelArea=new RelativeVector4(crop[0],crop[1],crop[2],crop[3]);
  cached=new CachedArtwork{Image=image,Parent=artworkPreloadRoot};
  artworkCache.Add(key,cached);artworkViewsCreated++;
  image.ResourceReady+=(s,e)=>{artworkReadyCount++;WriteState("artwork_ready");};
  artworkPreloadRoot.Add(image);image.ResourceUrl=Root+app.Asset;
  return cached;
 }

 void PreloadFeature(string asset,int w,int h)
 {
  string key=asset+"@"+w+"x"+h;
  if(!featureCache.ContainsKey(key)){
   var img=new ImageView{ResourceUrl=Root+asset,Size2D=new Size2D(w,h),DesiredWidth=w*2,DesiredHeight=h*2,SamplingMode=SamplingModeType.BoxThenLanczos,ReleasePolicy=ReleasePolicyType.Never,CornerRadius=new Vector4(SclX(28),SclX(28),SclX(28),SclX(28))};
   featureCache[key]=img;
   artworkPreloadRoot.Add(img);
  }
 }

 void ParkArtwork()
 {
  foreach(var cached in artworkCache.Values)if(cached.Parent!=artworkPreloadRoot){
   cached.Parent.Remove(cached.Image);artworkPreloadRoot.Add(cached.Image);cached.Parent=artworkPreloadRoot;
  }
  foreach(var img in featureCache.Values){
   if(img.GetParent()!=null&&img.GetParent()!=artworkPreloadRoot){
    img.GetParent().Remove(img);artworkPreloadRoot.Add(img);
   }
  }
  foreach(var img in quickCache.Values){
   if(img.GetParent()!=null&&img.GetParent()!=artworkPreloadRoot){
    img.GetParent().Remove(img);artworkPreloadRoot.Add(img);
   }
  }
 }

 int ArtworkReadyCount()
 {
  int count=0;foreach(var cached in artworkCache.Values)
   if(cached.Image.LoadingStatus==ImageView.LoadingStatusType.Ready)count++;
  return count;
 }

 string ArtworkPending()
 {
  var pending=new List<string>();
  foreach(var entry in artworkCache)
   if(entry.Value.Image.LoadingStatus!=ImageView.LoadingStatusType.Ready)
    pending.Add(entry.Key+":"+entry.Value.Image.LoadingStatus);
  return string.Join(";",pending);
 }

 Slot Feature(string label,string action,string description,string asset,int x,int y,int w,int h,string type)
 {
  var shadow=Add(new ImageView{ResourceUrl=Root+"shadow-"+type+".svg",Position2D=new Position2D(x-SclX(24),y-SclY(14)),Size2D=new Size2D(w+SclX(48),h+SclY(64))});
  var card=Panel(x+w/2,y+h/2,w,h,new Color(0,0,0,0),SclX(28));card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
  string key=asset+"@"+w+"x"+h;
  if(!featureCache.TryGetValue(key,out var img)){
   img=new ImageView{ResourceUrl=Root+asset,Size2D=new Size2D(w,h),FittingMode=FittingModeType.ScaleToFill,SamplingMode=SamplingModeType.BoxThenLanczos};
   featureCache[key]=img;
  }else{
   if(img.GetParent()!=null)img.GetParent().Remove(img);
   img.Size2D=new Size2D(w,h);
  }
  card.Add(img);
  var ring=new View{Size2D=new Size2D(w,h),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(SclX(28),SclX(28),SclX(28),SclX(28)),BorderlineWidth=SclX(4),BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
  card.Add(ring);
  var nameLabel=Text(label,x+SclX(4),y+h+SclY(16),w-SclX(8),SclY(36),SclF(28),captionMuted,"Semibold");
  var descLabel=Text(description,x+SclX(4),y+h+SclY(52),w-SclX(8),SclY(30),SclF(21),descMuted);
  var slot=new Slot{Card=card,Shadow=shadow,FocusRing=ring,Label=nameLabel,Desc=descLabel,Name=label,Action=action,Description=description,Row=0,X=x,Y=y,W=w,H=h};
  slots.Add(slot);
  return slot;
 }

 void Dock(string label,string action,string asset,int n)
 {
  int dockW=SclX(253),dockH=SclY(126);
  int x=SclX(80)+n*(dockW+SclX(48));
  var shadow=Add(new ImageView{ResourceUrl=Root+"shadow-dock.svg",Position2D=new Position2D(x-SclX(24),SclY(842)),Size2D=new Size2D(SclX(301),SclY(190))});
  var card=Panel(x+dockW/2,SclY(854)+dockH/2,dockW,dockH,new Color(0,0,0,0),SclX(22));card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
  string dockSvg=Root+"free-dock-"+asset+".svg";
  string dockPng=Root+"free-dock-"+asset+".png";
  if(File.Exists(dockPng)){
   card.Add(new ImageView{ResourceUrl=dockPng,Size2D=new Size2D(dockW,dockH),FittingMode=FittingModeType.ScaleToFill,SamplingMode=SamplingModeType.BoxThenLanczos});
  }else if(File.Exists(dockSvg)){
   card.Add(new ImageView{ResourceUrl=dockSvg,Size2D=new Size2D(dockW,dockH)});
  }else{
   var official=library.Find(a=>a.Id==action.Substring(action.IndexOf(':')+1));
   if(official!=null){card.BackgroundColor=HexColor(official.Background);AddAppArtwork(card,official,dockW,dockH);}
   else card.Add(new ImageView{ResourceUrl=dockSvg,Size2D=new Size2D(dockW,dockH)});
  }
  var ring=new View{Size2D=new Size2D(dockW,dockH),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(SclX(22),SclX(22),SclX(22),SclX(22)),BorderlineWidth=4,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
  card.Add(ring);
  slots.Add(new Slot{Card=card,Shadow=shadow,FocusRing=ring,Label=null,Name=label,Action=action,Description=label,Row=1,X=x,Y=SclY(854),W=dockW,H=dockH});
 }

 void SettingsTile(string title,string action,string description,string icon,int col,int row)
 {
  int w=SclX(410),h=SclY(286);
  int x=SclX(80)+col*SclX(450),y=SclY(262)+row*SclY(332);
  var card=Panel(x+w/2,y+h/2,w,h,new Color(0,0,0,0),SclX(24));card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
  if(!quickCache.TryGetValue(icon,out var tileImg)){
   tileImg=new ImageView{ResourceUrl=Root+"quick-"+icon+".png",Size2D=new Size2D(w,h),DesiredWidth=820,DesiredHeight=572,SamplingMode=SamplingModeType.BoxThenLanczos,ReleasePolicy=ReleasePolicyType.Never,CornerRadius=new Vector4(SclX(24),SclX(24),SclX(24),SclX(24))};
   quickCache[icon]=tileImg;
  }else{
   if(tileImg.GetParent()!=null)tileImg.GetParent().Remove(tileImg);
  }
  card.Add(tileImg);
  card.Add(Label(title,SclX(26),SclY(25),SclX(348),SclY(42),SclF(29),white,"Semibold"));
  var valLabel=Label(SettingDisplay(action),SclX(26),SclY(146),SclX(358),SclY(56),SclF(38),white,"Display");card.Add(valLabel);
  var descLabel=Label(description,SclX(26),SclY(226),SclX(370),SclY(34),SclF(20),new Color(.95f,.96f,1,1));card.Add(descLabel);
  var ring=new View{Size2D=new Size2D(w,h),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(SclX(24),SclX(24),SclX(24),SclX(24)),BorderlineWidth=4,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
  card.Add(ring);
  slots.Add(new Slot{Card=card,Shadow=null,FocusRing=ring,Label=valLabel,Desc=descLabel,Name=title,Action=action,Description=description,Row=row,X=x,Y=y,W=w,H=h});
 }

 string SettingDisplay(string action)
 {
  if(action=="volume")return currentVolume<0?T("unavailable"):currentVolume+"%";
  if(action=="brightness")return CanEdit("brightness")?SettingValue("brightness"):T("unavailable");
  if(action=="game-mode")return CanEdit("game-mode")?(SettingValue("game-mode")=="1"?T("on"):T("off")):T("unavailable");
  if(action=="sleeptimer")return CanEdit("sleeptimer")?(SettingValue("sleeptimer")=="0"?T("off"):SettingValue("sleeptimer")+" min"):T("unavailable");
  if(action=="mute")return currentMute<0?T("unavailable"):currentMute==1?T("muted"):T("sound_on");
  return "";
 }

 void Toolbar(string label,string action,string icon,int n,string description="")
 {
  int tbSize=SclX(68);
  int x=SclX(1508)+n*SclX(88);
  var card=Panel(x+tbSize/2,SclY(103),tbSize,tbSize,new Color(0,0,0,0),tbSize/2);card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
  card.Add(new ImageView{ResourceUrl=Root+(darkMode?"glass-button-dark.svg":"glass-button.svg"),Position2D=new Position2D(-SclX(16),-SclY(16)),Size2D=new Size2D(SclX(100),SclY(112))});
  card.Add(new ImageView{ResourceUrl=Root+"symbol-"+icon+".png",Color=ink,Position2D=new Position2D(SclX(19),SclY(19)),Size2D=new Size2D(SclX(30),SclY(30))});
  var ring=new View{Size2D=new Size2D(tbSize,tbSize),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(tbSize/2,tbSize/2,tbSize/2,tbSize/2),BorderlineWidth=3,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
  card.Add(ring);
  slots.Add(new Slot{Card=card,FocusRing=ring,Name=label,Action=action,Description=string.IsNullOrEmpty(description)?label:description,Row=-1,X=x,Y=SclY(69),W=tbSize,H=tbSize});
 }

 void RebuildRowCache()
 {
  rowCache.Clear();
  for(int i=0;i<slots.Count;i++){
   int r=slots[i].Row;
   if(!rowCache.TryGetValue(r,out var list)){list=new List<int>();rowCache[r]=list;}
   list.Add(i);
  }
 }

 List<int> Row(int row){return rowCache.TryGetValue(row,out var list)?list:emptyRow;}

 void UpdateFocus(bool animate=true)
 {
  if(!animationsEnabled) animate=false;
  if(focusAnimation!=null){focusAnimation.Stop();focusAnimation.Dispose();focusAnimation=null;}
  if(animate){focusAnimation=new Animation(180);PlayTick();}
  for(int i=0;i<3;i++){
   tabs[i].TextColor=i==page?ink:muted;
   tabs[i].FontFamily="Quiet Inter "+(i==page?"Semibold":"Medium");
   tabs[i].BackgroundColor=area==0&&i==page?(darkMode?new Color(1,1,1,0.18f):new Color(1,1,1,0.7f)):new Color(0,0,0,0);
   tabs[i].CornerRadius=new Vector4(34,34,34,34);
  }
  int curFocused = area != 0 ? selected : -1;
  if(animate && lastFocusedSlot >= 0 && lastFocusedSlot < slots.Count && lastFocusedSlot != curFocused){
   var prev = slots[lastFocusedSlot];
   focusAnimation.AnimateTo(prev.Card,"Scale",new Vector3(1,1,1));
   focusAnimation.AnimateTo(prev.Card,"Position",new Position(prev.X+prev.W/2f,prev.Y+prev.H/2f,0));
   if(prev.Shadow!=null)prev.Shadow.Opacity=darkMode?0.35f:0.55f;
   if(prev.FocusRing!=null){prev.FocusRing.Opacity=0;prev.FocusRing.BorderlineColor=accent;prev.FocusRing.BorderlineWidth=4;}
   if(prev.Label!=null&&page!=2)prev.Label.TextColor=captionMuted;
   if(prev.Desc!=null&&page!=2)prev.Desc.TextColor=descMuted;
  }
  if(curFocused >= 0 && curFocused < slots.Count){
   var cur = slots[curFocused];
   float scale=page==1&&cur.Row>=0?(reordering?1.10f:1.055f):page==2&&cur.Row>=0?1.035f:cur.Row==0?1.05f:cur.Row==1?1.20f:1.10f;
   var pos=new Position(cur.X+cur.W/2f,cur.Y+cur.H/2f-((cur.Row==0||page==1&&cur.Row>=0)?5:0),0);
   if(animate){
    focusAnimation.AnimateTo(cur.Card,"Scale",new Vector3(scale,scale,1));
    focusAnimation.AnimateTo(cur.Card,"Position",pos);
   }else{
    cur.Card.Scale=new Vector3(scale,scale,1);cur.Card.Position=pos;
   }
   if(cur.Shadow!=null)cur.Shadow.Opacity=1;
   if(cur.FocusRing!=null){
    cur.FocusRing.Opacity=1;
    cur.FocusRing.BorderlineColor=reordering?reorderColor:accent;
    cur.FocusRing.BorderlineWidth=reordering?SclX(6):SclX(4);
    cur.FocusRing.RaiseToTop();
   }
   if(cur.Label!=null&&page!=2)cur.Label.TextColor=ink;
   if(cur.Desc!=null&&page!=2)cur.Desc.TextColor=ink;
   cur.Card.RaiseToTop();
  }
  if(!animate){
   for(int i=0;i<slots.Count;i++){
    if(i==curFocused)continue;
    var s=slots[i];
    s.Card.Scale=new Vector3(1,1,1);
    s.Card.Position=new Position(s.X+s.W/2f,s.Y+s.H/2f,0);
    if(s.Shadow!=null)s.Shadow.Opacity=darkMode?0.35f:0.55f;
    if(s.FocusRing!=null){s.FocusRing.Opacity=0;s.FocusRing.BorderlineColor=accent;s.FocusRing.BorderlineWidth=4;}
    if(s.Label!=null&&page!=2)s.Label.TextColor=captionMuted;
    if(s.Desc!=null&&page!=2)s.Desc.TextColor=descMuted;
   }
  }
  lastFocusedSlot = curFocused;
  if(focusAnimation!=null)focusAnimation.Play();

  if(area==3){
   toolbarHint.Text="";
   detail.Text="";
  }else if(area==0){
   toolbarHint.Text="";
   detail.Text=page==0?T("inputs")+" · Direct HDMI switching":page==1?T("apps")+" · Installed streaming apps":T("settings")+" · Quick configuration";
  }else if(curFocused>=0&&curFocused<slots.Count){
   toolbarHint.Text="";
   detail.Text=slots[curFocused].Description;
  }
  if(page==2){
   foreach(var s in slots){
    if(s.Row<0||s.Label==null)continue;
    if(s.Action=="volume")s.Label.Text=SettingDisplay("volume");
    else if(s.Action=="brightness")s.Label.Text=SettingDisplay("brightness");
    else if(s.Action=="game-mode")s.Label.Text=SettingDisplay("game-mode");
    else if(s.Action=="sleeptimer")s.Label.Text=SettingDisplay("sleeptimer");
    else if(s.Action=="mute")s.Label.Text=currentMute<0?T("unavailable"):currentMute==1?T("muted"):T("sound_on");
    else if(s.Action=="controllers")s.Label.Text=T("pair_controller");
    else if(s.Action=="network")s.Label.Text=T("connections");
    else if(s.Action=="app:org.tizen.menu")s.Label.Text=T("all_settings");
   }
  }
  if(reordering&&curFocused>=0&&curFocused<slots.Count)notice.Text=T("reorder_hint");
  if(editing!="")detail.Text="Left / Right to adjust "+editing+" · Select or Back to finish";
  WriteState("focus");
 }

 void OnKey(object sender,Window.KeyEventArgs e)
 {
  if(e.Key==null)return;
  string k=e.Key.KeyPressedName;
  var state=e.Key.State;

  if(debugOverlay!=null){
   if(state==Key.StateType.Down&&(k=="XF86Back"||k=="Back"||k=="Escape"||k=="Return"||k=="KP_Enter"||k=="XF86Info"||k=="Info"||k=="d"||k=="D"||k=="0"||k=="MediaPlayPause"||k=="Play")){
    ToggleDebugLogView();
   }
   return;
  }

  if(state==Key.StateType.Down&&(k=="XF86Info"||k=="Info"||k=="d"||k=="D"||k=="0"||k=="MediaPlayPause"||k=="Play")){
   ToggleDebugLogView();
   return;
  }

  if(modalOverlay!=null){
   if(state==Key.StateType.Down)HandleModalKey(k);
   return;
  }

  if(state==Key.StateType.Up){
   HandleKeyUp(k);
   return;
  }

  if(state!=Key.StateType.Down)return;

  if((k=="Return"||k=="KP_Enter")&&(waitingForLongPress||longPressTriggered))return;

  long now=Environment.TickCount64;if(k==lastKey&&now-lastKeyAt<180)return;lastKey=k;lastKeyAt=now;
  try{
   if(k=="XF86Home"){CancelLongPress();if(reordering)StopReordering();Home("home_key");return;}
   if(k=="XF86SysMenu"||k=="XF86More"){CancelLongPress();if(reordering)StopReordering();OpenSettings("settings_key");return;}
   if(page==1&&HandleLibraryKey(k))return;
   if(editing!=""){if(k=="Left"||k=="Right"){int d=k=="Left"?-1:1;if(editing=="volume")ChangeVolume(d);else ChangeBrightness(d);}else if(k=="Return"||k=="KP_Enter"||k=="XF86Back"||k=="Back"||k=="Escape"){editing="";UpdateFocus();}return;}
   if(k=="Up"){
    if(area==2){var r0=Row(0);var r1=Row(1);int col=r1.IndexOf(selected);area=1;selected=r0.Count>0?(page==2?r0[Math.Max(0,Math.Min(col>=0?col:0,r0.Count-1))]:r0[0]):0;}
    else if(area==1){area=0;}
    else if(area==0){area=3;var r=Row(-1);selected=r.Count>0?r[Math.Min(page,r.Count-1)]:0;}
    UpdateFocus();
   }
   else if(k=="Down"){
    if(area==3){area=0;}
    else if(area==0){area=1;selected=Row(0).Count>0?Row(0)[0]:0;}
    else if(area==1){var r0=Row(0);var r1=Row(1);if(r1.Count>0){int col=r0.IndexOf(selected);area=2;selected=(page==2?r1[Math.Max(0,Math.Min(col>=0?col:0,r1.Count-1))]:r1[0]);}UpdateFocus();}
    UpdateFocus();
   }
   else if(k=="Left"||k=="Right"){
    int d=k=="Left"?-1:1;
    if(area==0){
     int next=page+d;
     if(next<0||next>2){area=3;var r=Row(-1);selected=r[next<0?r.Count-1:0];UpdateFocus();}
     else{page=next;selected=0;PlayTick();BuildPage(true);}
    }
    else if(area==3){
     var r=Row(-1);int n=r.IndexOf(selected)+d;
     if(n<0||n>=r.Count){area=0;page=n<0?2:0;selected=0;PlayTick();BuildPage(true);}
     else{selected=r[n];UpdateFocus();}
    }
    else{var r=Row(area-1);if(r.Count>0){int n=r.IndexOf(selected);selected=r[(n+d+r.Count)%r.Count];}UpdateFocus();}
   }
   else if(k=="Return"||k=="KP_Enter"){if(area==0){area=1;selected=0;UpdateFocus();}else Activate();}
   else if(k=="XF86Back"||k=="Back"||k=="Escape"||k=="XF86Exit")SelectSource(lastSource);
  }catch(Exception ex){DebugLog.Error("Key", "OnKey error for " + k, ex);ShowError(ex.Message);}
 }

 void HandleKeyUp(string k)
 {
  if(k!="Return"&&k!="KP_Enter")return;
  if(waitingForLongPress){
   waitingForLongPress=false;
   if(longPressTimer!=null){longPressTimer.Stop();longPressTimer.Dispose();longPressTimer=null;}
   Activate();
   return;
  }
  if(longPressTriggered){
   longPressTriggered=false;
   return;
  }
 }

 void CancelLongPress()
 {
  if(waitingForLongPress){
   waitingForLongPress=false;
   if(longPressTimer!=null){longPressTimer.Stop();longPressTimer.Dispose();longPressTimer=null;}
  }
 }

 void StartReordering()
 {
  if(page!=1||area!=1||slots.Count==0||selected<0||selected>=slots.Count)return;
  reordering=true;
  PlayTick();
  UpdateFocus(false);
  notice.Text=T("reorder_hint");
  WriteState("reorder_start");
 }

 void StopReordering()
 {
  if(!reordering)return;
  reordering=false;
  SaveLibrary();
  PlayTick();
  UpdateFocus(false);
  string name=selected>=0&&selected<slots.Count?slots[selected].Name:"App";
  notice.Text=$"Posición guardada: {name}";
  WriteState("reorder_done");
 }

 void SaveLibrary()
 {
  try{
   AppLibrary.Save(AppsDataPath, library);
  }catch(Exception ex){
   DebugLog.Error("Library", "SaveLibrary failed", ex);
   ShowError("Save apps failed: " + ex.Message);
  }
 }

 bool HandleLibraryKey(string k)
 {
  if(area==1&&(k=="Return"||k=="KP_Enter")){
   if(reordering){
    StopReordering();
    return true;
   }
   waitingForLongPress=true;
   longPressTriggered=false;
   if(longPressTimer!=null){longPressTimer.Stop();longPressTimer.Dispose();}
   longPressTimer=new Tizen.NUI.Timer(500);
   longPressTimer.Tick+=(s,e)=>{
    waitingForLongPress=false;
    longPressTriggered=true;
    StartReordering();
    return false;
   };
   longPressTimer.Start();
   return true;
  }

  if(reordering){
   if(k=="XF86Back"||k=="Back"||k=="Escape"){
    StopReordering();
    return true;
   }
   string moveKey=k=="Left"?"Left":k=="Right"?"Right":k=="Up"?"Up":k=="Down"?"Down":"";
   if(string.IsNullOrEmpty(moveKey))return true;
   int curIdx=libraryPage*AppLibrary.PageSize+selected;
   int next=AppLibrary.Move(library.Count,curIdx,moveKey);
   if(next>=0&&next!=curIdx){
    int targetSlot=next%AppLibrary.PageSize;
    var temp=library[curIdx];
    library[curIdx]=library[next];
    library[next]=temp;
    SaveLibrary();
    int nextPage=next/AppLibrary.PageSize;
    selected=next%AppLibrary.PageSize;
    libraryPage=nextPage;
    PlayTick();
    BuildPage(false);
    UpdateFocus(false);
    notice.Text=T("reorder_hint");
   }
   return true;
  }

  if(k=="Up"||k=="Down"||k=="Left"||k=="Right"){
   string key=k;
   if(area!=1)return false;
   int curCol=selected%5;int curRow=selected/5;
   if(k=="Up"&&curRow==0){area=0;UpdateFocus();return true;}
   if(k=="Down"){
    int totalVisible=AppLibrary.VisibleCount(library.Count,libraryPage);
    int nextRowFirst=(curRow+1)*5;
    if(nextRowFirst>=totalVisible){
     if(libraryPage+1<AppLibrary.PageCount(library.Count)){
      libraryPage++;selected=Math.Min(selected,AppLibrary.VisibleCount(library.Count,libraryPage)-1);
      PlayTick();BuildPage(true);return true;
     }
     return true;
    }
   }
   int next=AppLibrary.Move(library.Count,libraryPage*AppLibrary.PageSize+selected,key);
   if(next<0){area=0;UpdateFocus();return true;}
   int nextPage=next/AppLibrary.PageSize;selected=next%AppLibrary.PageSize;
   if(nextPage!=libraryPage){libraryPage=nextPage;PlayTick();BuildPage(true);}else UpdateFocus();
   return true;
  }
  return false;
 }

 void Activate()
 {
  PlayTick();
  string a=slots[selected].Action;notice.Text="";
  if(a=="modal:settings"){
   OpenLauncherSettings();
   return;
  }
  if(a=="toggle:theme"){
   darkMode=!darkMode;
   SaveConfig();
   BuildPage(false);
   return;
  }
  if(a.StartsWith("hdmi:")){SelectSource(int.Parse(a.Substring(5)));return;}
  if(a.StartsWith("app:org.tizen.menu")){Launch("org.tizen.menu");return;}
  if(a.StartsWith("app:")){Launch(a.Substring(4));return;}
  if(a.StartsWith("page:")){page=int.Parse(a.Substring(5));selected=0;area=1;BuildPage(true);return;}
  if(a=="controllers"){Launch("org.tizen.menu","add-bluetooth-keyboard");return;}
  if(a=="network"){Launch("org.tizen.menu","networksettings");return;}
  if(a=="volume"||a=="brightness"){RefreshSettings();if(a=="brightness"&&!CanEdit(a)){ShowError("Brightness is unavailable for the current input.");return;}editing=a;UpdateFocus();return;}
  if(a=="game-mode"){RefreshSettings();SetSetting(a,SettingValue(a)=="1"?"0":"1");return;}
  if(a=="sleeptimer"){RefreshSettings();int n;if(int.TryParse(SettingValue(a),out n))SetSetting(a,((n+1)%5).ToString());return;}
  if(a=="mute"){int n;bool ok=false;if(NativeCapabilities.GetMasterMute(out n)){ok=NativeCapabilities.SetMasterMute(n==1?0:1);}if(!ok)ShowError("Use the remote's mute button.");RefreshSettings();UpdateFocus();WriteState("mute_changed");}
 }

 void OpenLauncherSettings()
 {
  if(modalOverlay!=null)return;
  modalRow=0;

  modalOverlay=new View{
   Position2D=new Position2D(0,0),
   Size2D=new Size2D(ScreenWidth,ScreenHeight),
   BackgroundColor=new Color(0,0,0,0.65f)
  };

  var box=new View{
   Position2D=new Position2D(SclX(410),SclY(105)),
   Size2D=new Size2D(SclX(1100),SclY(870)),
   BackgroundColor=darkMode?new Color(0.12f,0.15f,0.22f,0.98f):new Color(0.96f,0.96f,0.98f,0.98f),
   CornerRadius=new Vector4(SclX(28),SclX(28),SclX(28),SclX(28)),
   BorderlineWidth=SclX(3),
   BorderlineColor=accent
  };
  modalOverlay.Add(box);

  box.Add(new TextLabel{
   Text=T("launcher_settings"),
   Position2D=new Position2D(SclX(50),SclY(40)),
   Size2D=new Size2D(SclX(1000),SclY(50)),
   PixelSize=SclF(38),
   FontFamily="Quiet Inter Semibold",
   TextColor=ink
  });
  box.Add(new TextLabel{
   Text=T("modal_hint"),
   Position2D=new Position2D(SclX(50),SclY(96)),
   Size2D=new Size2D(SclX(1000),SclY(32)),
   PixelSize=SclF(21),
   FontFamily="Quiet Inter Medium",
   TextColor=muted
  });

  modalRowViews=new View[8];
  modalRowLabels=new TextLabel[8];
  modalRowValues=new TextLabel[8];

  string[] rowTitles = new[] {
   "HDMI 1",
   "HDMI 2",
   "HDMI 3",
   T("language"),
   T("sound"),
   T("animations"),
   "Registro de Depuracion (Debug Log)",
   T("save_close")
  };

  for(int i=0;i<8;i++){
   int y=SclY(136)+i*SclY(88);
   var rView=new View{
    Position2D=new Position2D(SclX(40),y),
    Size2D=new Size2D(SclX(1020),SclY(76)),
    BackgroundColor=i==0?(darkMode?new Color(1,1,1,0.12f):new Color(0,0,0,0.08f)):new Color(0,0,0,0),
    CornerRadius=new Vector4(SclX(16),SclX(16),SclX(16),SclX(16)),
    BorderlineWidth=i==0?SclX(2):0,
    BorderlineColor=accent
   };
   box.Add(rView);
   modalRowViews[i]=rView;

   var lbl=new TextLabel{
    Text=rowTitles[i],
    Position2D=new Position2D(SclX(24),SclY(14)),
    Size2D=new Size2D(SclX(440),SclY(48)),
    PixelSize=SclF(24),
    FontFamily="Quiet Inter Semibold",
    TextColor=ink,
    VerticalAlignment=VerticalAlignment.Center
   };
   rView.Add(lbl);
   modalRowLabels[i]=lbl;

   var val=new TextLabel{
    Text=GetModalRowValueText(i),
    Position2D=new Position2D(SclX(480),SclY(14)),
    Size2D=new Size2D(SclX(516),SclY(48)),
    PixelSize=SclF(22),
    FontFamily="Quiet Inter Medium",
    TextColor=accent,
    HorizontalAlignment=HorizontalAlignment.End,
    VerticalAlignment=VerticalAlignment.Center
   };
   rView.Add(val);
   modalRowValues[i]=val;
  }

  Window.Default.Add(modalOverlay);
  modalOverlay.RaiseToTop();
  UpdateModalRowHighlights();
 }

 string GetModalRowValueText(int row)
 {
  if(row>=0&&row<=2){
   return $"◀  {hdmiNames[row]}  ▶";
  }
  if(row==3){
   return $"◀  {LanguageNames[currentLang]}  ▶";
  }
  if(row==4){
   return $"◀  {(soundEnabled ? T("sound_on") : T("sound_off"))}  ▶";
  }
  if(row==5){
   return $"◀  {(animationsEnabled ? T("sound_on") : T("sound_off"))}  ▶";
  }
  if(row==6){
   return "Ver / Volcar ▶";
  }
  if(row==7){
   return "OK";
  }
  return "";
 }

 void UpdateModalRowHighlights()
 {
  for(int i=0;i<8;i++){
   bool isSel = i == modalRow;
   modalRowViews[i].BackgroundColor = isSel ? (darkMode?new Color(1,1,1,0.14f):new Color(0,0,0,0.08f)) : new Color(0,0,0,0);
   modalRowViews[i].BorderlineWidth = isSel ? 3 : 0;
   modalRowViews[i].BorderlineColor = accent;
   modalRowValues[i].Text = GetModalRowValueText(i);
   modalRowValues[i].TextColor = isSel ? white : accent;
  }
 }

 void HandleModalKey(string k)
 {
  PlayTick();
  if(k=="XF86Back"||k=="Back"||k=="Escape"){
   CloseLauncherSettings();
   return;
  }
  if(k=="Up"){
   modalRow=(modalRow-1+8)%8;
   UpdateModalRowHighlights();
   return;
  }
  if(k=="Down"){
   modalRow=(modalRow+1)%8;
   UpdateModalRowHighlights();
   return;
  }
  if(k=="Left"||k=="Right"){
   int d = k=="Left" ? -1 : 1;
   AlterModalRow(modalRow, d);
   UpdateModalRowHighlights();
   return;
  }
  if(k=="Return"||k=="KP_Enter"){
   if(modalRow==6){
    ToggleDebugLogView();
    return;
   }
   if(modalRow==7){
    CloseLauncherSettings();
    return;
   }
   AlterModalRow(modalRow, 1);
   UpdateModalRowHighlights();
  }
 }

 void AlterModalRow(int row, int dir)
 {
  if(row>=0&&row<=2){
   int cur = hdmiPresetIndex[row];
   cur = (cur + dir + HdmiPresets.Length) % HdmiPresets.Length;
   hdmiPresetIndex[row] = cur;
   hdmiNames[row] = HdmiPresets[cur].name;
   hdmiDescs[row] = GetHdmiDesc(row);
   return;
  }
  if(row==3){
   currentLang = (currentLang + dir + LanguageCodes.Length) % LanguageCodes.Length;
   for(int h=0; h<3; h++) hdmiDescs[h] = GetHdmiDesc(h);
   return;
  }
  if(row==4){
   soundEnabled = !soundEnabled;
   return;
  }
  if(row==5){
   animationsEnabled = !animationsEnabled;
   return;
  }
 }

 void CloseLauncherSettings()
 {
  if(modalOverlay!=null){
   Window.Default.Remove(modalOverlay);
   modalOverlay.Dispose();
   modalOverlay=null;
  }
  SaveConfig();
  PlayTick();
  BuildPage(false);
 }

 void RefreshSettings(){
  currentVolume=NativeCapabilities.GetMasterVolume(out var v)?v:-1;
  currentMute=NativeCapabilities.GetMasterMute(out var m)?m:-1;
  foreach(var id in new[]{"brightness","game-mode","sleeptimer"}){
   try{if(NativeCapabilities.GetItemValue(id,out var name,out var value)&&!string.IsNullOrEmpty(value)){settingValues[id]=value;settingNames[id]=name;}else{settingValues.Remove(id);settingNames.Remove(id);}
    settingEnabled[id]=NativeCapabilities.GetItemEnable(id,out var enabled)&&enabled;
   }catch(Exception ex){settingValues.Remove(id);settingNames.Remove(id);settingEnabled[id]=false;DebugLog.Warn("Settings", "Item query failed: " + id, ex);}
  }
  try{if(!NativeCapabilities.GetItemMinMaxStepValue("brightness",out brightnessMin,out brightnessMax,out brightnessStep)||brightnessStep<=0){brightnessMin=0;brightnessMax=50;brightnessStep=1;}}catch(Exception ex){brightnessMin=0;brightnessMax=50;brightnessStep=1;DebugLog.Warn("Settings", "Brightness step query failed", ex);}
 }

 string SettingValue(string id){return settingValues.TryGetValue(id,out var v)?v:"";}
 string SettingName(string id){return settingNames.TryGetValue(id,out var v)?v:"Unavailable";}
 bool CanEdit(string id){return settingEnabled.TryGetValue(id,out var enabled)&&enabled&&settingValues.ContainsKey(id);}
 void SetSetting(string id,string value){
  if(!CanEdit(id)){ShowError("This setting is unavailable for the current input.");return;}
  bool ok=false;try{ok=NativeCapabilities.SetItemValue(id,value);}catch(Exception ex){DebugLog.Error("Settings", "SetItemValue failed for " + id, ex);}
  RefreshSettings();UpdateFocus();
  bool verified=ok&&SettingValue(id)==value;
  if(!verified)ShowError("The TV could not apply this setting. Try Advanced settings.");
  WriteState("setting:"+id+":"+value+":"+verified);
 }

 void ChangeBrightness(int delta){double n;if(double.TryParse(SettingValue("brightness"),NumberStyles.Float,CultureInfo.InvariantCulture,out n))SetSetting("brightness",Math.Max(brightnessMin,Math.Min(brightnessMax,n+delta*brightnessStep)).ToString(CultureInfo.InvariantCulture));}
 void ChangeVolume(int delta){int n;bool ok=false;if(NativeCapabilities.GetMasterVolume(out n)){ok=NativeCapabilities.SetMasterVolume(Math.Max(0,Math.Min(100,n+delta)));}if(!ok)ShowError("Use the remote's volume buttons.");RefreshSettings();UpdateFocus();WriteState("volume_changed");}

 void SelectSource(int source)
 {
  try {
   int r = NativeCapabilities.SwitchHdmiSource(source);
   WriteState("source_switch:" + source + ":" + r);
   if (r == 0) {
    lastSource = source;
    return;
   }
   ShowError("Input could not be switched. Open Advanced settings to check the connection.");
   WriteState("source_failed:" + r);
  } catch(Exception ex) {
   DebugLog.Error("Source", "SelectSource exception for " + source, ex);
   ShowError(ex.Message);
  }
 }

 void Launch(string id,string menuItem=null)
 {
  try{
   notice.Text="";
   int r=NativeCapabilities.LaunchAppControl(id,menuItem);
   WriteState("launch:"+id+(menuItem==null?"":":"+menuItem)+":"+r);
   if(r==0)return;
   int res=NativeCapabilities.ExecuteSystem("app_launcher -s "+id);
   WriteState("launch:"+id+":sys:"+res);
   if(res!=0&&r!=0)ShowError("Could not open this screen. Try Advanced settings.");
  }catch(Exception ex){
   DebugLog.Error("App", "Launch exception for " + id, ex);
   ShowError(ex.Message);
  }
 }

 void ShowError(string text){notice.Text=text;}

 View debugOverlay;
 void ToggleDebugLogView()
 {
  if (debugOverlay != null)
  {
   Window.Default.Remove(debugOverlay);
   debugOverlay.Dispose();
   debugOverlay = null;
   return;
  }

  DebugLog.DumpToFile("/tmp/quiet-home-debuglog.txt");

  debugOverlay = new View {
   Position2D = new Position2D(0, 0),
   Size2D = new Size2D(ScreenWidth, ScreenHeight),
   BackgroundColor = new Color(0, 0, 0, 0.88f)
  };

  var box = new View {
   Position2D = new Position2D(SclX(180), SclY(80)),
   Size2D = new Size2D(SclX(1560), SclY(920)),
   BackgroundColor = darkMode ? new Color(0.10f, 0.12f, 0.18f, 0.98f) : new Color(0.96f, 0.96f, 0.98f, 0.98f),
   CornerRadius = new Vector4(SclX(24), SclX(24), SclX(24), SclX(24)),
   BorderlineWidth = SclX(3),
   BorderlineColor = accent
  };
  debugOverlay.Add(box);

  box.Add(new TextLabel {
   Text = $"Registro de Depuracion (Debug Log) - {DebugLog.ErrorCount} errores, {DebugLog.WarnCount} avisos",
   Position2D = new Position2D(SclX(40), SclY(24)),
   Size2D = new Size2D(SclX(1480), SclY(45)),
   PixelSize = SclF(30),
   FontFamily = "Quiet Inter Semibold",
   TextColor = ink
  });

  box.Add(new TextLabel {
   Text = "Atras (Back) o Aceptar para cerrar * Volcado guardado en /tmp/quiet-home-debuglog.txt",
   Position2D = new Position2D(SclX(40), SclY(72)),
   Size2D = new Size2D(SclX(1480), SclY(30)),
   PixelSize = SclF(18),
   FontFamily = "Quiet Inter Medium",
   TextColor = muted
  });

  var logs = DebugLog.GetAll();
  int start = Math.Max(0, logs.Count - 16);
  for (int i = start; i < logs.Count; i++)
  {
   int lineIdx = i - start;
   string line = logs[i];
   Color lineCol = line.Contains("[ERROR]") ? new Color(1f, 0.35f, 0.35f, 1) :
                   line.Contains("[WARN]") ? new Color(1f, 0.85f, 0.25f, 1) :
                   line.Contains("[STATE]") ? captionMuted : ink;

   box.Add(new TextLabel {
    Text = line,
    Position2D = new Position2D(SclX(40), SclY(115) + lineIdx * SclY(48)),
    Size2D = new Size2D(SclX(1480), SclY(42)),
    PixelSize = SclF(18),
    FontFamily = "Quiet Inter Medium",
    TextColor = lineCol
   });
  }

  Window.Default.Add(debugOverlay);
  debugOverlay.RaiseToTop();
 }

 void WriteState(string activity)
 {
  string summary = $"page={page} sel={selected} area={area} reord={reordering} key={lastKey} vol={currentVolume} mute={currentMute}";
  DebugLog.RecordState(activity, summary);
 }

 public static void Main(string[] args){using(var app=new QuietHome())app.Run(args);}
}

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
 Color ink => darkMode ? new Color(0.98f,0.98f,1.0f,1) : new Color(0.114f,0.114f,0.122f,1);
 Color muted => darkMode ? new Color(0.65f,0.70f,0.80f,1) : new Color(0.431f,0.431f,0.451f,1);
 readonly Color white=new Color(1,1,1,1);
 Color accent => darkMode ? new Color(0.40f,0.60f,1.0f,1) : new Color(0.24f,0.29f,0.72f,1);
 Color bg => darkMode ? new Color(0.082f,0.110f,0.184f,1) : new Color(0.96f,0.96f,0.97f,1);
 Color captionMuted => darkMode ? new Color(0.78f,0.83f,0.92f,1) : new Color(0.28f,0.32f,0.40f,1);
 Color descMuted => darkMode ? new Color(0.62f,0.68f,0.80f,1) : new Color(0.42f,0.46f,0.54f,1);
 [DllImport("libsource-api.so")] static extern int connect_source(int sourceType);
 [DllImport("libvconf.so.0")] static extern int vconf_get_int(string key,out int value);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_create(out IntPtr app);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_set_app_id(IntPtr app,string id);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_set_operation(IntPtr app,string operation);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_add_extra_data(IntPtr app,string key,string value);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_send_launch_request(IntPtr app,IntPtr cb,IntPtr data);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_destroy(IntPtr app);
 [DllImport("libc.so.6")] static extern int system(string command);
 [DllImport("libcapi-media-sound-manager-tv.so.0")] static extern int sound_manager_get_master_volume(out int value);
 [DllImport("libcapi-media-sound-manager-tv.so.0")] static extern int sound_manager_set_master_volume(int value);
 [DllImport("libcapi-media-sound-manager-tv.so.0")] static extern int sound_manager_get_master_mute(out int value);
 [DllImport("libcapi-media-sound-manager-tv.so.0")] static extern int sound_manager_set_master_mute(int value);
 [DllImport("libmmfkeysound.so.0")] static extern int mm_sound_play_keysound(string filename, int type);
 [DllImport("libsettings-api.so")] static extern bool CSharpGetItemValue(string id,out string name,out string value);
 [DllImport("libsettings-api.so")] static extern bool CSharpSetItemValue(string id,string value);
 [DllImport("libsettings-api.so")] static extern bool CSharpGetItemEnable(string id,out bool enabled);
 [DllImport("libsettings-api.so")] static extern bool CSharpGetItemMinMaxStepValue(string id,out double min,out double max,out double step);



 protected override void OnCreate()
 {
  base.OnCreate();
  
  try{fontLoaded=FontClient.Instance.AddCustomFontDirectory(Root+"fonts");}catch{fontLoaded=false;}
  try{
   string appsPath=Root+"apps.json";
   string tmpPath="/tmp/quiet-home-apps.json";
   if(File.Exists(tmpPath)&&File.Exists(appsPath)&&File.GetLastWriteTimeUtc(tmpPath)>File.GetLastWriteTimeUtc(appsPath))
    library=AppLibrary.Load(tmpPath);
   else if(File.Exists(appsPath))
    library=AppLibrary.Load(appsPath);
   else if(File.Exists(tmpPath))
    library=AppLibrary.Load(tmpPath);
  }catch{try{library=AppLibrary.Load(Root+"apps.json");}catch(Exception ex2){libraryError=ex2.Message;}}
  try{if(File.Exists("/tmp/quiet-home-theme.txt")){string t=File.ReadAllText("/tmp/quiet-home-theme.txt").Trim();if(t=="light")darkMode=false;else if(t=="dark")darkMode=true;}}catch{}
   Window.Default.BackgroundColor=bg;
  Window.Default.Title="Quiet Home";Window.Default.KeyEvent+=OnKey;
  PreloadArtwork();
  RefreshSource();selected=SourceIndex();BuildPage();
  clockTimer=new Tizen.NUI.Timer(30000);
  clockTimer.Tick+=(s,e)=>{if(clock!=null)clock.Text=DateTime.Now.ToString("ddd, MMM d · h:mm tt");return true;};clockTimer.Start();WriteState("created");
 }
 protected override void OnResume(){base.OnResume();clockTimer?.Start();RefreshSource();if(tabs!=null&&page==2){RefreshSettings();UpdateFocus(false);}WriteState("resumed");}
 protected override void OnAppControlReceived(AppControlReceivedEventArgs e){base.OnAppControlReceived(e);string requested; if(e.ReceivedAppControl.ExtraData.TryGet("quiet_page",out requested)&&requested=="settings")OpenSettings("settings_request");else if(requested=="apps"){page=1;area=1;selected=0;BuildPage(true);}else Home("home_request");}
 protected override void OnPause(){base.OnPause();CancelLongPress();if(reordering)StopReordering();editing="";clockTimer?.Stop();WriteState("paused");}
 void Home(string activity){CancelLongPress();if(reordering)StopReordering();clockTimer?.Start();RefreshSource();if(tabs!=null){bool unchanged=page==0&&area==1&&editing==""&&selected==SourceIndex();page=0;area=1;editing="";selected=SourceIndex();if(!unchanged)BuildPage();WriteState(activity);}}
 void OpenSettings(string activity){CancelLongPress();if(reordering)StopReordering();clockTimer?.Start();page=2;area=1;selected=0;editing="";BuildPage(true);WriteState(activity);}
 void RefreshSource(){try{int n;if(vconf_get_int("memory/system/source_type",out n)==0&&n>=13&&n<=16)lastSource=n;}catch{}}
 int SourceIndex(){return lastSource==14?1:lastSource==15?2:0;}
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
  var stopwatch=Stopwatch.StartNew();
  if(focusAnimation!=null){focusAnimation.Stop();focusAnimation.Dispose();focusAnimation=null;}
  if(pageAnimation!=null){pageAnimation.Stop();pageAnimation.Dispose();pageAnimation=null;}
  ParkArtwork();pageArtworkCold=0;
  foreach(var v in scene){Window.Default.Remove(v);v.Dispose();}scene.Clear();slots.Clear();
  pageContainer=null;
  Window.Default.BackgroundColor=bg;
  AddWindow(new ImageView{ResourceUrl=Root+(darkMode?"free-background-dark.svg":"free-background.svg"),Size2D=new Size2D(1920,1080)});
  tabs=new TextLabel[3];string[] nav={"Inputs","Apps","Settings"};int[] tx={54,221,366};int[] tw={153,131,182};
  for(int i=0;i<3;i++){tabs[i]=TextWindow(nav[i],tx[i],158,tw[i],68,30,muted);tabs[i].HorizontalAlignment=HorizontalAlignment.Center;}
  clock=TextWindow(DateTime.Now.ToString("ddd, MMM d · h:mm tt"),1200,170,640,45,24,muted);clock.HorizontalAlignment=HorizontalAlignment.End;

  pageContainer=new View{
   Size2D=new Size2D(1920,1080),
   Position=new Position(960,540,0),
   PivotPoint=new Position(0.5f,0.5f,0.5f),
   PositionUsesPivotPoint=true,
   BackgroundColor=new Color(0,0,0,0)
  };
  Window.Default.Add(pageContainer);
  scene.Add(pageContainer);

  var title=TextWindow(page==0?"Home":page==1?"Your Apps":"Quick Settings",76,55,1350,100,76,ink,"Display");

  if(page==0){
   var hero=Feature("PlayStation 4 / PC","hdmi:13","Console or PC · HDMI 1","feature-ps4.png",80,262,1040,442,"hero");
   hero.Card.Add(Label("PLAYSTATION 4",44,52,550,40,24,new Color(.86f,.91f,1,1),"Semibold"));
   hero.Card.Add(Label("Let's play.",40,124,560,100,72,white,"Display"));
   hero.Card.Add(Label("Switch to HDMI 1",46,246,440,48,30,new Color(.88f,.93f,1,1)));
   Feature("Nintendo Switch","hdmi:14","Game console · HDMI 2","feature-switch.png",1160,262,320,442,"portrait");
   Feature("HDMI 3","hdmi:15","External device · HDMI 3","feature-hdmi.svg",1520,262,320,442,"portrait");
   Add(new ImageView{ResourceUrl=Root+(darkMode?"glass-shelf-dark.svg":"glass-shelf.svg"),Position2D=new Position2D(0,784),Size2D=new Size2D(1920,280)});
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
   SettingsTile("Volume","volume","Select · Left / Right to adjust","volume",0,0);
   SettingsTile("Brightness","brightness","Select · Left / Right to adjust","brightness",1,0);
   SettingsTile("Game Mode","game-mode","Select to switch on or off","game",2,0);
   SettingsTile("Sleep Timer","sleeptimer","Select to change the timer","sleep",3,0);
   SettingsTile("Sound","mute","Select to mute or unmute","mute",0,1);
   SettingsTile("Controllers","controllers","DualSense · hold PS + Create","controller",1,1);
   SettingsTile("Network","network","Wi-Fi and connection setup","network",2,1);
   SettingsTile("Advanced","app:org.tizen.menu","Open Samsung's full settings","settings",3,1);
  }

  var animContainer=pageContainer;
  pageContainer=null;

  Toolbar("Wi-Fi","network","wifi",0,"Wi-Fi · Connection setup");
  Toolbar("Inputs","page:0","hdmi",1,"Inputs · HDMI & sources");
  Toolbar("Settings","app:org.tizen.menu","settings",2,"Settings · Open Samsung settings");
  Toolbar(darkMode?"Dark Mode":"Light Mode","toggle:theme",darkMode?"moon":"sun",3);
  toolbarHint=TextWindow("",1400,170,440,45,24,ink);toolbarHint.HorizontalAlignment=HorizontalAlignment.End;
  detail=TextWindow("",80,1030,1320,35,20,muted);
  notice=TextWindow("",80,1008,1760,34,22,accent);
  var hint=TextWindow(reordering?"Flechas: mover · OK: fijar · Back: salir":page==1?(AppLibrary.PageCount(library.Count)<=1?"Select to open · Back to Home":libraryPage+1<AppLibrary.PageCount(library.Count)?"Down for more apps · Back to Home":"Up for previous apps · Back to Home"):"Back to input",1310,1030,530,35,20,muted);hint.HorizontalAlignment=HorizontalAlignment.End;

  RebuildRowCache();lastFocusedSlot=-1;
  selected=Math.Max(0,Math.Min(selected,slots.Count-1));UpdateFocus(false);

  if(animate){
   animContainer.Opacity=0.0f;
   animContainer.Scale=new Vector3(0.982f,0.982f,1.0f);
   animContainer.Position=new Position(960,540+24,0);
   title.Opacity=0.0f;
   title.Position=new Position(76,55+4,0);
   pageAnimation=new Animation(230);
   pageAnimation.AnimateTo(animContainer,"Opacity",1.0f,new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.AnimateTo(animContainer,"Scale",new Vector3(1.0f,1.0f,1.0f),new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.AnimateTo(animContainer,"Position",new Position(960,540,0),new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.AnimateTo(title,"Opacity",1.0f,new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.AnimateTo(title,"Position",new Position(76,55,0),new AlphaFunction(AlphaFunction.BuiltinFunctions.EaseOut));
   pageAnimation.Play();
  }

  buildMilliseconds=stopwatch.ElapsedMilliseconds;WriteState("page_ready");
 }
 void BuildLibrary()
 {
  if(library.Count==0){Text("Apps could not be loaded.",80,300,1500,70,32,ink);return;}
  libraryPage=Math.Clamp(libraryPage,0,AppLibrary.PageCount(library.Count)-1);
  int start=libraryPage*AppLibrary.PageSize,count=AppLibrary.VisibleCount(library.Count,libraryPage);
  for(int i=0;i<count;i++)
  {
   var app=library[start+i];int x=80+(i%5)*358,y=266+(i/5)*245,w=328,h=184;
   var shadow=Add(new ImageView{ResourceUrl=Root+"shadow-app.svg",Position2D=new Position2D(x-24,y-10),Size2D=new Size2D(w+48,h+64)});
   var card=Panel(x+w/2,y+h/2,w,h,HexColor(app.Background),24);
   card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
   AddAppArtwork(card,app,w,h);
   var ring=new View{Size2D=new Size2D(w,h),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(24,24,24,24),BorderlineWidth=4,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
   card.Add(ring);
   slots.Add(new Slot{Card=card,Shadow=shadow,FocusRing=ring,Label=null,Name=app.Name,Action="app:"+app.Id,Description=app.Description,Row=i/5,X=x,Y=y,W=w,H=h});
  }
  Text(library.FindAll(a=>!a.Utility).Count+" apps",80,218,500,30,21,muted);
  if(AppLibrary.PageCount(library.Count)>1){
   var pages=Text((libraryPage+1)+" / "+AppLibrary.PageCount(library.Count),1650,218,190,30,21,muted);
   pages.HorizontalAlignment=HorizontalAlignment.End;
  }
 }
 void PlayTick(){try{mm_sound_play_keysound(Root+"click.wav", 0);}catch{}}
 Color HexColor(string hex)
 {
  try{int n=Convert.ToInt32(hex.TrimStart('#'),16);return new Color(((n>>16)&255)/255f,((n>>8)&255)/255f,(n&255)/255f,1);}
  catch{return white;}
 }
 void AddAppArtwork(View card,LibraryApp app,int w,int h)
 {
  if(!app.Utility){
   var cached=GetArtwork(app,w,h);
   if(cached.Image.LoadingStatus!=ImageView.LoadingStatusType.Ready)pageArtworkCold++;
   cached.Parent.Remove(cached.Image);card.Add(cached.Image);cached.Parent=card;
  }
  if(app.Utility){
   card.Add(new View{Position2D=new Position2D(w/2-28,h/2-2),Size2D=new Size2D(56,4),BackgroundColor=ink});
   card.Add(new View{Position2D=new Position2D(w/2-2,h/2-28),Size2D=new Size2D(4,56),BackgroundColor=ink});
  }
 }
 void PreloadArtwork()
 {
  // Keep the real image views alive on the scene; moving between pages never
  // destroys their decoded textures. The zero-opacity parent is never drawn.
  artworkPreloadRoot=new View{Size2D=new Size2D(1920,1080),Opacity=0};
  Window.Default.Add(artworkPreloadRoot);
  PreloadFeature("feature-ps4.png",1040,442);
  PreloadFeature("feature-switch.png",320,442);
  PreloadFeature("feature-hdmi.svg",320,442);
  string[] quickIcons={"volume","brightness","game","sleep","mute","controller","network","settings"};
  foreach(var icon in quickIcons){
   var img=new ImageView{ResourceUrl=Root+"quick-"+icon+".png",Size2D=new Size2D(410,286),DesiredWidth=820,DesiredHeight=572,SamplingMode=SamplingModeType.BoxThenLanczos,ReleasePolicy=ReleasePolicyType.Never,CornerRadius=new Vector4(24,24,24,24)};
   quickCache[icon]=img;
   artworkPreloadRoot.Add(img);
  }
  foreach(var app in library)if(!app.Utility)GetArtwork(app,328,184);
  foreach(var id in new[]{"org.tizen.netflix-app","com.samsung.tv.cobalt-yt","org.tizen.primevideo"}){
   var app=library.Find(a=>a.Id==id);if(app!=null)GetArtwork(app,253,126);
  }
 }
 CachedArtwork GetArtwork(LibraryApp app,int w,int h)
 {
  string key=app.Id+"@"+w+"x"+h;
  if(artworkCache.TryGetValue(key,out var cached))return cached;
  bool cover=app.Fit=="cover",full=cover||app.Fit=="contain-full";
  int iw=full?w:w-54,ih=full?h:h-40;var crop=app.Crop;
  float cropWidth=crop==null?1:crop[2],cropHeight=crop==null?1:crop[3];
  var image=new ImageView{
   Position2D=new Position2D((w-iw)/2,(h-ih)/2),Size2D=new Size2D(iw,ih),
   FittingMode=cover?FittingModeType.ScaleToFill:FittingModeType.ShrinkToFit,
   SamplingMode=SamplingModeType.BoxThenLanczos,
   DesiredWidth=(int)Math.Ceiling(iw*2/cropWidth),DesiredHeight=(int)Math.Ceiling(ih*2/cropHeight),
   ReleasePolicy=ReleasePolicyType.Destroyed,CornerRadius=new Vector4(24,24,24,24)};
  if(crop!=null)image.PixelArea=new RelativeVector4(crop[0],crop[1],crop[2],crop[3]);
  cached=new CachedArtwork{Image=image,Parent=artworkPreloadRoot};
  artworkCache.Add(key,cached);artworkViewsCreated++;
  image.ResourceReady+=(s,e)=>{artworkReadyCount++;WriteState("artwork_ready");};
  artworkPreloadRoot.Add(image);image.ResourceUrl=Root+app.Asset;
  return cached;
 }
 void PreloadFeature(string asset,int w,int h)
 {
  if(!featureCache.ContainsKey(asset)){
   var img=new ImageView{ResourceUrl=Root+asset,Size2D=new Size2D(w,h),DesiredWidth=w*2,DesiredHeight=h*2,SamplingMode=SamplingModeType.BoxThenLanczos,ReleasePolicy=ReleasePolicyType.Never,CornerRadius=new Vector4(28,28,28,28)};
   featureCache[asset]=img;
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
 void SettingsTile(string name,string action,string description,string icon,int column,int row)
 {
  int x=80+column*450,y=262+row*332,w=410,h=286;
  var card=Panel(x+w/2,y+h/2,w,h,darkMode?new Color(0.12f,0.15f,0.22f,1):new Color(0.92f,0.93f,0.95f,1),24);card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
  ImageView tileImg;
  if(!quickCache.TryGetValue(icon,out tileImg)){
   tileImg=new ImageView{ResourceUrl=Root+"quick-"+icon+".png",Size2D=new Size2D(w,h),DesiredWidth=820,DesiredHeight=572,SamplingMode=SamplingModeType.BoxThenLanczos,ReleasePolicy=ReleasePolicyType.Never,CornerRadius=new Vector4(24,24,24,24)};
   quickCache[icon]=tileImg;
  }
  card.Add(tileImg);
  card.Add(Label(name,26,25,348,42,29,white,"Semibold"));
  var value=Label("",26,151,358,56,38,white,"Display");card.Add(value);
  card.Add(Label(description,26,226,370,34,20,new Color(.95f,.96f,1,1)));
  var ring=new View{Size2D=new Size2D(w,h),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(24,24,24,24),BorderlineWidth=4,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
  card.Add(ring);
  slots.Add(new Slot{Card=card,FocusRing=ring,Label=value,Name=name,Action=action,Description=description,Row=row,X=x,Y=y,W=w,H=h});
 }
 Slot Feature(string label,string action,string description,string asset,int x,int y,int w,int h,string shape)
 {
  var shadow=Add(new ImageView{ResourceUrl=Root+"shadow-"+shape+".svg",Position2D=new Position2D(x-24,y-10),Size2D=new Size2D(w+48,h+64)});
  var card=Panel(x+w/2,y+h/2,w,h,new Color(0,0,0,0),28);card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
  ImageView featImg;
  if(!featureCache.TryGetValue(asset,out featImg)){
   featImg=new ImageView{ResourceUrl=Root+asset,Size2D=new Size2D(w,h),DesiredWidth=w*2,DesiredHeight=h*2,SamplingMode=SamplingModeType.BoxThenLanczos,ReleasePolicy=ReleasePolicyType.Never,CornerRadius=new Vector4(28,28,28,28)};
   featureCache[asset]=featImg;
   artworkPreloadRoot.Add(featImg);
  }
  featImg.CornerRadius=new Vector4(28,28,28,28);
  if(featImg.GetParent()!=null)featImg.GetParent().Remove(featImg);
  card.Add(featImg);
  var ring=new View{Size2D=new Size2D(w,h),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(28,28,28,28),BorderlineWidth=4,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
  card.Add(ring);
  var caption=Text(label,x,716,w,45,31,captionMuted,"Semibold");
  var descLabel=Text(description,x,760,w,36,25,descMuted,"Medium");
  var slot=new Slot{Card=card,Shadow=shadow,FocusRing=ring,Label=caption,Desc=descLabel,Name=label,Action=action,Description=description,Row=0,X=x,Y=y,W=w,H=h};slots.Add(slot);return slot;
 }
 void Dock(string label,string action,string asset,int n)
 {
  int x=(int)Math.Round(80+n*(253.333+48));
  var shadow=Add(new ImageView{ResourceUrl=Root+"shadow-dock.svg",Position2D=new Position2D(x-24,842),Size2D=new Size2D(301,190)});
  var card=Panel(x+126,917,253,126,new Color(0,0,0,0),24);card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
  string dockPng=Root+"free-dock-"+asset+".png";
  string dockSvg=Root+"free-dock-"+asset+".svg";
  if(File.Exists(dockPng)){
   card.Add(new ImageView{ResourceUrl=dockPng,Size2D=new Size2D(253,126),DesiredWidth=506,DesiredHeight=252,SamplingMode=SamplingModeType.BoxThenLanczos});
  }else if(File.Exists(dockSvg)){
   card.Add(new ImageView{ResourceUrl=dockSvg,Size2D=new Size2D(253,126)});
  }else{
   var official=library.Find(a=>a.Id==action.Substring(action.IndexOf(':')+1));
   if(official!=null){card.BackgroundColor=HexColor(official.Background);AddAppArtwork(card,official,253,126);}
   else card.Add(new ImageView{ResourceUrl=dockSvg,Size2D=new Size2D(253,126)});
  }
  var ring=new View{Size2D=new Size2D(253,126),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(24,24,24,24),BorderlineWidth=4,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
  card.Add(ring);
  slots.Add(new Slot{Card=card,Shadow=shadow,FocusRing=ring,Name=label,Action=action,Description=label,Row=1,X=x,Y=854,W=253,H=126});
 }
 void Toolbar(string label,string action,string icon,int n,string description="")
 {
  int x=1508+n*88;
  var card=Panel(x+34,103,68,68,new Color(0,0,0,0),34);card.PivotPoint=new Position(.5f,.5f,.5f);card.PositionUsesPivotPoint=true;
  card.Add(new ImageView{ResourceUrl=Root+(darkMode?"glass-button-dark.svg":"glass-button.svg"),Position2D=new Position2D(-16,-16),Size2D=new Size2D(100,112)});
  card.Add(new ImageView{ResourceUrl=Root+"symbol-"+icon+".png",Color=ink,Position2D=new Position2D(19,19),Size2D=new Size2D(30,30)});
  var ring=new View{Size2D=new Size2D(68,68),BackgroundColor=new Color(0,0,0,0),CornerRadius=new Vector4(34,34,34,34),BorderlineWidth=3,BorderlineOffset=-1.0f,BorderlineColor=accent,Opacity=0};
  card.Add(ring);
  slots.Add(new Slot{Card=card,FocusRing=ring,Name=label,Action=action,Description=string.IsNullOrEmpty(description)?label:description,Row=-1,X=x,Y=69,W=68,H=68});
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
    cur.FocusRing.BorderlineWidth=reordering?6:4;
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
  lastFocusedSlot=curFocused;
  if(focusAnimation!=null)focusAnimation.Play();
  clock.Opacity=area==3?0:1;toolbarHint.Text=area==3?slots[selected].Name:"";
  detail.Text=area==3?(slots[selected].Action=="toggle:theme"?"Select to switch to "+(darkMode?"Light Mode":"Dark Mode"):slots[selected].Description):page==1&&area==1&&slots[selected].Row>=0?slots[selected].Name+" · "+slots[selected].Description:area==0?"":area==2?slots[selected].Name:"";
  if(page==2){
   slots[0].Label.Text=currentVolume<0?"Unavailable":currentVolume.ToString();
   slots[1].Label.Text=SettingDisplay("brightness");
   slots[2].Label.Text=SettingDisplay("game-mode");
   slots[3].Label.Text=SettingDisplay("sleeptimer");
   slots[4].Label.Text=currentMute<0?"Unavailable":currentMute==1?"Muted":"Sound on";
   slots[5].Label.Text="Pair controller";
   slots[6].Label.Text="Connections";
   slots[7].Label.Text="More settings";
   detail.Text="Your everyday controls";
  }
  if(reordering&&curFocused>=0&&curFocused<slots.Count)notice.Text=$"Mover '{slots[curFocused].Name}' con flechas · OK para fijar";
  if(editing!="")detail.Text="Left / Right to adjust "+editing+" · Select or Back to finish";
  WriteState("focus");
 }
 void OnKey(object sender,Window.KeyEventArgs e)
 {
  if(e.Key==null)return;
  string k=e.Key.KeyPressedName;
  var state=e.Key.State;

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
    if(area==2){int col=Row(1).IndexOf(selected);area=1;selected=Row(0)[page==2?col:0];}
    else if(area==1){area=0;}
    else if(area==0){area=3;var r=Row(-1);selected=r.Count>0?r[Math.Min(page,r.Count-1)]:0;}
    UpdateFocus();
   }
   else if(k=="Down"){
    if(area==3){area=0;}
    else if(area==0){area=1;selected=Row(0)[0];}
    else if(area==1){int col=Row(0).IndexOf(selected);area=2;selected=Row(1)[page==2?col:0];}
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
    else{var r=Row(area-1);int n=r.IndexOf(selected);selected=r[(n+d+r.Count)%r.Count];UpdateFocus();}
   }
   else if(k=="Return"||k=="KP_Enter"){if(area==0){area=1;selected=0;UpdateFocus();}else Activate();}
   else if(k=="XF86Back"||k=="Back"||k=="Escape"||k=="XF86Exit")SelectSource(lastSource);
  }catch(Exception ex){ShowError(ex.Message);}
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
  notice.Text=$"Mover '{slots[selected].Name}' con flechas · OK para fijar";
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
   AppLibrary.Save(Root+"apps.json",library);
   WriteState("library_saved_root");
  }catch(Exception ex){
   WriteState("library_save_root_err:"+ex.Message);
  }
  try{
   AppLibrary.Save("/tmp/quiet-home-apps.json",library);
   WriteState("library_saved_tmp");
  }catch(Exception ex){
   WriteState("library_save_tmp_err:"+ex.Message);
  }
 }
 bool HandleLibraryKey(string key)
 {
  if(reordering)
  {
   if(key=="XF86Back"||key=="Back"||key=="Escape"||key=="XF86Exit"||key=="Return"||key=="KP_Enter")
   {
    StopReordering();
    return true;
   }
   int abs=libraryPage*AppLibrary.PageSize+selected;
   int next=abs;
   if(key=="Left"&&abs>0) next=abs-1;
   else if(key=="Right"&&abs+1<library.Count) next=abs+1;
   else if(key=="Up"&&abs>=AppLibrary.Columns) next=abs-AppLibrary.Columns;
   else if(key=="Down"&&abs+AppLibrary.Columns<library.Count) next=abs+AppLibrary.Columns;

   if(next!=abs)
   {
    var tmp=library[abs];
    library[abs]=library[next];
    library[next]=tmp;
    int nextPage=next/AppLibrary.PageSize;
    selected=next%AppLibrary.PageSize;
    libraryPage=nextPage;
    PlayTick();
    BuildPage(false);
    UpdateFocus(false);
    notice.Text=$"Mover '{slots[selected].Name}' con flechas · OK para fijar";
   }
   return true;
  }

  if(key=="XF86Back"||key=="Back"||key=="Escape"||key=="XF86Exit"){Home("apps_back");return true;}
  if(library.Count==0)return key=="Up"||key=="Down";
  if((area==0||area==3)&&key=="Down"){area=1;selected=0;UpdateFocus();return true;}
  if(area!=1)return false;

  if(key=="Return"||key=="KP_Enter")
  {
   waitingForLongPress=true;
   longPressTriggered=false;
   if(longPressTimer!=null){longPressTimer.Stop();longPressTimer.Dispose();}
   longPressTimer=new Tizen.NUI.Timer(500);
   longPressTimer.Tick+=(s,e)=>{
    waitingForLongPress=false;
    longPressTriggered=true;
    if(longPressTimer!=null){longPressTimer.Stop();longPressTimer.Dispose();longPressTimer=null;}
    StartReordering();
    return false;
   };
   longPressTimer.Start();
   return true;
  }

  CancelLongPress();

  if(key=="Left"||key=="Right"||key=="Up"||key=="Down"){
   int next=AppLibrary.Move(library.Count,libraryPage*AppLibrary.PageSize+selected,key);
   if(next<0){area=0;UpdateFocus();return true;}
   int nextPage=next/AppLibrary.PageSize;selected=next%AppLibrary.PageSize;
   if(nextPage!=libraryPage){libraryPage=nextPage;PlayTick();BuildPage(true);}else UpdateFocus();
   return true;
  }
  return false;
 }
 void Activate()
 {PlayTick();
  string a=slots[selected].Action;notice.Text="";
  if(a=="toggle:theme"){
   darkMode=!darkMode;
   try{File.WriteAllText("/tmp/quiet-home-theme.txt",darkMode?"dark":"light");}catch{}
   int prevArea=area;int prevSelected=selected;
   BuildPage();
   area=prevArea;selected=prevSelected;UpdateFocus(false);
   return;
  }
  if(a.StartsWith("hdmi:")){SelectSource(int.Parse(a.Substring(5)));return;}
  if(a.StartsWith("page:")){page=int.Parse(a.Substring(5));selected=page==0?SourceIndex():0;area=1;BuildPage(true);return;}
  if(a.StartsWith("app:")){Launch(a.Substring(4));return;}
  if(a=="controllers"){Launch("org.tizen.menu","add-bluetooth-keyboard");return;}
  if(a=="network"){Launch("org.tizen.menu","networksettings");return;}
  if(a=="volume"||a=="brightness"){RefreshSettings();if(a=="brightness"&&!CanEdit(a)){ShowError("Brightness is unavailable for the current input.");return;}editing=a;UpdateFocus();return;}
  if(a=="game-mode"){RefreshSettings();SetSetting(a,SettingValue(a)=="1"?"0":"1");return;}
  if(a=="sleeptimer"){RefreshSettings();int n;if(int.TryParse(SettingValue(a),out n))SetSetting(a,((n+1)%5).ToString());return;}
  if(a=="mute"){int n;int r=sound_manager_get_master_mute(out n);if(r==0)r=sound_manager_set_master_mute(n==1?0:1);if(r!=0)ShowError("Use the remote's mute button.");RefreshSettings();UpdateFocus();WriteState("mute_changed");}
 }
 void RefreshSettings(){
  currentVolume=sound_manager_get_master_volume(out var v)==0?v:-1;
  currentMute=sound_manager_get_master_mute(out var m)==0?m:-1;
  foreach(var id in new[]{"brightness","game-mode","sleeptimer"}){
   try{if(CSharpGetItemValue(id,out var name,out var value)&&!string.IsNullOrEmpty(value)){settingValues[id]=value;settingNames[id]=name;}else{settingValues.Remove(id);settingNames.Remove(id);}
    settingEnabled[id]=CSharpGetItemEnable(id,out var enabled)&&enabled;
   }catch{settingValues.Remove(id);settingNames.Remove(id);settingEnabled[id]=false;}
  }
  try{if(!CSharpGetItemMinMaxStepValue("brightness",out brightnessMin,out brightnessMax,out brightnessStep)||brightnessStep<=0){brightnessMin=0;brightnessMax=50;brightnessStep=1;}}catch{}
 }
 string SettingDisplay(string id)
 {
  if(!settingValues.TryGetValue(id,out var val)||!settingNames.TryGetValue(id,out var name))return "Unavailable";
  string lower=name.Trim().ToLowerInvariant();
  if(id=="game-mode"){
   if(val=="1"||lower.Contains("encendido")||lower.Contains("activado")||lower.Contains("on"))return "Turned On";
   return "Off";
  }
  if(id=="sleeptimer"){
   if(val=="0"||lower.Contains("apagado")||lower.Contains("off")||lower.Contains("desactivado"))return "Off";
   if(lower.Contains("encendido")||lower.Contains("activado")||lower.Contains("on"))return "Turned On";
   return name;
  }
  if(lower=="apagado"||lower=="desactivado")return "Off";
  if(lower=="encendido"||lower=="activado")return "Turned On";
  return name;
 }
 string SettingValue(string id){return settingValues.TryGetValue(id,out var v)?v:"";}
 string SettingName(string id){return settingNames.TryGetValue(id,out var v)?v:"Unavailable";}
 bool CanEdit(string id){return settingEnabled.TryGetValue(id,out var enabled)&&enabled&&settingValues.ContainsKey(id);}
 void SetSetting(string id,string value){
  if(!CanEdit(id)){ShowError("This setting is unavailable for the current input.");return;}
  bool ok=false;try{ok=CSharpSetItemValue(id,value);}catch{}
  RefreshSettings();UpdateFocus();
  bool verified=ok&&SettingValue(id)==value;
  if(!verified)ShowError("The TV could not apply this setting. Try Advanced settings.");
  WriteState("setting:"+id+":"+value+":"+verified);
 }
 void ChangeBrightness(int delta){double n;if(double.TryParse(SettingValue("brightness"),NumberStyles.Float,CultureInfo.InvariantCulture,out n))SetSetting("brightness",Math.Max(brightnessMin,Math.Min(brightnessMax,n+delta*brightnessStep)).ToString(CultureInfo.InvariantCulture));}
 void ChangeVolume(int delta){int n;int r=sound_manager_get_master_volume(out n);if(r==0)r=sound_manager_set_master_volume(Math.Max(0,Math.Min(100,n+delta)));if(r!=0)ShowError("Use the remote's volume buttons.");RefreshSettings();UpdateFocus();WriteState("volume_changed");}
 void SelectSource(int source)
 {
  try {
   IntPtr c;
   int r = app_control_create(out c);
   if (r == 0) {
    try {
     app_control_set_app_id(c, "org.tizen.tv-viewer");
     app_control_set_operation(c, "http://tizen.org/appcontrol/operation/default");
     app_control_add_extra_data(c, "CALLER", "org.tizen.source-list");
     app_control_add_extra_data(c, "change_source", source.ToString());
     app_control_add_extra_data(c, "mbrIndex", "-2");
     app_control_add_extra_data(c, "sourceswitch_banner", "disable");
     app_control_add_extra_data(c, "__K_EX_FORCE_BOOST", "enable");
     r = app_control_send_launch_request(c, IntPtr.Zero, IntPtr.Zero);
     WriteState("source_switch:" + source + ":" + r);
     if (r == 0) {
      lastSource = source;
      return;
     }
    } finally {
     app_control_destroy(c);
    }
   }
   ShowError("Input could not be switched. Open Advanced settings to check the connection.");
   WriteState("source_failed:" + r);
  } catch(Exception ex) {
   ShowError(ex.Message);
  }
 }
 void Launch(string id,string menuItem=null)
 {
  try{
   notice.Text="";
   IntPtr c;int r=app_control_create(out c);
   if(r==0){
    try{
     app_control_set_app_id(c,id);
     app_control_set_operation(c,"http://tizen.org/appcontrol/operation/default");
     if(id=="org.tizen.menu"&&menuItem!=null){
      app_control_add_extra_data(c,"menuitem",menuItem);
      app_control_add_extra_data(c,"launchoption","pressed");
     }
     app_control_add_extra_data(c,"__K_EX_FORCE_BOOST","enable");
     app_control_add_extra_data(c,"__K_SPLASH_SCREEN","LA_UI_VR_TRANSPARENT");
     r=app_control_send_launch_request(c,IntPtr.Zero,IntPtr.Zero);
     WriteState("launch:"+id+(menuItem==null?"":":"+menuItem)+":"+r);
     if(r==0)return;
    }finally{app_control_destroy(c);}
   }
   int res=system("app_launcher -s "+id);
   WriteState("launch:"+id+":sys:"+res);
   if(res!=0&&r!=0)ShowError("Could not open this screen. Try Advanced settings.");
  }catch(Exception ex){ShowError(ex.Message);}
 }
 void ShowError(string text){notice.Text=text;}
 void WriteState(string activity){try{File.WriteAllText(StateFile,DateTime.UtcNow.ToString("o")+"\npage="+page+"\nselected="+selected+"\narea="+area+"\nreordering="+reordering+"\nactivity="+activity+"\nkey="+lastKey+"\nfont_loaded="+fontLoaded+"\nstyle=free-my-tv\nbuild_ms="+buildMilliseconds+"\nlibrary_page="+libraryPage+"\nlibrary_count="+library.Count+"\nlibrary_error="+libraryError+"\nselected_name="+(selected>=0&&selected<slots.Count?slots[selected].Name:"")+"\nartwork_cached="+artworkCache.Count+"\nartwork_ready="+artworkReadyCount+"\nartwork_pending="+ArtworkPending()+"\nartwork_views_created="+artworkViewsCreated+"\npage_artwork_cold="+pageArtworkCold+"\ntheme="+(darkMode?"dark":"light")+"\nprocess_id="+ProcessId+"\nediting="+editing+"\nvolume="+currentVolume+"\nmute="+currentMute+"\nbrightness="+SettingValue("brightness")+"\ngame-mode="+SettingValue("game-mode")+"\nsleeptimer="+SettingValue("sleeptimer")+"\n");}catch{}}
 public static void Main(string[] args){using(var app=new QuietHome())app.Run(args);}
}

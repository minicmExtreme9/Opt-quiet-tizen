using System;
using System.Runtime.InteropServices;

public static class Probe {
 [DllImport("libcapi-appfw-app-operation-tv.so.0")] static extern int app_operation_get_current_app(string operation,out IntPtr app);
 [DllImport("libcapi-appfw-app-operation-tv.so.0")] static extern int app_operation_set_current_app(string operation,string app);
 [DllImport("libsource-api.so")] static extern IntPtr get_source_list();
 [DllImport("libsource-api.so")] static extern int connect_source(int sourceType);
 [DllImport("libvconf.so.0")] static extern int vconf_get_int(string key,out int value);
 [DllImport("libc.so.6")] static extern void free(IntPtr p);
 [DllImport("libc.so.6")] static extern int system(string command);
 [DllImport("libcapi-media-sound-manager-tv.so.0")] static extern int sound_manager_get_master_volume(out int value);
 [DllImport("libcapi-media-sound-manager-tv.so.0")] static extern int sound_manager_get_master_mute(out int value);
 [DllImport("libmmfkeysound.so.0")] static extern int mm_sound_play_keysound(string filename, int volume);
 [DllImport("libfeedback.so.0")] static extern int feedback_initialize();
 [DllImport("libfeedback.so.0")] static extern int feedback_play_type(int type, int pattern);
 [DllImport("libfeedback.so.0")] static extern int feedback_set_resource_path(int type, int pattern, string path);
 [DllImport("libfeedback.so.0")] static extern int feedback_get_resource_path(int type, int pattern, out IntPtr path);
 [DllImport("libfeedback.so.0")] static extern int feedback_is_supported_pattern(int type, int pattern);
 [DllImport("libfeedback.so.0")] static extern int feedback_deinitialize();
 [DllImport("libcapi-media-wav-player.so.0")] static extern int wav_player_play_simple(string path, int sound_type);

 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_create(out IntPtr app_control);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_destroy(IntPtr app_control);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_set_app_id(IntPtr app_control,string app_id);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_set_operation(IntPtr app_control,string operation);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_add_extra_data(IntPtr app_control,string key,string value);
 [DllImport("libcapi-appfw-app-control.so.0")] static extern int app_control_send_launch_request(IntPtr app_control,IntPtr reply_cb,IntPtr user_data);

 [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
 delegate void SourceListCallback(int id, IntPtr itemPtr, IntPtr userData);
 [DllImport("libsource-api.so")] static extern int sourceapi_get_sourcelist(SourceListCallback cb);

 static void OnSourceItem(int id, IntPtr itemPtr, IntPtr userData) {
  try {
   int srcType = Marshal.ReadInt32(itemPtr, 0);
   IntPtr pName = Marshal.ReadIntPtr(itemPtr, 4);
   IntPtr pUnique = Marshal.ReadIntPtr(itemPtr, 8);
   IntPtr pIcon = Marshal.ReadIntPtr(itemPtr, 12);
   IntPtr pStr4 = Marshal.ReadIntPtr(itemPtr, 16);
   IntPtr pOsd = Marshal.ReadIntPtr(itemPtr, 20);
   string sName = pName != IntPtr.Zero ? Marshal.PtrToStringAnsi(pName) : "";
   string sUnique = pUnique != IntPtr.Zero ? Marshal.PtrToStringAnsi(pUnique) : "";
   string sIcon = pIcon != IntPtr.Zero ? Marshal.PtrToStringAnsi(pIcon) : "";
   string sOsd = pOsd != IntPtr.Zero ? Marshal.PtrToStringAnsi(pOsd) : "";
   Console.WriteLine($"SOURCE id={id} type={srcType} name='{sName}' unique='{sUnique}' osd='{sOsd}' icon='{sIcon}'");
  } catch(Exception ex) {
   Console.WriteLine("Error in callback: " + ex);
  }
 }

 public static int Main(string[] args) {
  try {
   if(args.Length >= 2 && args[0] == "test-mmf") {
    string path = args[1];
    int vol = args.Length >= 3 ? int.Parse(args[2]) : 0;
    Console.WriteLine($"Calling mm_sound_play_keysound('{path}', {vol})...");
    for(int i = 0; i < 5; i++) {
     int res = mm_sound_play_keysound(path, vol);
     Console.WriteLine($"Iter {i}: mm_sound_play_keysound returned: {res}");
     System.Threading.Thread.Sleep(100);
    }
    return 0;
   }
   if(args.Length >= 2 && args[0] == "test-wav") {
    string path = args[1];
    int soundType = args.Length >= 3 ? int.Parse(args[2]) : 0;
    Console.WriteLine($"Calling wav_player_play_simple({path}, {soundType})...");
    int res = wav_player_play_simple(path, soundType);
    Console.WriteLine($"wav_player_play_simple returned: {res}");
    return res;
   }
   if(args.Length >= 1 && args[0] == "test-sound") {
    Console.WriteLine("Initializing feedback...");
    int initRet = feedback_initialize();
    Console.WriteLine("feedback_initialize returned: " + initRet);
    for(int t = 0; t <= 1; t++) {
     for(int patIdx = 0; patIdx <= 15; patIdx++) {
      int supp = feedback_is_supported_pattern(t, patIdx);
      if(supp == 0 || supp == 1) {
       IntPtr ptr;
       int getRet = feedback_get_resource_path(t, patIdx, out ptr);
       string curPath = ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) : "(null)";
       Console.WriteLine($"TYPE {t} PAT {patIdx}: supported={supp}, curPath='{curPath}', getRet={getRet}");
      }
     }
    }
    feedback_deinitialize();
    return 0;
   }
   if(args.Length >= 2 && args[0] == "test-switch") {
    string src = args[1];
    IntPtr c;
    int rsw = app_control_create(out c);
    Console.WriteLine("app_control_create: " + rsw);
    app_control_set_app_id(c, "org.tizen.tv-viewer");
    app_control_set_operation(c, "http://tizen.org/appcontrol/operation/default");
    app_control_add_extra_data(c, "CALLER", "org.tizen.source-list");
    app_control_add_extra_data(c, "change_source", src);
    app_control_add_extra_data(c, "mbrIndex", "-2");
    app_control_add_extra_data(c, "sourceswitch_banner", "disable");
    rsw = app_control_send_launch_request(c, IntPtr.Zero, IntPtr.Zero);
    Console.WriteLine("app_control_send_launch_request: " + rsw);
    app_control_destroy(c);
    System.Threading.Thread.Sleep(2000);
    int currVal;
    vconf_get_int("memory/system/source_type", out currVal);
    Console.WriteLine("memory/system/source_type is now: " + currVal);
    return 0;
   }
   if(args.Length >= 2 && args[0] == "test-source") {
    int src = int.Parse(args[1]);
    int ret = connect_source(src);
    Console.WriteLine("connect_source(" + src + ") returned: " + ret);
    System.Threading.Thread.Sleep(2500);
    int currVal;
    vconf_get_int("memory/system/source_type", out currVal);
    Console.WriteLine("memory/system/source_type is now: " + currVal);
    return 0;
   }
   if(args.Length >= 2 && args[0] == "test-launch") {
    string target = args[1];
    Console.WriteLine("Testing launch of: " + target);

    // Test 1: app_control with operation
    IntPtr c1;
    app_control_create(out c1);
    app_control_set_app_id(c1, target);
    app_control_set_operation(c1, "http://tizen.org/appcontrol/operation/default");
    int r1 = app_control_send_launch_request(c1, IntPtr.Zero, IntPtr.Zero);
    app_control_destroy(c1);
    Console.WriteLine("app_control with default operation: " + r1);

    // Test 2: app_control without operation
    IntPtr c2;
    app_control_create(out c2);
    app_control_set_app_id(c2, target);
    int r2 = app_control_send_launch_request(c2, IntPtr.Zero, IntPtr.Zero);
    app_control_destroy(c2);
    Console.WriteLine("app_control without operation: " + r2);

    // Test 3: system app_launcher
    int r3 = system("app_launcher -u 5001 -s " + target);
    Console.WriteLine("system app_launcher: " + r3);

    return 0;
   }
   if(args.Length==2 && args[0]=="set-home") {int s=app_operation_set_current_app("http://tizen.org/appcontrol/operation/main",args[1]); Console.WriteLine("set_home="+s);return s==0?0:1;}
   if(args.Length==1 && args[0]=="state") {int v,m;int vr=sound_manager_get_master_volume(out v),mr=sound_manager_get_master_mute(out m);Console.WriteLine("volume="+v+" volume_result="+vr+" mute="+m+" mute_result="+mr);return vr==0&&mr==0?0:1;}
   IntPtr p;int r=app_operation_get_current_app("http://tizen.org/appcontrol/operation/main",out p);
   Console.WriteLine("home_result="+r+" home="+(p==IntPtr.Zero?"null":Marshal.PtrToStringAnsi(p)));if(p!=IntPtr.Zero)free(p);
   p=get_source_list();Console.WriteLine("sources="+(p==IntPtr.Zero?"null":Marshal.PtrToStringAnsi(p)));
   try {
    Console.WriteLine("Querying sourceapi_get_sourcelist...");
    sourceapi_get_sourcelist(OnSourceItem);
   } catch(Exception ex) {
    Console.WriteLine("sourceapi_get_sourcelist failed: " + ex);
   }
   int source; r=vconf_get_int("memory/system/source_type",out source);Console.WriteLine("current_source_result="+r+" current_source="+source);
   return 0;
  } catch(Exception e) {Console.WriteLine(e);return 1;}
 }
}

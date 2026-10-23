using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public sealed class LibraryApp
{
 public string Name {get;set;}
 public string Id {get;set;}
 public string Asset {get;set;}
 public string Description {get;set;}
 public string Fit {get;set;}="cover";
 public string Background {get;set;}="#FFFFFF";
 public int Width {get;set;}
 public int Height {get;set;}
 public float[] Crop {get;set;}
 public bool Utility {get;set;}
}

public static class AppLibrary
{
 public const int Columns=5, Rows=3, PageSize=Columns*Rows;
 public static List<LibraryApp> Load(string path)
 {
  var apps=JsonSerializer.Deserialize<List<LibraryApp>>(File.ReadAllText(path));
  if(apps==null||apps.Count==0)throw new InvalidDataException("App library is empty.");
  var ids=new HashSet<string>();
  foreach(var app in apps)
  {
   if(string.IsNullOrWhiteSpace(app.Name)||string.IsNullOrWhiteSpace(app.Id)||!ids.Add(app.Id)
      ||string.IsNullOrWhiteSpace(app.Asset)||Path.IsPathRooted(app.Asset)||app.Asset.Contains("..")
      ||app.Width<=0||app.Height<=0)
   throw new InvalidDataException("Invalid app library entry.");
   var crop=app.Crop;
   if(crop!=null&&(crop.Length!=4||!Array.TrueForAll(crop,float.IsFinite)
      ||crop[0]<0||crop[1]<0||crop[2]<=0||crop[3]<=0
      ||crop[0]+crop[2]>1||crop[1]+crop[3]>1))
    throw new InvalidDataException("Invalid artwork crop.");
  }
  return apps;
 }
 public static void Save(string path, List<LibraryApp> apps)
 {
  var options = new JsonSerializerOptions { WriteIndented = true };
  string json = JsonSerializer.Serialize(apps, options);
  File.WriteAllText(path, json);
 }
 public static int PageCount(int count)=>(count+PageSize-1)/PageSize;
 public static int VisibleCount(int count,int page)=>Math.Max(0,Math.Min(PageSize,count-page*PageSize));
 // Return an absolute catalog index. -1 moves focus to the navigation tabs.
 public static int Move(int count,int absolute,string key)
 {
  if(count<=0)return -1;
  absolute=Math.Clamp(absolute,0,count-1);
  int page=absolute/PageSize,local=absolute%PageSize,col=local%Columns,row=local/Columns;
  int pageStart=page*PageSize,visible=VisibleCount(count,page);
  if(key=="Left")return Math.Max(0,absolute-1);
  if(key=="Right")return Math.Min(count-1,absolute+1);
  if(key=="Up")
  {
   if(row>0)return absolute-Columns;
   if(page==0)return -1;
   return pageStart-Columns+col;
  }
  if(key=="Down")
  {
   if((row+1)*Columns<visible)return pageStart+Math.Min(local+Columns,visible-1);
   int next=pageStart+PageSize;
   return next<count?Math.Min(next+col,count-1):absolute;
  }
  return absolute;
 }
}

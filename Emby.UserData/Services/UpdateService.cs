using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Emby.UserData.Services {
 public sealed class UpdateService {
  const string LatestReleaseUrl="https://api.github.com/repos/ratopro/emby-user-data/releases/latest";
  public async Task CheckAndUpdateAsync(bool force = false) {
   var config=Plugin.Instance.Configuration;
   force=force||config.ManualUpdateRequested;
   if(config.ManualUpdateRequested) { config.ManualUpdateRequested=false; Plugin.Instance.SaveConfiguration(); }
   if(!force&&!config.AutoUpdateEnabled) return;
   try {
    using(var client=new HttpClient()) {
     client.DefaultRequestHeaders.UserAgent.ParseAdd("Emby-UserData-Updater");
     var json=await client.GetStringAsync(LatestReleaseUrl).ConfigureAwait(false);
     var tag=Match(json,"\\\"tag_name\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
     var url=Match(json,"\\\"browser_download_url\\\"\\s*:\\s*\\\"([^\\\"]*Emby\\.UserData-[^\\\"]*\\.zip)\\\"");
     Version latest; if(!Version.TryParse((tag??"").TrimStart('v','V'),out latest)) throw new InvalidDataException("The latest GitHub release has an invalid version.");
     if(latest<=CurrentVersion()) { config.LastUpdateCheckUtc=DateTime.UtcNow; config.LastUpdateStatus="UpToDate"; config.LastUpdateError=null; Plugin.Instance.SaveConfiguration(); return; }
     if(string.IsNullOrWhiteSpace(url)) throw new InvalidDataException("The latest GitHub release has no plugin ZIP asset.");
     var archive=await client.GetByteArrayAsync(url).ConfigureAwait(false);
     var updateDir=Path.Combine(Plugin.Instance.DataFolderPath,"automatic-updates"); Directory.CreateDirectory(updateDir);
     var zipPath=Path.Combine(updateDir,"download-"+latest+".zip"); File.WriteAllBytes(zipPath,archive);
     var dllPath=ExtractDll(zipPath,updateDir);
     new UserDataStore().CreateInternalBackup("before-update-"+latest);
     var assemblyPath=typeof(Plugin).GetTypeInfo().Assembly.Location;
     var dllBackup=Path.Combine(updateDir,"Emby.UserData-"+CurrentVersion()+".dll"); File.Copy(assemblyPath,dllBackup,true);
     var temporary=assemblyPath+".update"; File.Copy(dllPath,temporary,true);
     if(File.Exists(assemblyPath)) File.Replace(temporary,assemblyPath,null); else File.Move(temporary,assemblyPath);
     config.PendingRestartForUpdate=true; config.LastUpdateVersion=latest.ToString(); config.LastUpdateCheckUtc=DateTime.UtcNow; config.LastUpdateStatus="RestartRequired"; config.LastUpdateError=null; Plugin.Instance.SaveConfiguration();
   }} catch(Exception ex) { config.LastUpdateCheckUtc=DateTime.UtcNow; config.LastUpdateStatus="Error"; config.LastUpdateError=ex.Message; Plugin.Instance.SaveConfiguration(); }
  }
  static Version CurrentVersion() { return typeof(Plugin).GetTypeInfo().Assembly.GetName().Version; }
  static string Match(string value,string pattern) { var match=Regex.Match(value,pattern,RegexOptions.IgnoreCase); return match.Success?match.Groups[1].Value.Replace("\\/","/"):null; }
  static string ExtractDll(string zipPath,string folder) { using(var zip=ZipFile.OpenRead(zipPath)) { foreach(var entry in zip.Entries) if(string.Equals(Path.GetFileName(entry.FullName),"Emby.UserData.dll",StringComparison.OrdinalIgnoreCase)) { var path=Path.Combine(folder,"Emby.UserData.next.dll"); entry.ExtractToFile(path,true); return path; } } throw new InvalidDataException("The release does not contain Emby.UserData.dll."); }
 }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;
using Emby.UserData.Models;
using Emby.UserData.Services;

namespace Emby.UserData.Api {
 [Route("/UserData/Fields", "GET")] public class GetFields : IReturn<List<FieldDefinition>> { }
 [Route("/UserData/Fields", "POST")] public class SaveField : IReturn<FieldDefinition> { public FieldDefinition Field { get; set; } }
 [Route("/UserData/Fields/{Id}", "DELETE")] public class DeleteField { public string Id { get; set; } public bool Permanent { get; set; } }
 [Route("/UserData/Fields/Order", "POST")] public class SetFieldOrder { public List<string> Ids { get; set; } }
 [Route("/UserData/Users/{UserId}", "GET")] public class GetUserValues : IReturn<UserRecord> { public string UserId { get; set; } }
 [Route("/UserData/Users/{UserId}", "POST")] public class SaveUserValues { public string UserId { get; set; } public Dictionary<string,string> Values { get; set; } }
 [Route("/UserData/Backup", "GET")] public class GetBackup : IReturn<BackupDocument> { public string Scope { get; set; } }
 [Route("/UserData/Backup/Import", "POST")] public class ImportBackup { public BackupDocument Backup { get; set; } public string Mode { get; set; } }
 public class UserDataService : IService, IRequiresRequest {
  private readonly UserDataStore store = new UserDataStore();
  private readonly ISessionContext sessionContext;
  public IRequest Request { get; set; }
  public UserDataService(ISessionContext sessionContext) { this.sessionContext = sessionContext; }
  private void RequireAdministrator() { var user = sessionContext.GetUser(Request); if (user == null || !user.Policy.IsAdministrator) throw new MediaBrowser.Controller.Net.SecurityException("Administrator access is required."); }
  public object Get(GetFields request) { RequireAdministrator(); return store.Read().Fields.OrderBy(x=>x.Order).ToList(); }
  public object Post(SaveField request) { RequireAdministrator(); if(request.Field==null||string.IsNullOrWhiteSpace(request.Field.Name)) throw new ArgumentException("A field name is required"); request.Field.Name=request.Field.Name.Trim(); return store.Upsert(request.Field); }
  public object Delete(DeleteField request) { RequireAdministrator(); if(request.Permanent) store.Delete(request.Id); else store.Deactivate(request.Id); return null; }
  public object Post(SetFieldOrder request) { RequireAdministrator(); var doc=store.Read(); for(var i=0;i<(request.Ids??new List<string>()).Count;i++) { var field=doc.Fields.FirstOrDefault(x=>x.Id==request.Ids[i]); if(field!=null) field.Order=i; } store.Save(doc); return null; }
  public object Get(GetUserValues request) { RequireAdministrator(); return store.GetUser(request.UserId); }
  public object Post(SaveUserValues request) { RequireAdministrator(); if(string.IsNullOrWhiteSpace(request.UserId)) throw new ArgumentException("User id is required"); store.SaveUser(request.UserId,request.Values??new Dictionary<string,string>()); return null; }
  public object Get(GetBackup request) { RequireAdministrator(); var scope=request.Scope=="structure" ? "structure" : "full"; return store.Export(scope); }
  public object Post(ImportBackup request) { RequireAdministrator(); store.Import(request.Backup, request.Mode); return null; }
 }
}

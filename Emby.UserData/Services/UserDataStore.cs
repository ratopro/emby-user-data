using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Emby.UserData.Models;

namespace Emby.UserData.Services {
 public sealed class UserDataStore {
  private static readonly object Sync = new object(); private readonly string file;
  public UserDataStore() : this(Plugin.Instance.DataFolderPath) { }
  public UserDataStore(string dataFolderPath) { var folder=Path.Combine(dataFolderPath, "user-data"); Directory.CreateDirectory(folder); file=Path.Combine(folder,"store.json"); }
  public StoreDocument Read() { lock(Sync) { if(!File.Exists(file)) return new StoreDocument(); try { return JsonConvert.DeserializeObject<StoreDocument>(File.ReadAllText(file)) ?? new StoreDocument(); } catch { var corrupt=file+".corrupt-"+DateTime.UtcNow.Ticks; File.Move(file,corrupt); return new StoreDocument(); } } }
  public void Save(StoreDocument data) { lock(Sync) { var tmp=file+".tmp"; File.WriteAllText(tmp,JsonConvert.SerializeObject(data,Formatting.Indented)); if(File.Exists(file)) File.Replace(tmp,file,null); else File.Move(tmp,file); } }
  public FieldDefinition Upsert(FieldDefinition field) { var d=Read(); var existing=d.Fields.FirstOrDefault(x=>x.Id==field.Id); if(existing==null) { field.Id=string.IsNullOrWhiteSpace(field.Id)?Guid.NewGuid().ToString("N"):field.Id; field.Order=d.Fields.Count; d.Fields.Add(field); } else { existing.Name=field.Name; existing.Type=field.Type; existing.Required=field.Required; existing.IsActive=field.IsActive; existing.Options=field.Options??existing.Options; } Save(d); return field; }
  public void Deactivate(string id) { var d=Read(); var f=d.Fields.FirstOrDefault(x=>x.Id==id); if(f==null) throw new ArgumentException("Field not found"); f.IsActive=false; Save(d); }
  public void Delete(string id) { var d=Read(); d.Fields.RemoveAll(x=>x.Id==id); foreach(var r in d.Users.Values) r.Values.Remove(id); Save(d); }
  public UserRecord GetUser(string id) { var d=Read(); return d.Users.ContainsKey(id)?d.Users[id]:new UserRecord{UserId=id}; }
  public void SaveUser(string id, System.Collections.Generic.Dictionary<string,string> values) { var d=Read(); var active=d.Fields.Where(x=>x.IsActive).ToDictionary(x=>x.Id); foreach(var p in values) { if(!active.ContainsKey(p.Key)) throw new ArgumentException("Unknown or inactive field"); Validate(active[p.Key],p.Value); } foreach(var field in active.Values.Where(x=>x.Required)) if(!values.ContainsKey(field.Id)||string.IsNullOrWhiteSpace(values[field.Id])) throw new ArgumentException(field.Name+" is required"); var existing=d.Users.ContainsKey(id) ? d.Users[id] : new UserRecord { UserId=id }; foreach(var field in active.Keys) { if(values.ContainsKey(field)) existing.Values[field]=values[field]; else existing.Values.Remove(field); } d.Users[id]=existing; Save(d); }
  private static void Validate(FieldDefinition f,string v) { if(string.IsNullOrEmpty(v)) return; decimal n; DateTime dt; if(f.Type==FieldType.Number&&!decimal.TryParse(v,System.Globalization.NumberStyles.Number,System.Globalization.CultureInfo.InvariantCulture,out n)) throw new ArgumentException(f.Name+" must be a number"); if(f.Type==FieldType.Date&&!DateTime.TryParse(v,out dt)) throw new ArgumentException(f.Name+" must be a date"); if(f.Type==FieldType.Boolean&&v!="true"&&v!="false") throw new ArgumentException(f.Name+" must be true or false"); if(f.Type==FieldType.Select&&!f.Options.Contains(v)) throw new ArgumentException(f.Name+" has an invalid selection"); }
 }
}

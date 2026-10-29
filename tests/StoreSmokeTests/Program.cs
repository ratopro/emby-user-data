using System;
using System.Collections.Generic;
using System.IO;
using Emby.UserData.Models;
using Emby.UserData.Services;

static class Program
{
    static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    static int Main()
    {
        var root = Path.Combine(Path.GetTempPath(), "emby-user-data-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new UserDataStore(root);
            var text = store.Upsert(new FieldDefinition { Name = "Contrato", Type = FieldType.Text, Required = true });
            var select = store.Upsert(new FieldDefinition { Name = "Plan", Type = FieldType.Select, Options = new List<string> { "Básico", "Premium" } });
            var file = Path.Combine(root, "user-data", "store.json");
            Assert(File.Exists(file), "No se creó store.json.");
            Assert(store.Read().Fields.Count == 2, "Los campos no se persistieron.");

            store.SaveUser("user-1", new Dictionary<string, string> { [text.Id] = "C-001", [select.Id] = "Premium" });
            Assert(store.GetUser("user-1").Values[select.Id] == "Premium", "El valor del usuario no se persistió.");

            store.Deactivate(select.Id);
            store.SaveUser("user-1", new Dictionary<string, string> { [text.Id] = "C-002" });
            Assert(store.GetUser("user-1").Values[select.Id] == "Premium", "Un campo desactivado perdió su valor.");

            var structureBackup = store.Export("structure");
            Assert(structureBackup.Users == null && structureBackup.Fields.Count == 2, "La exportación de estructura es incorrecta.");
            var fullBackup = store.Export("full");
            Assert(fullBackup.Users.ContainsKey("user-1"), "La exportación completa no contiene usuarios.");
            var imported = new UserDataStore(Path.Combine(root, "import"));
            imported.Import(fullBackup, "replace");
            Assert(imported.GetUser("user-1").Values[text.Id] == "C-002", "La importación completa no restauró valores.");

            store.Delete(select.Id);
            Assert(!store.Read().Fields.Exists(f => f.Id == select.Id), "El campo no se eliminó.");
            Assert(!store.GetUser("user-1").Values.ContainsKey(select.Id), "El borrado no eliminó el valor asociado.");

            File.WriteAllText(file, "{ json inválido");
            Assert(store.Read().Fields.Count == 0, "La recuperación ante JSON dañado falló.");
            Assert(Directory.GetFiles(Path.GetDirectoryName(file), "store.json.corrupt-*").Length == 1, "No se conservó una copia del JSON dañado.");
            Console.WriteLine("OK: persistencia de campos y valores verificada.");
            return 0;
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}

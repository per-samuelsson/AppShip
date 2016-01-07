using Starcounter;
using Starcounter.Internal;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.IO.Compression;

namespace AppShip {

    partial class AppShip : AppShipPartial {

        public AppShip InitDefault() {
            var us = Starcounter.Application.Current;
            var apps = Db.Applications;

            if (apps.Length == 1) {
                // Only we are running. Suggest us as the example.
                Application = us.Name;
                RunScriptIt(us.FilePath, us.Name);
            }
            else if (apps.Length == 2) {
                // Its us and another one. Lets suggest that other one.
                var other = apps.First(a => !a.Name.Equals(us.Name));
                Application = other.Name;
                RunScriptIt(other.FilePath, other.Name);
            }
            else {
                var suggest = "[";
                foreach (var app in apps) {
                    if (app.Name.Equals(us.Name)) continue;
                    suggest += app.Name + " | ";
                }
                suggest += @"full\path\to\application.exe]";
                Application = suggest;
            }

            return this;
        }

        bool GetPathAndName(out string name, out string path) {
            var appSpec = Application;
            name = path = null;

            if (string.IsNullOrEmpty(appSpec)) {
                ResultMessage = "No application given";
                return false;
            }

            var app = Db.Applications.FirstOrDefault(a => a.Name.Equals(appSpec, StringComparison.InvariantCultureIgnoreCase));
            if (app != null) {
                path = app.FilePath;
                name = app.Name;
            } else if (File.Exists(appSpec)) {
                path = appSpec;
                name = Path.GetFileName(path);
            } else {
                ResultMessage = "Couldn't find application: " + appSpec;
                return false;
            }

            return true;
        }

        void Handle(Input.ZipIt zipit) {
            string path, name;
            if (GetPathAndName(out name, out path)) {
                RunZipIt(path, name);
            }
        }

        void Handle(Input.ScriptIt scriptit) {
            string path, name;
            if (GetPathAndName(out name, out path)) {
                RunScriptIt(path, name);
            }
        }

        void RunZipIt(string path, string name) {
            ResultMessage = string.Format("{0} ({1})", name, path);
            Result.OutDirectory = string.Format("AppShip-{0}-{1}", Db.Environment.DatabaseNameLower, name);

            var zip = Path.Combine(Path.GetDirectoryName(path), Result.OutDirectory + ".zip");
            try {
                ZipFile.CreateFromDirectory(Path.GetDirectoryName(path), zip);
            }
            catch (Exception e) {
                ResultMessage = e.Message;
            }
        }

        void RunScriptIt(string path, string name) {
            ResultMessage = string.Format("{0} ({1})", name, path);

            var props = DatabaseProperties.CreateFromServerRequest();

            Result.OutDirectory = string.Format("AppShip-{0}-{1}", Db.Environment.DatabaseNameLower, name);
            Result.InstalledVersion = CurrentVersion.Version;
            Result.Database = Db.Environment.DatabaseNameLower;

            var dir = Path.GetDirectoryName(path);
            foreach (var item in Directory.GetFiles(dir)) {
                Result.ApplicationFiles.Add().Path = item;
            }

            foreach (var item in Directory.GetFiles(props.ImageDirectory)) {
                Result.ImageFiles.Add().Path = item;
            }

            // Only populate unique transaction log directory if image- and
            // transaction log directories differ

            if (!props.ImageDirectory.Equals(props.TransactionLogDirectory, StringComparison.InvariantCultureIgnoreCase)) {
                foreach (var item in Directory.GetFiles(props.TransactionLogDirectory)) {
                    Result.TransactionLogFiles.Add().Path = item;
                }
            }
        }
    }
}

using Starcounter;
using Starcounter.Internal;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AppShip {

    partial class AppShip : Partial {

        public AppShip InitDefault() {
            var us = Starcounter.Application.Current;
            var apps = Db.Applications;

            if (apps.Length == 1) {
                // Only we are running. Suggest us as the example.
                Application = us.Name;
                BuildResult(us.FilePath, us.Name);
            }
            else if (apps.Length == 2) {
                // Its us and another one. Lets suggest that other one.
                var other = apps.First(a => !a.Name.Equals(us.Name));
                Application = other.Name;
                BuildResult(other.FilePath, other.Name);
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

        void Handle(Input.ShipIt shipit) {
            var appSpec = Application;
            string path, name = null;

            Result = new ResultJson();
            
            if (string.IsNullOrEmpty(appSpec)) {
                ResultMessage = "No application given";
                return;
            }
            
            var app = Db.Applications.FirstOrDefault(a => a.Name.Equals(appSpec, StringComparison.InvariantCultureIgnoreCase));
            if (app != null) {
                path = app.FilePath;
                name = app.Name;
            }
            else if (File.Exists(appSpec)) {
                path = appSpec;
                name = Path.GetFileName(path);
            }
            else {
                ResultMessage = "Couldn't find application: " + appSpec;
                return;
            }

            BuildResult(path, name);
        }

        void BuildResult(string path, string name) {
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

using Starcounter;
using System;
using System.IO;
using System.Linq;

namespace AppShip {

    partial class AppShip : Partial {

        void Handle(Input.ShipIt shipit) {
            var appSpec = Application;

            if (string.IsNullOrEmpty(appSpec)) {
                ApplicationFiles.Clear();
                return;
            }

            var app = Db.Applications.FirstOrDefault(a => a.Name.Equals(appSpec, StringComparison.InvariantCultureIgnoreCase));
            if (app == null) {
                ApplicationFiles.Clear();
                return;
            }
            
            ApplicationFiles.Clear();

            OutDirectory = string.Format("AppShip-{0}-{1}", Db.Environment.DatabaseNameLower, app.Name);
            var dir = Path.GetDirectoryName(app.FilePath);
            foreach (var item in Directory.GetFiles(dir)) {
                ApplicationFiles.Add().Path = item;
            }
        }
    }
}

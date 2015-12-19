using System;
using Starcounter;

namespace AppShip {

    class Program {
        static void Main() {
            Handle.GET("/appship", () => {
                var a = new AppShip() {
                    Html = "/appship.html",
                };

                a.OutDirectory = string.Format("AppShip-{0}-{1}", Db.Environment.DatabaseNameLower, "Testing");
                a.ApplicationFiles.Add().Path = @"C:\path\to\app.exe";

                return a;
            });
        }
    }
}
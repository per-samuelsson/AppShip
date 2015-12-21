using System;
using Starcounter;
using Starcounter.Internal;

namespace AppShip {

    class Program {

        static void Main() {

            Handle.GET("/appship", () => {
                var a = new AppShip() {
                    Html = "/appship.html",
                };

                return a.InitDefault();
            });
        }
    }
}
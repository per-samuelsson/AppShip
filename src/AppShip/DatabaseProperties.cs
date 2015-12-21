
using Starcounter;
using Starcounter.Internal;

namespace AppShip {

    public interface IDatabaseProperties {
        string ImageDirectory { get; }
        string TransactionLogDirectory { get; }
        string TempDirectory { get; }
    }

    public class DatabaseProperties : IDatabaseProperties {
        string imageDir;
        string tlogDir;
        string tempDir;

        string IDatabaseProperties.ImageDirectory {
            get {
                return imageDir;
            }
        }

        string IDatabaseProperties.TempDirectory {
            get {
                return tempDir;
            }
        }

        string IDatabaseProperties.TransactionLogDirectory {
            get {
                return tlogDir;
            }
        }

        public static IDatabaseProperties CreateFromServerRequest() {
            var serverPort = StarcounterEnvironment.Default.SystemHttpPort;
            var x = Http.GET(string.Format("http://localhost:{0}/api/admin/databases/{1}/settings", serverPort, Db.Environment.DatabaseNameLower));
            var j = new Json(x.ToString());

            var props = new DatabaseProperties();
            props.imageDir = j["ImageDirectory"] as string;
            props.tlogDir = j["TransactionLogDirectory"] as string;
            props.tempDir = j["TempDirectory"] as string;

            return props;
        }
    }
}

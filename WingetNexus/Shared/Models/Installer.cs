using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WingetNexus.Shared.Models
{
    public class Installer
    {

        public Installer()
        { }

        public Installer(string architecture, string installerType, string filename, string checksum, string scope)
        {
            Architecture = architecture;
            InstallerType = installerType;
            FileName = filename;
            InstallerSha256 = checksum;
            Scope = scope;
            Switches = new List<InstallerSwitch>();
            NestedInstallerType = "";
            NestedInstallerFiles = new List<NestedInstallerFile>();
        }

        public int Id { get; set; }
        public int VersionId { get; set; }
        public string Architecture { get; set; }
        public string InstallerType { get; set; }
        public string FileName { get; set; }
        public string InstallerSha256 { get; set; }
        public string Scope { get; set; }
        public virtual ICollection<InstallerSwitch> Switches { get; set; }
        public string? NestedInstallerType { get; set; }
        public virtual ICollection<NestedInstallerFile>? NestedInstallerFiles { get; set; }

        [JsonIgnore]
        public virtual PackageVersion Version { get; set; }
        [ForeignKey("Version")]
        public int PackageVersionId { get; set; }

        public JObject ToJson()
        {
            var switches = new JArray(Switches.Select(s => s.ToJson()));
            return new JObject(
                new JProperty("id", Id),
                new JProperty("version_id", VersionId),
                new JProperty("architecture", Architecture),
                new JProperty("installer_type", InstallerType),
                new JProperty("file_name", FileName),
                new JProperty("installer_sha256", InstallerSha256),
                new JProperty("scope", Scope),
                new JProperty("switches", switches)
            );
        }
    }
}

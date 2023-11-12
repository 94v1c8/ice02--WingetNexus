using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WingetNexus.Shared.Models
{
    public class PackageVersion
    {
        public PackageVersion()
        {

        }

        //contructor for package version with identifier, version code, default locale, package locale, and short description
        //version, "en-US", name, identifier
        public PackageVersion(string versionCode,
            string defaultLocale, string identifier)
        {
            Identifier = identifier;
            VersionCode = versionCode;
            DefaultLocale = defaultLocale;
            PackageLocale = defaultLocale;
            ShortDescription = "";

        }

        public int Id { get; set; }
        public string Identifier { get; set; }
        public string VersionCode { get; set; }
        public string DefaultLocale { get; set; }
        public string PackageLocale { get; set; }
        public string ShortDescription { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public virtual ICollection<Installer> Installers { get; set; }
        [JsonIgnore]
        public virtual Package Package { get; set; }
        [ForeignKey("Package")]
        public int PackageId { get; set; }
    }
}

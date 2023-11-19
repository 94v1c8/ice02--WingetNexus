using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetNexus.Shared.Models.Dtos
{
    public class VersionDto
    {
        public string? VersionCode { get; set; }
        public string? PackageIdentifier { get; set; }
        public string? ShortDescription { get; set; }

        public static VersionDto FromDb(PackageVersion version)
        {
            return new VersionDto()
            {
                VersionCode = version.VersionCode,
                //PackageIdentifier = version.Package.Identifier,
                ShortDescription = version.ShortDescription
            };
        }
    }
}

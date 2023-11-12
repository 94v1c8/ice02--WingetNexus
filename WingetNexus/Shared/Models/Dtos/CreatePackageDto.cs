using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetNexus.Shared.Models.Dtos
{
    public class CreatePackageDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Publisher { get; set; }
        [Required]
        public string Identifier { get; set; }
        [Required]
        public string Architecture { get; set; }
        [Required]
        public string Version { get; set; }
        [Required]
        public string InstallerType { get; set; }
        [Required]
        public string Filename { get; set; }
        [Required]
        public string ShortDescription { get; set; }
        [Required]
        public string Checksum { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public SwitchesDto Switches { get; set; } = new SwitchesDto();
        public string? RelativeFilePath { get; set; }

        public CreatePackageDto(string name, string publisher, string identifier, string architecture, string version, string installerType)
        {
            Name = name;
            Publisher = publisher;
            Identifier = identifier;
            Architecture = architecture;
            Version = version;
            InstallerType = installerType;
        }

        public CreatePackageDto()
        {

        }
    }
}

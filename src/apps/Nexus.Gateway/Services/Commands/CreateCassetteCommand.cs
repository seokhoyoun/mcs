using System.ComponentModel.DataAnnotations;

namespace Nexus.Gateway.Services.Commands
{
    public class CreateCassetteCommand
    {
        [Required]
        public string CassetteId { get; set; } = string.Empty;

        [Required]
        public string CassetteName { get; set; } = string.Empty;

        [Required]
        public string LocationId { get; set; } = string.Empty;
    }
}
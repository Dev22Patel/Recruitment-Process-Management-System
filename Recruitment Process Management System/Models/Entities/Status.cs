using System.ComponentModel.DataAnnotations;

namespace Recruitment_Process_Management_System.Models.Entities
{
    public class Status
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; }

        [Required]
        [MaxLength(50)]
        public string StatusName { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

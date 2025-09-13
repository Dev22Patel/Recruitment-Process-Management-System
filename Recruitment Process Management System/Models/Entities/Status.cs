namespace Recruitment_Process_Management_System.Models.Entities
{
    public class Status
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public string StatusName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

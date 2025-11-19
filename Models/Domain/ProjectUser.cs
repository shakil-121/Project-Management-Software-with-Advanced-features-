namespace FastPMS.Models.Domain
{
    public class ProjectUser
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserRole { get; set; } = "Client";
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public string AssignedBy { get; set; }

        public virtual Project Project { get; set; }
        public virtual Users User { get; set; }
    }
}
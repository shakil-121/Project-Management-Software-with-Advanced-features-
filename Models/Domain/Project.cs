namespace FastPMS.Models.Domain
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string ProjectDescription { get; set; } 
        public string Stack {  get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public string Status {  get; set; }

    }
}

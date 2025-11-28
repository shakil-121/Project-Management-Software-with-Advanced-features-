using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastPMS.Models.Domain
{
    public class ProjectTask
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public int ProjectId { get; set; } 

        [Required]
        public string AssignedToId { get; set; }

        public string AssignedById { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Not Started";

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = "Medium";

        public DateTime DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public decimal EstimatedHours { get; set; }

        public decimal ActualHours { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual Users AssignedTo { get; set; }

        [ForeignKey("AssignedById")]
        public virtual Users AssignedBy { get; set; }
    }
}
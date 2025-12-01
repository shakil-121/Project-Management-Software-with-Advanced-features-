using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastPMS.Models.Domain
{
    public class SubTask
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string ProjectTaskId { get; set; } = string.Empty;

        [Required]
        public string AssignedToId { get; set; } = string.Empty;

        public string? AssignedById { get; set; }

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

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("ProjectTaskId")]
        public virtual ProjectTask? ProjectTask { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual Users? AssignedTo { get; set; }

        [ForeignKey("AssignedById")]
        public virtual Users? AssignedBy { get; set; }
    }
}
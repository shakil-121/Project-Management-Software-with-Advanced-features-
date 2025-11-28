using System;
using System.ComponentModel.DataAnnotations;

namespace FastPMS.Models.ViewModel
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select a project")]
        public int ProjectId { get; set; }  

        [Required(ErrorMessage = "Please assign to a team member")]
        public string AssignedToId { get; set; }

        [Required(ErrorMessage = "Please select priority")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);

        [Range(0.5, 500, ErrorMessage = "Estimated hours must be between 0.5 and 500")]
        public decimal EstimatedHours { get; set; } = 8;
    }
}
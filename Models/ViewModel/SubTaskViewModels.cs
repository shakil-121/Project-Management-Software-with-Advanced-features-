using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FastPMS.Models.ViewModel
{
    public class SubTaskCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a parent task")]
        [Display(Name = "Parent Task")]
        public string ProjectTaskId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please assign to a team member")]
        [Display(Name = "Assign To")]
        public string AssignedToId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Due date is required")]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);

        [Display(Name = "Priority Level")]
        public string Priority { get; set; } = "Medium";

        [Display(Name = "Estimated Hours")]
        [Range(0.1, 1000, ErrorMessage = "Estimated hours must be between 0.1 and 1000")]
        public decimal EstimatedHours { get; set; } = 1;

        [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
        public string Notes { get; set; } = string.Empty;

        // ✅ REMOVED: AvailableTasks and AvailableTeamMembers properties
        // These should ONLY be in ViewBag, not in ViewModel
    }

    public class SubTaskEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a parent task")]
        [Display(Name = "Parent Task")]
        public string ProjectTaskId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please assign to a team member")]
        [Display(Name = "Assign To")]
        public string AssignedToId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Due date is required")]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Priority Level")]
        public string Priority { get; set; } = "Medium";

        [Display(Name = "Estimated Hours")]
        [Range(0.1, 1000, ErrorMessage = "Estimated hours must be between 0.1 and 1000")]
        public decimal EstimatedHours { get; set; }

        [Display(Name = "Actual Hours")]
        [Range(0, 1000, ErrorMessage = "Actual hours must be between 0 and 1000")]
        public decimal ActualHours { get; set; }

        [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Not Started";

        // ✅ REMOVED: AvailableTasks and AvailableTeamMembers properties
        // These should ONLY be in ViewBag, not in ViewModel
    }
}
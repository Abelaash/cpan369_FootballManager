using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FootballManager.Models
{
    [Table("Injurytbl")]
    public class Injury
    {
        [Key]
        public int InjuryId { get; set; }

        [Required(ErrorMessage = "Player is required")]
        [ForeignKey("Player")]
        public int PlayerId { get; set; }
        public Player Player { get; set; } // object of foreign key model Player

        [Required(ErrorMessage = "Team is required")]
        [ForeignKey("Team")]
        public int TeamId { get; set; }
        public Team Team { get; set; } // object of foreign key model Team

        [Required(ErrorMessage = "Injury type is required")]
        [StringLength(50, ErrorMessage = "Injury type cannot exceed 50 characters")]
        public string InjuryType { get; set; }

        [Required(ErrorMessage = "Severity is required")]
        public string Severity { get; set; }

        [Required(ErrorMessage = "Date Injured is required")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/2000", "12/31/2099", ErrorMessage = "Date must be between {1} and {2}")]
        public DateTime DateInjured { get; set; }

        [Required(ErrorMessage = "Expected Recovery Date is required")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Injury), "ValidateRecoveryDate")]
        public DateTime ExpectedRecoveryDate { get; set; }

        [Required(ErrorMessage = "Notes are required")]
        [StringLength(100, ErrorMessage = "Notes cannot exceed 100 characters")]
        public string Notes { get; set; }



        public static ValidationResult ValidateRecoveryDate(DateTime expectedRecoveryDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as Injury;
            if (instance != null && expectedRecoveryDate < instance.DateInjured)
            {
                return new ValidationResult("Expected Recovery Date must be after Date Injured");
            }
            return ValidationResult.Success;
        }
    }
}

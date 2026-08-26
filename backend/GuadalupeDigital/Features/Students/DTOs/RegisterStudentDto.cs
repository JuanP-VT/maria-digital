namespace GuadalupeDigital.Features.Students.DTOs
{
    public class CreateStudentRegistrationDto
    {
        // Form & Enrollment Metadata
        public string Level { get; set; } = string.Empty;

        // Student Basic Info
        public string Name { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }

        // Sacramental History
        public string? DateAndPlaceOfBaptism { get; set; }
        public string? DateAndPlaceOfConfirmation { get; set; }
        public string? DateAndPlaceOfFirstCommunion { get; set; }

        // Parents & Home
        public string? FathersName { get; set; }
        public string? MothersName { get; set; }
        public bool AreParentsMarriedByChurch { get; set; }
        public bool DoParentsLiveTogether { get; set; }
        public string? Address { get; set; }
        public string? Cellphone { get; set; }
        public string? Email { get; set; }
        public string? GodparentName { get; set; }

        // Schooling & Activities
        public string? SchoolName { get; set; }
        public string? SchoolGrade { get; set; }
        public string? ExtracurricularActivities { get; set; }

        // Health Information
        public string? IllnessesOrAllergies { get; set; }
        public string? MedicationTaken { get; set; }
        public MedicalHealthInstitution? HealthInstitution { get; set; }

        // Logistics & Ministry
        public string? AuthorizedPickupPerson { get; set; }
        public bool WishToServeInChoir { get; set; }
        public bool WishToServeAsAcolyte { get; set; }

        // Administration
        public decimal? FinancialContribution { get; set; }
        public string? CatechistName { get; set; }
    }

    public enum MedicalHealthInstitution
    {
        None = 0,
        IMSS = 1,
        ISSSTE = 2,
        Other = 3
    }
}

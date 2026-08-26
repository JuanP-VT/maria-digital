namespace GuadalupeDigital.Features.Students.DTOs
{
    public class Student
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? DateAndPlaceOfBaptism { get; set; }
        public string? DateAndPlaceOfConfirmation { get; set; }
        public string? DateAndPlaceOfFirstCommunion { get; set; }
        public string? FathersName { get; set; }
        public string? MothersName { get; set; }
        public bool? AreParentsMarriedByChurch { get; set; }
        public bool? DoParentsLiveTogether { get; set; }
        public string? Address { get; set; }
        public string? Cellphone { get; set; }
        public string? Email { get; set; }
        public string? GodparentName { get; set; }
        public string? SchoolName { get; set; }
        public string? SchoolGrade { get; set; }
        public string? ExtracurricularActivities { get; set; }
        public string? IllnessesOrAllergies { get; set; }
        public string? MedicationTaken { get; set; }
        public MedicalHealthInstitution? HealthInstitution { get; set; }
        public string? AuthorizedPickupPerson { get; set; }
        public bool? WishToServeInChoir { get; set; }
        public bool? WishToServeAsAcolyte { get; set; }
        public decimal? FinancialContribution { get; set; }
        public string? CatechistName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

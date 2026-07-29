using System.ComponentModel.DataAnnotations;

namespace BookIt.Models.DTOs.Request;

public class AppointmentRequest : IValidatableObject
{
    public Guid ServiceId { get; set; }
    public DateTimeOffset StartTime { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime <= DateTimeOffset.UtcNow)
        {
            yield return new ValidationResult("StartTime must be in the future.", new[] { nameof(StartTime) });
        }
    }
}

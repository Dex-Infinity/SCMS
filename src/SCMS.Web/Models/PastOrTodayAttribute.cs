using System.ComponentModel.DataAnnotations;

namespace SCMS.Web.Models;

/// <summary>Validates that an optional <see cref="DateOnly"/> is not in the future.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PastOrTodayAttribute : ValidationAttribute
{
    public PastOrTodayAttribute() => ErrorMessage = "The incident date can't be in the future.";

    public override bool IsValid(object? value) =>
        value is not DateOnly date || date <= DateOnly.FromDateTime(DateTime.Today);
}

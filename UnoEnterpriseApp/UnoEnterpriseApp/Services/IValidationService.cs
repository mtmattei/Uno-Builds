using System.ComponentModel.DataAnnotations;

namespace UnoEnterpriseApp.Services;

public interface IValidationService
{
    ValidationResult ValidateObject(object instance);
    IEnumerable<ValidationResult> ValidateProperty(object instance, string propertyName, object? value);
    bool IsValid(object instance);
}

public class ValidationService : IValidationService
{
    public ValidationResult ValidateObject(object instance)
    {
        var context = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        
        if (Validator.TryValidateObject(instance, context, results, true))
        {
            return ValidationResult.Success!;
        }
        
        return new ValidationResult(string.Join("; ", results.Select(r => r.ErrorMessage)));
    }
    
    public IEnumerable<ValidationResult> ValidateProperty(object instance, string propertyName, object? value)
    {
        var context = new ValidationContext(instance) { MemberName = propertyName };
        var results = new List<ValidationResult>();
        
        Validator.TryValidateProperty(value, context, results);
        return results;
    }
    
    public bool IsValid(object instance)
    {
        var context = new ValidationContext(instance);
        return Validator.TryValidateObject(instance, context, null, true);
    }
}
using System.Collections.Generic;

namespace UpgradeOctoSystem.Abstractions.Models
{
    public interface IErrorResponse
    {
        string ErrorMessage { get; set; }
        string ErrorMessageI18n { get; set; }
        IDictionary<string, ICollection<string>> Errors { get; set; }
        string ErrorType { get; set; }
    }
}
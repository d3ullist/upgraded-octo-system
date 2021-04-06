using System.Collections.Generic;
using UpgradeOctoSystem.Abstractions.Models;

namespace UpgradeOctoSystem.Api.Models
{
    public class ErrorResponse : IErrorResponse
    // 400 - 500
    {
        public string ErrorType { get; set; }
        public IDictionary<string, ICollection<string>> Errors { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorMessageI18n { get; set; }
    }
}
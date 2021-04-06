using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace UpgradeOctoSystem.Abstractions.Extensions
{
    public static class ModelStateExtensions
    {
        public static IDictionary<string, ICollection<string>> AllErrors(this ModelStateDictionary modelState)
        {
            var result = new Dictionary<string, ICollection<string>>();
            var erroneousFields = modelState.Where(ms => ms.Value.Errors.Any())
                                            .Select(x => new { x.Key, x.Value.Errors });

            foreach (var erroneousField in erroneousFields)
            {
                result.Add(erroneousField.Key.ToSnakeCase(), new List<string>());

                var value = result[erroneousField.Key.ToSnakeCase()];
                erroneousField.Errors.ToList().ForEach(error => value.Add(error.ErrorMessage));
            }

            return result;
        }
    }
}
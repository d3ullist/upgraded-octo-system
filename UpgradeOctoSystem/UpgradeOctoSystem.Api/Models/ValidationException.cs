using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using UpgradeOctoSystem.Abstractions.Extensions;

namespace UpgradeOctoSystem.Api.Models
{
    public enum ExceptionMapping
    {
        [Description("")]
        None,

        [Description("validationRules.string.password_match")]
        Register_PasswordMisMatch,

        Register_OrganizationExists,
    }

    public class ValidationException : Exception
    {
        public ExceptionMapping ExceptionMapping { get; set; }
        public ModelStateDictionary ModelState { get; set; }

        public ValidationException()
        {
        }

        public ValidationException(ExceptionMapping exception, string message = null)
            : base(message)
        {
            ExceptionMapping = exception;
        }

        public ValidationException(ExceptionMapping exception, string message, ModelStateDictionary modelState)
            : this(exception, message)
        {
            ModelState = modelState;
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ErrorResponse ToErrorResponse()
        {
            var errors = ModelState?.AllErrors();
            return new ErrorResponse
            {
                ErrorMessage = Message,
                ErrorMessageI18n = ExceptionMapping.GetAttributeOfType<DescriptionAttribute>()?.Description,
                ErrorType = ExceptionMapping.ToString(),
                Errors = errors,
            };
        }
    }
}
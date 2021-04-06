using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Services;

namespace UpgradeOctoSystem.Api.Services
{
    public class MailerService : IMailerService
    {
        private readonly IConfiguration _configuration;
        private SendGridClient _sendGridClient;
        private readonly string _baseUrl;

        public MailerService(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendGridClient = new SendGridClient(_configuration["Mailer:SendgridApiKey"]);
            _baseUrl = _configuration["Mailer:BaseUrl"];
        }

        public async Task SendForgotPasswordMailAsync(string email, string name, string resetToken, Guid userId)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_configuration["Mailer:Sender"], "Octo"),
                ReplyTo = new EmailAddress(_configuration["Mailer:Sender"], "Octo"),
                Subject = "Reset your Octo password",
                PlainTextContent =
@$"Hi {name},

Someone recently requested a password change for your Octo account.
If this was you, you can set a new password here:
{_baseUrl}/reset-password/{userId}/{WebUtility.UrlEncode(resetToken)}

To keep your account secure, please don't forward this email to anyone.

Greetings from the Octo team!
"
            };
            msg.AddTo(email, name);

            var result = await _sendGridClient.SendEmailAsync(msg).ConfigureAwait(false);
        }

        public async Task SendEmailVerificationAsync(string email, string name, string verificationToken, Guid userId)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_configuration["Mailer:Sender"], "Octo"),
                ReplyTo = new EmailAddress(_configuration["Mailer:Sender"], "Octo"),
                Subject = "Verify your Octo email",
                PlainTextContent =
@$"Hi {name},

In order to help maintain the security of your Octo account, please verify your email address.
{_baseUrl}/verify/{userId}/{WebUtility.UrlEncode(verificationToken)}

Greetings from the Octo team!
"
            };
            msg.AddTo(email, name);

            var result = await _sendGridClient.SendEmailAsync(msg).ConfigureAwait(false);
        }
    }
}
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Service;

/// <summary>
///     AWS SES email sender using the AWS Credentials Chain for authentication.
///     The AWS SDK automatically searches for credentials in this order:
///     1. Environment variables (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY)
///     2. IAM Role (when running on EC2, ECS, Lambda, etc.)
///     3. AWS Credentials file (~/.aws/credentials)
///     4. AWS Configuration file (~/.aws/config)
///     5. Instance metadata (EC2)
///     This approach is more secure than hardcoding credentials and works
///     across development, Docker, and production environments.
///     See AWS_SES_AUTHENTICATION_GUIDE.md for detailed documentation.
/// </summary>
public class AwsEmailSender : IEmailSender<UserEntity>
{
    private readonly IConfiguration _configuration;
    private readonly string _fromAddress;
    private readonly string _fromName;
    private readonly ILogger<AwsEmailSender> _logger;
    private readonly string _region;

    public AwsEmailSender(IConfiguration configuration, ILogger<AwsEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _fromAddress = configuration["Smtp:From_Address"]
                       ?? throw new ApplicationException(
                           "From address not configured in Smtp:From_Address");
        _fromName = configuration["Smtp:From_Name"]
                    ?? throw new ApplicationException("From name not configured in Smtp:From_Name");
        _region = configuration["AWS:Region"] ?? "us-east-1";

        _logger.LogInformation(
            "AwsEmailSender initialized for region: {Region}, from: {FromAddress}",
            _region, _fromAddress);
    }

    public async Task SendConfirmationLinkAsync(UserEntity user, string email,
        string confirmationLink)
    {
        var isEmailEnabled = _configuration.GetValue("EMAIL__ENABLED", true);
        if (!isEmailEnabled)
        {
            _logger.LogInformation(
                "Email notifications disabled, skipping confirmation email to {Email}", email);
            return;
        }

        try
        {
            // AWS SDK automatically finds credentials from the credentials chain
            using (var client = new AmazonSimpleEmailServiceClient(
                       RegionEndpoint.GetBySystemName(_region)))
            {
                var destination = new Destination([email]);
                var subject = new Content("Email Confirmation");
                var body = new Body
                {
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = confirmationLink
                    }
                };

                var message = new Message(subject, body);
                var source = $"{_fromName} <{_fromAddress}>";
                var request = new SendEmailRequest(source, destination, message);

                var response = await client.SendEmailAsync(request);
                _logger.LogInformation(
                    "Confirmation email sent successfully to {Email}, MessageId: {MessageId}",
                    email, response.MessageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", email);
            throw new ApplicationException($"Failed to send confirmation email to {email}", ex);
        }
    }

    public async Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink)
    {
        var isEmailEnabled = _configuration.GetValue("EMAIL__ENABLED", true);
        if (!isEmailEnabled)
        {
            _logger.LogInformation(
                "Email notifications disabled, skipping password reset email to {Email}", email);
            return;
        }

        try
        {
            using (var client = new AmazonSimpleEmailServiceClient(
                       RegionEndpoint.GetBySystemName(_region)))
            {
                var destination = new Destination([email]);
                var subject = new Content("Password Reset");
                var body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data =
                            $"<p>Please reset your password by clicking this link:</p><a href=\"{resetLink}\">Reset Password</a>"
                    }
                };

                var message = new Message(subject, body);
                var source = $"{_fromName} <{_fromAddress}>";
                var request = new SendEmailRequest(source, destination, message);

                var response = await client.SendEmailAsync(request);
                _logger.LogInformation(
                    "Password reset email sent successfully to {Email}, MessageId: {MessageId}",
                    email, response.MessageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            throw new ApplicationException($"Failed to send password reset email to {email}", ex);
        }
    }

    public async Task SendPasswordResetCodeAsync(UserEntity user, string email, string resetCode)
    {
        var isEmailEnabled = _configuration.GetValue("EMAIL__ENABLED", true);
        if (!isEmailEnabled)
        {
            _logger.LogInformation(
                "Email notifications disabled, skipping password reset code email to {Email}",
                email);
            return;
        }

        try
        {
            using (var client = new AmazonSimpleEmailServiceClient(
                       RegionEndpoint.GetBySystemName(_region)))
            {
                var destination = new Destination([email]);
                var subject = new Content("Password Reset Code");
                var body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = $"<p>Your password reset code is: <strong>{resetCode}</strong></p>"
                    }
                };

                var message = new Message(subject, body);
                var source = $"{_fromName} <{_fromAddress}>";
                var request = new SendEmailRequest(source, destination, message);

                var response = await client.SendEmailAsync(request);
                _logger.LogInformation(
                    "Password reset code email sent successfully to {Email}, MessageId: {MessageId}",
                    email, response.MessageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset code email to {Email}", email);
            throw new ApplicationException($"Failed to send password reset code email to {email}",
                ex);
        }
    }
}
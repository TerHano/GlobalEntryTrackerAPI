using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Service;

public class AwsEmailSender : IEmailSender<UserEntity>
{
    private readonly string _fromAddress;
    private readonly string _fromName;
    private readonly string AWSAccessKey;
    private readonly string AWSSecretKey;

    public AwsEmailSender(IConfiguration configuration)
    {
        var fromAddress =
            configuration["Smtp:From_Address"] ?? null;
        var fromName =
            configuration["Smtp:From_Name"] ?? null;
        AWSAccessKey = configuration["AWS:Access_Key"] ?? null;
        AWSSecretKey = configuration["AWS:Secret_Key"] ?? null;
        if (fromName == null || fromAddress == null)
            throw new ApplicationException(
                "From address or name is not set in environment variables");
        if (AWSAccessKey == null || AWSSecretKey == null)
            throw new ApplicationException(
                "AWS Access Key or Secret Key is not set in environment variables");
        _fromAddress = fromAddress;
        _fromName = fromName;
    }

    public async Task SendConfirmationLinkAsync(UserEntity user, string email,
        string confirmationLink)
    {
        using (var client = new AmazonSimpleEmailServiceClient(AWSAccessKey, AWSSecretKey,
                   RegionEndpoint.USEast1))
        {
            // Create recipient destination
            var destination = new Destination([email]);

            // Create email content
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

            // Create SendEmailRequest
            var request = new SendEmailRequest(source, destination, message);
            try
            {
                // Send the email
                await client.SendEmailAsync(request);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }

    public Task SendPasswordResetLinkAsync(UserEntity user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(UserEntity user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}
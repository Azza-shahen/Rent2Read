using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace Rent2Read.Web.BackgroundTasks
{
    public class HangfireTasks(ApplicationDbContext _dbContext
                                            , IEmailBody _emailBody
                                            , IEmailSender _emailSender)
    {
        public async Task PrepareExpirationAlert()
        {
            // Get all subscribers whose latest subscription expires in 5 days
            var subscribers = _dbContext.Subscribers
                .Include(s => s.Subscriptions)
                .Where(s => s.Subscriptions.OrderByDescending(x => x.EndDate).First().EndDate == DateTime.Today.AddDays(5))
                .ToList();

            foreach (var subscriber in subscribers)
            {
                // Format the subscription end date for email body
                var endDate = subscriber.Subscriptions.Last().EndDate.ToString("d MMM, yyyy");

                // Prepare placeholders for the email template
                var placeholders = new Dictionary<string, string>()
        {
            { "imageUrl", "https://res.cloudinary.com/rent2read/image/upload/v1756902437/calendar_zfohjc_crckuz.png" },
            { "header", $"Hello {subscriber.FirstName}," },
            { "body", $"your subscription will be expired by {endDate} 🙁" }
        };

                // Generate the email body using the placeholders
                var body = _emailBody.GetEmailBody(EmailTemplates.Notification, placeholders);

                // Send the expiration alert email to the subscriber
                await _emailSender.SendEmailAsync(
                    subscriber.Email,
                    "Rent2Read Subscription Expiration", body);
            }
        }

    }


}

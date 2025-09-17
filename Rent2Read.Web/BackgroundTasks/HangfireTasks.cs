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
                .Where(s => !s.IsBlackListed && s.Subscriptions.OrderByDescending(x => x.EndDate).First().EndDate == DateTime.Today.AddDays(5))
                .ToList();

            foreach (var subscriber in subscribers)
            {
                // Format the subscription end date for email body
                var endDate = subscriber.Subscriptions.Last().EndDate.ToString("d MMM, yyyy");

                // Prepare placeholders for the email template
                var placeholders = new Dictionary<string, string>()
        {
            { "imageUrl", "https://res.cloudinary.com/rent2read/image/upload/v1757692047/alarm_ogwzaj.webp" },
            { "header", $"Hello {subscriber.FirstName}," },
            { "body", $"your subscription will be expired by {endDate} 🙁" }
        };

                // Generate the email body using the placeholders
                var body = _emailBody.GetEmailBody(EmailTemplates.Notification, placeholders);

                // Send the expiration alert email to the subscriber
                await _emailSender.SendEmailAsync(
                    subscriber.Email,
                    "Rent2Read Subscription Expiration⚠︎", body);
            }
        }
        public async Task RentalsExpirationAlert()
        {
            var tomorrow = DateTime.Today.AddDays(1);

            var rentals = _dbContext.Rentals
                    .Include(r => r.Subscriber)
                    .Include(r => r.RentalCopies)
                    .ThenInclude(c => c.BookCopy)
                    .ThenInclude(bc => bc!.Book)
                    .Where(r => r.RentalCopies.Any(r => r.EndDate.Date == tomorrow))
                    .ToList();

            foreach (var rental in rentals)
            {
                var expiredCopies = rental.RentalCopies.Where(c => c.EndDate.Date == tomorrow && !c.ReturnDate.HasValue).ToList();

                var message = $"Your rental for the following book(s) will expire tomorrow ({tomorrow:dd MMM, yyyy}) 💔:";

                message += "<ul>";

                foreach (var copy in expiredCopies)
                {
                    message += $"<li>{copy.BookCopy!.Book!.Title}</li>";
                }

                message += "</ul>";

                var placeholders = new Dictionary<string, string>()
                {
                    { "imageUrl", "https://res.cloudinary.com/rent2read/image/upload/v1757692047/alarm_ogwzaj.webp" },
                    { "header", $"Hello {rental.Subscriber!.FirstName}," },
                    { "body", message }
                };

                var body = _emailBody.GetEmailBody(EmailTemplates.Notification, placeholders);

                await _emailSender.SendEmailAsync(
                    rental.Subscriber!.Email,
                    "Rent2Read Rental Expiration 🔔", body);
            }
        }

    }


}

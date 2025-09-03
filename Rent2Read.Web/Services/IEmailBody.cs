namespace Rent2Read.Web.Services
{
    public interface IEmailBody
    {
        public string GetEmailBody(string template, Dictionary<string, string> placeholders);

    }
}

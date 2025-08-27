namespace Rent2Read.Web.Services
{
    public interface IEmailBody
    {
        string GetEmailBody(string imageUrl, string header, string body, string url, string linkTitle);

    }
}

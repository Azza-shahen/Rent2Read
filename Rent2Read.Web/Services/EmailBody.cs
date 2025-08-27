namespace Rent2Read.Web.Services
{
    public class EmailBody(IWebHostEnvironment _webHostEnvironment) : IEmailBody
    {
        string IEmailBody.GetEmailBody(string imageUrl, string header, string body, string url, string linkTitle)
        {
            var filepath = $"{_webHostEnvironment.WebRootPath}/templates/email.html";
            // I specify the location of the HTML file that contains the message template
            StreamReader str = new(filepath);//read the file using StreamReader

            var template = str.ReadToEnd();//read all the contents of the file as a string
            str.Close();

            // perform a Replace in the template to replace the placeholders with the real values.
            return template
                     .Replace(oldValue: "[imageUrl]", newValue: imageUrl)
                     .Replace(oldValue: "[header]", newValue: header)
                     .Replace(oldValue: "[body]", newValue: body)
                     .Replace(oldValue: "[url]", newValue: url)
                     .Replace(oldValue: "[linkTitle]", newValue: linkTitle);
        }
    }
}

namespace Rent2Read.Web.Services
{
    public class EmailBody(IWebHostEnvironment _webHostEnvironment) : IEmailBody
    {
        public string GetEmailBody(string template, Dictionary<string, string> placeholders)
        {
            var filePath = $"{_webHostEnvironment.WebRootPath}/templates/{template}.html";
            // I specify the location of the HTML file that contains the message template

            StreamReader str = new(filePath);//read the file using StreamReader

            var templateContent = str.ReadToEnd();//read all the contents of the file as a string
            str.Close();

            // perform a Replace in the template to replace the placeholders with the real values.

            foreach (var placeholder in placeholders)
                templateContent = templateContent.Replace($"[{placeholder.Key}]", placeholder.Value);

            return templateContent;


        }
    }
}

using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;

namespace Application.Utilities
{

    public interface ITranslator
    {
        Task<string> TranslateTextAsync(string text, string language, string sourceLanguage);
        Task<string> TranslateHtmlAsync(string text, string language);
    }
    public class Translator : ITranslator
    {
        private readonly TranslationClient client;
        public Translator()
        {
            client = TranslationClient.Create(credential: GoogleCredential.FromFile("./Utilities/engrisk-1606464910316-6c7be56cc428.json"));
        }
        public async Task<string> TranslateHtmlAsync(string text, string language)
        {
            var response = await client.TranslateHtmlAsync(text, language);
            return response.TranslatedText;
        }
        public async Task<string> TranslateTextAsync(string text, string language, string sourceLanguage)
        {
            var response = await client.TranslateTextAsync(text, language, sourceLanguage);
            return response.TranslatedText;
        }
    }
}
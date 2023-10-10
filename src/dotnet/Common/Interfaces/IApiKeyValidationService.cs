namespace FoundationaLLM.Common.Interfaces
{
    public interface IApiKeyValidationService
    {
        bool IsValid(string apiKey);
    }
}

namespace Front_end.Services
{
    public interface IApiUrlBuilder
    {
        string Build(string relativePath);
    }

    public class ApiUrlBuilder : IApiUrlBuilder
    {
        private readonly string _baseUrl;

        public ApiUrlBuilder(IConfiguration config)
        {
            _baseUrl = config["ApiGateway:BaseUrl"];
        }

        public string Build(string relativePath)
        {
            return $"{_baseUrl}{relativePath}";
        }
    }

}

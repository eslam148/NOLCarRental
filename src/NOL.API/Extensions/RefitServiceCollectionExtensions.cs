using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NOL.Application.ExternalServices.WASL;
using Refit;

namespace NOL.API.Extensions
{
    public static class RefitServiceCollectionExtensions
    {
        public static IServiceCollection AddExternalApis(this IServiceCollection services, IConfiguration configuration)
        {
            // Demo: JSONPlaceholder API client (for testing)
            var jsonPlaceholderBaseUrl = configuration["ExternalApis:JsonPlaceholder:BaseUrl"] ?? "https://jsonplaceholder.typicode.com";

            services
                .AddRefitClient<JsonPlaceholderApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(jsonPlaceholderBaseUrl));

            // WASL API (Saudi Arabia Fleet Management System)
            var waslBaseUrl = configuration["ExternalApis:WASL:BaseUrl"] ?? "https://wasl.api.elm.sa";
            var waslClientId = configuration["ExternalApis:WASL:ClientId"] ?? "";
            var waslAppId = configuration["ExternalApis:WASL:AppId"] ?? "";
            var waslAppKey = configuration["ExternalApis:WASL:AppKey"] ?? "";

            services
                .AddRefitClient<IWaslApiService>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(waslBaseUrl);
                    c.Timeout = TimeSpan.FromSeconds(30);
                })
                .AddHttpMessageHandler(() => new WaslApiMessageHandler(waslClientId, waslAppId, waslAppKey));

            // Register WASL wrapper service
            services.AddScoped<IWaslService, WaslService>();

            return services;
        }
    }

    /// <summary>
    /// Custom message handler for WASL API
    /// Automatically adds WASL authentication headers to all requests
    /// </summary>
    public class WaslApiMessageHandler : DelegatingHandler
    {
        private readonly string _clientId;
        private readonly string _appId;
        private readonly string _appKey;

        public WaslApiMessageHandler(string clientId, string appId, string appKey)
        {
            _clientId = clientId;
            _appId = appId;
            _appKey = appKey;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Add WASL required authentication headers
            if (!request.Headers.Contains("client-id"))
            {
                request.Headers.Add("client-id", _clientId);
            }
            
            if (!request.Headers.Contains("app-id"))
            {
                request.Headers.Add("app-id", _appId);
            }
            
            if (!request.Headers.Contains("app-key"))
            {
                request.Headers.Add("app-key", _appKey);
            }

            // Add standard headers
            if (!request.Headers.Contains("Accept"))
            {
                request.Headers.Add("Accept", "application/json");
            }
            
            if (!request.Headers.Contains("User-Agent"))
            {
                request.Headers.Add("User-Agent", "NOL-CarRental/1.0");
            }

            // Add content type for POST/PUT requests
            if (request.Content != null && request.Content.Headers.ContentType == null)
            {
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            }

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                
                // Log error responses for debugging
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"WASL API Error: {response.StatusCode} - {content}");
                    Console.WriteLine($"Request URL: {request.RequestUri}");
                }

                return response;
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("WASL API request timed out");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"WASL API request failed: {ex.Message}", ex);
            }
        }
    }

    #region Demo API (JSONPlaceholder)

    public interface JsonPlaceholderApi
    {
        [Get("/todos/{id}")]
        Task<TodoItem> GetTodoAsync(int id);
    }

    public sealed class TodoItem
    {
        public int UserId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool Completed { get; set; }
    }

    #endregion
}



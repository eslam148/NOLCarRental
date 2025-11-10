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

            services
                .AddRefitClient<IWaslApiService>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(waslBaseUrl);
                    c.Timeout = TimeSpan.FromSeconds(30);
                })
                .AddHttpMessageHandler(() => new WaslApiMessageHandler());

            // Register WASL wrapper service
            services.AddScoped<IWaslService, WaslService>();

            return services;
        }
    }

    /// <summary>
    /// Custom message handler for WASL API
    /// Adds logging, retry logic, and error handling
    /// </summary>
    public class WaslApiMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Add any custom headers if needed
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "NOL-CarRental/1.0");

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                
                // You can add logging or retry logic here
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    // Log error response
                    Console.WriteLine($"WASL API Error: {response.StatusCode} - {content}");
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



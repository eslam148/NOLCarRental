using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace NOL.API.Extensions
{
    public static class RefitServiceCollectionExtensions
    {
        public static IServiceCollection AddExternalApis(this IServiceCollection services, IConfiguration configuration)
        {
            // Example: Demo JSONPlaceholder API client
            var baseUrl = configuration["ExternalApis:JsonPlaceholder:BaseUrl"] ?? "https://jsonplaceholder.typicode.com";

            services
                .AddRefitClient<JsonPlaceholderApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));

            return services;
        }
    }

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
}



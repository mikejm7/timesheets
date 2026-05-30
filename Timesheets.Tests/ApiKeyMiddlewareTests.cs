using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Timesheets.API.Data;
using System.Linq;

namespace Timesheets.Tests
{
    public class ApiKeyMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ApiKeyMiddlewareTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor1 = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor1 != null) services.Remove(descriptor1);

                    var descriptor2 = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions));
                    if (descriptor2 != null) services.Remove(descriptor2);

                    var allEFDescriptors = services
                        .Where(d => d.ServiceType.Namespace != null && d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore"))
                        .ToList();

                    foreach (var d in allEFDescriptors)
                    {
                        services.Remove(d);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Add NewtonSoftJson to bypass the SystemTextJsonOutputFormatter bug in TestHost
                    services.AddControllers().AddNewtonsoftJson();
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetJobs_WithoutApiKey_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/jobs");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetJobs_WithInvalidApiKey_ReturnsUnauthorized()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/jobs");
            request.Headers.Add("X-Api-Key", "invalid-key-123");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetJobs_WithValidApiKey_ReturnsOk()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/jobs");
            request.Headers.Add("X-Api-Key", "YOUR_SECURE_KEY");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}

using AutoMapper;
using FluentAssertions;
using IdentityApi.DTOs;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace IdentityApi.IntegrationTests.Controllers
{
    public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<ICompanyService> _companyServiceMock;
        private readonly IMapper _mapper;

        private readonly HttpClient _client;

        private readonly Mock<ICompanyService> _mockCompanyService;

        public CompanyControllerTests(WebApplicationFactory<Program> factory)
        {
            // Mock ICompanyService
            _mockCompanyService = new Mock<ICompanyService>();

            // Create a client and inject the mock service
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the actual service registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICompanyService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add mock service registration
                    services.AddScoped(_ => _mockCompanyService.Object);
                });
            }).CreateClient();
        }

        [Fact]
        public async Task GetCompanies_ShouldReturnOk_WhenCompaniesExist()
        {
            // Arrange
            var companies = new List<Company> {
                new Company { Id = 1, Name = "Company A" },
                new Company { Id = 2, Name = "Company B" }
            };

            _mockCompanyService
                .Setup(service => service.GetCompanies(It.IsAny<CancellationToken>()))
                .ReturnsAsync(companies);

            var endpoint = "/api/company";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Company A").And.Contain("Company B");
        }

        [Fact]
        public async Task GetCompanies_ShouldReturnNoContent_WhenNoCompaniesExist()
        {
            // Arrange
            _mockCompanyService
                .Setup(service => service.GetCompanies(It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Company>)null); // No companies

            var endpoint = "/api/company";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task CreateCompany_ShouldReturnCreated_WhenCompanyIsCreated()
        {
            // Arrange
            var companyPostModel = new CompanyPostModel
            {
                Name = "New Company",
                Street = "123 Test Street",
                MaticniBroj = "12345678",
                Pib = "123456789"
                // Add other fields as necessary
            };

            // Mock the service to return a new Company ID when CreateCompany is called
            _mockCompanyService
                .Setup(service => service.CreateCompany(It.IsAny<Company>()))
                .ReturnsAsync(1);  // Assuming the new company ID is 1

            // Serialize the request content
            var jsonContent = new StringContent(JsonSerializer.Serialize(companyPostModel), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/company", jsonContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("id").And.Contain("1");
        }

        [Fact]
        public async Task CreateCompany_ShouldReturnBadRequest_WhenCompanyPostModelIsInvalid()
        {
            // Arrange: send an empty JSON object
            var jsonContent = new StringContent("{}", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/company", jsonContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Instead of checking the whole response, you can check for key parts of it
            responseContent.Should().Contain("One or more validation errors occurred.");
            responseContent.Should().Contain("Company name is required");
            responseContent.Should().Contain("'Pib' must not be empty.");
        }

        
    }
}

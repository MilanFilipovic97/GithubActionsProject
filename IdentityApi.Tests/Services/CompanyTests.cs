using AutoMapper;
using IdentityApi.Database;
using IdentityApi.DTOs;
using IdentityApi.Models;
using IdentityApi.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityApi.Tests.Services
{
    [TestClass]
    public class CompanyTests
    {
        private DataContext _dataContext;
        private IMapper _mapper;
        private CompanyService _companyService;


        [TestInitialize]
        public void Setup()
        {
            // Use InMemory database for the DataContext
            var options = new DbContextOptionsBuilder<DataContext>()
                            .UseInMemoryDatabase(databaseName: "TestDatabase")
                            .Options;

            _dataContext = new DataContext(options);

            // Seed some data into the InMemory database
            _dataContext.Companies.AddRange(
                new Company { Id = 1, Name = "Company A" },
                new Company { Id = 2, Name = "Company B" }
            );
            _dataContext.SaveChanges();

            // Optionally set up IMapper (in case your service uses mapping in other methods)
            var mapperConfig = new MapperConfiguration(cfg => { /* Add any mapping profiles if needed */ });
            _mapper = mapperConfig.CreateMapper();

            // Initialize the CompanyService
            _companyService = new CompanyService(_dataContext, _mapper);
        }

        [TestMethod]
        public async Task GetCompanies_ReturnsListOfCompanies()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _companyService.GetCompanies(cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count); // Check that we have 2 companies
            Assert.AreEqual("Company A", result[0].Name); // Validate company names
            Assert.AreEqual("Company B", result[1].Name);
        }

        [TestMethod]
        public async Task DeleteCompany_ExistingCompany_DeletesCompany()
        {
            // Arrange
            var companyId = 1; // Existing company ID
            var cancellationToken = CancellationToken.None;

            // Act
            await _companyService.DeleteCompany(companyId, cancellationToken);

            // Assert
            var deletedCompany = await _dataContext.Companies.FindAsync(companyId);
            Assert.IsNull(deletedCompany); // Ensure the company was deleted
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DeleteCompany_NonExistentCompany_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentCompanyId = 999; // Non-existent company ID
            var cancellationToken = CancellationToken.None;

            // Act
            await _companyService.DeleteCompany(nonExistentCompanyId, cancellationToken);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public async Task CreateCompany_ValidCompany_ReturnsNewCompanyId()
        {
            // Arrange
            var newCompany = new Company
            {
                Name = "New Company"
            };

            // Act
            var result = await _companyService.CreateCompany(newCompany);

            // Assert
            var createdCompany = await _dataContext.Companies.FindAsync(result);
            Assert.IsNotNull(createdCompany); // Ensure the company was created
            Assert.AreEqual("New Company", createdCompany.Name); // Verify the name
            Assert.AreEqual(result, createdCompany.Id); // Verify the returned ID matches the created entity's ID
        }

        [TestMethod]
        public async Task UpdateCompanyUsingReflection_ExistingCompany_UpdatesCorrectly()
        {
            // Arrange
            var companyId = 1; // Existing company ID
            var updatedCompany = new CompanyPostModel
            {
                Name = "Updated Company A",
                Street = "Updated Address A"
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _companyService.UpdateCompanyUsingReflection(companyId, updatedCompany);

            // Assert
            var updatedEntity = await _dataContext.Companies.FindAsync(companyId);
            Assert.IsNotNull(updatedEntity); // Ensure company exists
            Assert.AreEqual("Updated Company A", updatedEntity.Name); // Verify name was updated
            Assert.AreEqual("Updated Address A", updatedEntity.Street); // Verify address was updated
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task UpdateCompanyUsingReflection_NonExistentCompany_ThrowsKeyNotFoundException()
        {
            // Arrange
            var nonExistentCompanyId = 999; // Non-existent company ID
            var updatedCompany = new CompanyPostModel
            {
                Name = "Non Existent Company",
                Street = "Non Existent Address"
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _companyService.UpdateCompanyUsingReflection(nonExistentCompanyId, updatedCompany);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public async Task UpdateCompanyUsingReflection_IgnoreNullAndWhitespaceValues_SkipsUpdatingProperties()
        {
            // Arrange
            var companyId = 2; // Existing company ID
            var updatedCompany = new CompanyPostModel
            {
                Name = "  ", // Intentionally whitespace
                Street = null // Intentionally null
            };
            var cancellationToken = CancellationToken.None;

            // Act
            await _companyService.UpdateCompanyUsingReflection(companyId, updatedCompany);

            // Assert
            var updatedEntity = await _dataContext.Companies.FindAsync(companyId);
            Assert.IsNotNull(updatedEntity); // Ensure company exists
            Assert.AreEqual("Company B", updatedEntity.Name); // Name should remain unchanged
            Assert.AreEqual("Address B", updatedEntity.Street); // Address should remain unchanged
        }
        // This test in here is failing. Pay attention why since it actually find a bug and to fix it
        // you need to improve service that does this stuff.


        [TestCleanup]
        public void Cleanup()
        {
            // Dispose the in-memory database after each test to avoid conflicts
            _dataContext.Database.EnsureDeleted();
            _dataContext.Dispose();
        }
    }
}

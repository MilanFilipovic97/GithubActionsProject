using AutoMapper;
using IdentityApi.Controllers;
using IdentityApi.DTOs;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IdentityApi.Tests.Controllers
{
    [TestClass]
    public class CompanyControllerTests
    {
        private Mock<ICompanyService> _companyServiceMock;
        private Mock<IMapper> _mapperMock;
        private CompanyController _controller;

        [TestInitialize]
        public void Setup()
        {
            _companyServiceMock = new Mock<ICompanyService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new CompanyController(_companyServiceMock.Object, _mapperMock.Object);
        }

        [TestMethod]
        public async Task GetCompanies_ReturnsOkResult_WhenCompaniesExist()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var companyList = new List<Company>
            {
                new Company { Id = 1, Name = "Test 1" },
                new Company { Id = 2, Name = "Test 2" },
            };

            /*
            This line down is using Moq, a popular library for creating mock objects in unit testing. 
            It is used to set up the behavior of the mock ICompanyService so that it returns the 
            expected data when its method is called.
             
            _companyServiceMock is a mock object of the ICompanyService interface created by Moq.

            Setup is a method provided by Moq that allows you to specify how the mock object 
            should behave when its methods are called. In this case, you're setting up the 
            behavior for the GetCompanies method of ICompanyService.

            service => service.GetCompanies(It.IsAny<CancellationToken>())

            This is a lambda expression that specifies which method you want to mock (GetCompanies in this case).

            It.IsAny<CancellationToken>() is a Moq matcher that tells the mock object to accept any CancellationToken
            value as an argument when GetCompanies is called. It means that the mock will behave the same way no
            matter what specific CancellationToken is passed in.

            It is a class provided by Moq, and IsAny<T>() is a method that indicates the mock should match any 
            value of type T (in this case, CancellationToken).

            This is useful in tests because you may not care about the actual value of the CancellationToken in 
            this context. You're only interested in how the method behaves when it’s called.

            .ReturnsAsync(companyList)
            ReturnsAsync specifies what the mock method should return when it is called.
            In this case, companyList (a predefined list of Company objects) is returned asynchronously when the 
              GetCompanies method is called.
            */
            _companyServiceMock
                .Setup(service => service.GetCompanies(It.IsAny<CancellationToken>()))
                .ReturnsAsync(companyList);

            // Act

            var result = await _controller.GetCompanies(cancellationToken);

            // Assert

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var returnedCompanies = okResult.Value as List<Company>;
            Assert.AreEqual(2, returnedCompanies!.Count);
        }

        /* u delete metodi nemas bazu nit kolekciju nego se oslanjas na logiku brisanja u ovom servisu
         mokira se ICompanyService.
         since this is a unit test, you're testing the controller method in isolation by using Moq to simulate the behavior of the service.
        If you were writing an integration test (rather than a unit test), you might be working
        with an actual data source like a list or database where companies are stored.*/
        
        [TestMethod]
        public async Task DeleteCompany_ValidId_ReturnsNoContent()
        {
            // Arrange
            int validCompanyId = 1;
            var cancellationToken = new CancellationToken();

            // Mock the DeleteCompany method to simulate successful deletion
            _companyServiceMock
                .Setup(service => service.DeleteCompany(validCompanyId, cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCompany(validCompanyId, cancellationToken);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult); // Ensure the result is NoContentResult
            Assert.AreEqual(204, noContentResult.StatusCode); // Check for 204 No Content
        }

        [TestMethod]
        public async Task CreateCompany_ValidModel_ReturnsCreatedResult()
        {
            // Arrange
            var companyPostModel = new CompanyPostModel { Name = "Test Company" };
            var company = new Company { Name = "Test Company" };
            int generatedCompanyId = 1;

            _mapperMock.Setup(mapper => mapper.Map<Company>(companyPostModel)).Returns(company);
            _companyServiceMock.Setup(service => service.CreateCompany(company)).ReturnsAsync(generatedCompanyId);

            var cancellationToken = new CancellationToken();

            // Act
            var result = await _controller.CreateCompany(companyPostModel, cancellationToken);

            // Assert
            var createdResult = result.Result as CreatedResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode); // HTTP 201 Created
            //Assert.AreEqual(generatedCompanyId, ((dynamic)createdResult.Value!).Id); // Check returned ID
        }

        [TestMethod]
        public async Task CreateCompany_NullModel_ReturnsBadRequest()
        {
            // Arrange
            CompanyPostModel nullModel = null;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _controller.CreateCompany(nullModel, cancellationToken);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode); // HTTP 400 Bad Request
            Assert.AreEqual("Company data is required.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task CreateCompany_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var companyPostModel = new CompanyPostModel { Name = "Test Company" };
            var company = new Company { Name = "Test Company" };
            var cancellationToken = new CancellationToken();

            _mapperMock.Setup(mapper => mapper.Map<Company>(companyPostModel)).Returns(company);
            _companyServiceMock.Setup(service => service.CreateCompany(company)).ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.CreateCompany(companyPostModel, cancellationToken);

            // Assert
            var statusCodeResult = result.Result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(500, statusCodeResult.StatusCode); // HTTP 500 Internal Server Error
            Assert.AreEqual("Service error", statusCodeResult.Value);
        }

        // patch 
        /*
         Here's how the unit tests for the UpdateCompany endpoint could be written using MSTest. We'll cover three different cases:

        Valid Input Using Reflection – Test for a successful update when reflection is true.
        Valid Input Using AutoMapper – Test for a successful update when reflection is false.
        Invalid Input – Test for cases where companyPostModel is null or id <= 0.
        KeyNotFoundException – Test for cases where the company doesn't exist.
        Exception Handling – Test for cases where an unhandled exception occurs.
         */

        [TestMethod]
        public async Task UpdateCompany_ValidInput_UsesReflection_ReturnsNoContent()
        {
            // Arrange
            int validId = 1;
            var companyPostModel = new CompanyPostModel { Name = "Test Company" };

            // Mock the service method to simulate a successful update using reflection
            _companyServiceMock
                .Setup(service => service.UpdateCompanyUsingReflection(validId, companyPostModel))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateCompany(validId, companyPostModel, reflection: true);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode); // Check for HTTP 204 No Content
        }

        [TestMethod]
        public async Task UpdateCompany_ValidInput_UsesAutoMapper_ReturnsNoContent()
        {
            // Arrange
            int validId = 1;
            var companyPostModel = new CompanyPostModel { Name = "Test Company" };

            // Mock the service method to simulate a successful update using AutoMapper
            _companyServiceMock
                .Setup(service => service.UpdateCompanyUsingAutoMapper(validId, companyPostModel))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateCompany(validId, companyPostModel, reflection: false);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode); // Check for HTTP 204 No Content
        }

        [TestMethod]
        public async Task UpdateCompany_NullModel_ReturnsBadRequest()
        {
            // Arrange
            CompanyPostModel nullModel = null;
            int validId = 1;

            // Act
            var result = await _controller.UpdateCompany(validId, nullModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode); // HTTP 400 Bad Request
            Assert.AreEqual("Invalid company data.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task UpdateCompany_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var companyPostModel = new CompanyPostModel { Name = "Test Company" };
            int invalidId = -1; // Invalid company ID

            // Act
            var result = await _controller.UpdateCompany(invalidId, companyPostModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode); // HTTP 400 Bad Request
            Assert.AreEqual("Invalid company data.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task UpdateCompany_CompanyNotFound_ReturnsNotFound()
        {
            // Arrange
            int validId = 1;
            var companyPostModel = new CompanyPostModel { Name = "Test Company" };

            // Mock the service method to throw KeyNotFoundException
            _companyServiceMock
                .Setup(service => service.UpdateCompanyUsingReflection(validId, companyPostModel))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.UpdateCompany(validId, companyPostModel, reflection: true);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode); // HTTP 404 Not Found
        }

        [TestMethod]
        public async Task UpdateCompany_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            int validId = 1;
            var companyPostModel = new CompanyPostModel { Name = "Test Company" };

            // Mock the service method to throw a general exception
            _companyServiceMock
                .Setup(service => service.UpdateCompanyUsingReflection(validId, companyPostModel))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.UpdateCompany(validId, companyPostModel, reflection: true);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(500, statusCodeResult.StatusCode); // HTTP 500 Internal Server Error
            Assert.AreEqual("Internal server error: {ex.Message}.", statusCodeResult.Value);
        }

        // put full update

        [TestMethod]
        public async Task UpdateCompany_ValidInput_ReturnsNoContent()
        {
            // Arrange
            int validId = 1;
            var companyPostModel = new CompanyPostModel { Name = "Updated Company" };
            var mappedCompany = new Company { Id = validId, Name = "Updated Company" };

            // Mock the mapper and service
            _mapperMock
                .Setup(mapper => mapper.Map<Company>(companyPostModel))
                .Returns(mappedCompany);

            _companyServiceMock
                .Setup(service => service.UpdateCompany(validId, mappedCompany))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateCompany(validId, companyPostModel);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode); // HTTP 204 No Content
        }

        [TestMethod]
        public async Task UpdateCompanyFull_NullModel_ReturnsBadRequest()
        {
            // Arrange
            CompanyPostModel nullModel = null;
            int validId = 1;

            // Act
            var result = await _controller.UpdateCompany(validId, nullModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode); // HTTP 400 Bad Request
            Assert.AreEqual("Invalid company data.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task UpdateCompanyFull_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var companyPostModel = new CompanyPostModel { Name = "Updated Company" };
            int invalidId = -1; // Invalid company ID

            // Act
            var result = await _controller.UpdateCompany(invalidId, companyPostModel);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode); // HTTP 400 Bad Request
            Assert.AreEqual("Invalid company data.", badRequestResult.Value);
        }

        [TestMethod]
        public async Task UpdateCompanyFull_CompanyNotFound_ReturnsNotFound()
        {
            // Arrange
            int validId = 1;
            var companyPostModel = new CompanyPostModel { Name = "Updated Company" };
            var mappedCompany = new Company { Id = validId, Name = "Updated Company" };

            // Mock the mapper and service to throw KeyNotFoundException
            _mapperMock
                .Setup(mapper => mapper.Map<Company>(companyPostModel))
                .Returns(mappedCompany);

            _companyServiceMock
                .Setup(service => service.UpdateCompany(validId, mappedCompany))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.UpdateCompany(validId, companyPostModel);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode); // HTTP 404 Not Found
        }

        [TestMethod]
        public async Task UpdateCompanyFull_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            int validId = 1;
            var companyPostModel = new CompanyPostModel { Name = "Updated Company" };
            var mappedCompany = new Company { Id = validId, Name = "Updated Company" };

            // Mock the mapper and service to throw a general exception
            _mapperMock
                .Setup(mapper => mapper.Map<Company>(companyPostModel))
                .Returns(mappedCompany);

            _companyServiceMock
                .Setup(service => service.UpdateCompany(validId, mappedCompany))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.UpdateCompany(validId, companyPostModel);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(500, statusCodeResult.StatusCode); // HTTP 500 Internal Server Error
            Assert.AreEqual("Internal server error: Service error", statusCodeResult.Value);
        }
    }
}

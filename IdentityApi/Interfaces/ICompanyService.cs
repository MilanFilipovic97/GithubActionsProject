using IdentityApi.DTOs;
using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface ICompanyService
    {
        Task<List<Company>> GetCompanies(CancellationToken cancellationToken);
        Task DeleteCompany(int id, CancellationToken cancellationToken);
        Task<int> CreateCompany(Company company);
        Task UpdateCompanyUsingAutoMapper(int id, CompanyPostModel CompanyPostModel);
        Task UpdateCompanyUsingReflection(int id, CompanyPostModel CompanyPostModel);
        Task UpdateCompany(int id, Company company);
    }
}

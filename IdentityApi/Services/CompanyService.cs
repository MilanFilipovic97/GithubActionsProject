using AutoMapper;
using IdentityApi.Database;
using IdentityApi.DTOs;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityApi.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        public CompanyService(DataContext context, IMapper mapper)
        {
            _dataContext = context;
            _mapper = mapper;
        }
        public async Task<List<Company>> GetCompanies(CancellationToken cancellationToken)
        {
            return await _dataContext.Companies.ToListAsync(cancellationToken);
        }

        public async Task DeleteCompany(int id, CancellationToken cancellationToken)
        {
            var company = await _dataContext.Companies.FindAsync(id, cancellationToken);

            if (company == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }

            _dataContext.Companies.Remove(company);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CreateCompany(Company company)
        {
            _dataContext.Add(company);
            await _dataContext.SaveChangesAsync();
            return company.Id;
        }

        public async Task UpdateCompanyUsingReflection(int id, CompanyPostModel updatedCompany)
        {
            var companyResult = await _dataContext.Companies.FindAsync(id);
            if (companyResult == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }
            
            var properties = typeof(CompanyPostModel).GetProperties(); // Get properties of the input model
            
            foreach (var property in properties)
            {
                var updatedValue = property.GetValue(updatedCompany); // Get value from CompanyPostModel (updatedCompany)
                
                if (updatedValue != null)
                {
                    // Handle different types (e.g., strings) and update only if value is meaningful
                    if (property.PropertyType == typeof(string) && string.IsNullOrWhiteSpace((string)updatedValue))
                    {
                        continue; // Skip if the string is empty or whitespace
                    }
                    var companyProperty = companyResult.GetType().GetProperty(property.Name); // Match the property on Company
            
                    if (companyProperty != null && companyProperty.CanWrite)
                    {
                        companyProperty.SetValue(companyResult, updatedValue); // Set the value on Company
                    }
                }
            }
            await _dataContext.SaveChangesAsync();
        }


        // testovi do ovde unit odradjeni
        public async Task UpdateCompanyUsingAutoMapper(int id, CompanyPostModel updatedCompany)
        {
            var companyResult = await _dataContext.Companies.FindAsync(id);
            if (companyResult == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }
            _mapper.Map(updatedCompany, companyResult);
            await _dataContext.SaveChangesAsync();
        }

        public async Task UpdateCompany(int id, Company updatedCompany)
        {
            var companyResult = await _dataContext.Companies.FindAsync(id);
            if (companyResult == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }

            // Prevent modifying the primary key (Id)
            updatedCompany.Id = companyResult.Id;
            // Alternative is to manual assign values for all properties
            // companyResult.Name = updatedCompany.Name;
            _dataContext.Entry(companyResult).CurrentValues.SetValues(updatedCompany);
            await _dataContext.SaveChangesAsync();
        }
    }
}

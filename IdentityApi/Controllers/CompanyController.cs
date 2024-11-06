using AutoMapper;
using IdentityApi.DTOs;
using IdentityApi.Interfaces;
using IdentityApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IMapper _mapper;
        public CompanyController(ICompanyService companyService, IMapper mapper)
        {
            _companyService = companyService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<Company>>> GetCompanies(CancellationToken cancellationToken)
        {
            var companies = await _companyService.GetCompanies(cancellationToken);

            if (companies == null)
            {
                return NoContent();
            }

            return Ok(companies);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCompany(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid company ID.");
            }
            try
            {
                await _companyService.DeleteCompany(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Company not found.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateCompany(
            CompanyPostModel companyPostModel,
            CancellationToken cancellationToken)
        {

            if (companyPostModel == null)
            {
                return BadRequest("Company data is required.");
            }

            try
            {
                var company = _mapper.Map<Company>(companyPostModel);
                int companyId = await _companyService.CreateCompany(company);
                
                return Created(string.Empty, new { id = companyId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // pogledati strategy design pattern da se registrira refleksija ili auto mapper
        // napraviti servis koji se moze injectati u company servis za tu logiku
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateCompany(
            int id,
            CompanyPostModel companyPostModel,
            bool reflection = true)
        {
            if (companyPostModel == null || id <= 0)
            {
                return BadRequest("Invalid company data.");
            }
            try
            {
                if (reflection)
                {
                    await _companyService.UpdateCompanyUsingReflection(id, companyPostModel);
                }
                else
                {
                    await _companyService.UpdateCompanyUsingAutoMapper(id, companyPostModel);
                }
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: {ex.Message}.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCompany(
            int id,
            CompanyPostModel companyPostModel)
        {
            if (companyPostModel == null || id <= 0)
            {
                return BadRequest("Invalid company data.");
            }
            try
            {
                var company = _mapper.Map<Company>(companyPostModel);
                await _companyService.UpdateCompany(id, company);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

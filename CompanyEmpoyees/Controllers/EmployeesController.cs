using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Filters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using LoggerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repository;
        private readonly IDataShaper<EmployeeDTO> _dataShaper;

        public EmployeesController(ILoggerManager logger, IMapper mapper, IRepositoryManager repository, IDataShaper<EmployeeDTO> dataShaper)
        {
            _logger = logger;
            _mapper = mapper;
            _repository = repository;
            _dataShaper = dataShaper;
        }
        [HttpGet]
        [HttpHead]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
            {
                return BadRequest("Max age can't be less than min age.");
            }

            var company = await _repository.Company.GetCompanyAsync(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Something with id: {companyId} doesn't exist in the database");
                return NotFound();
            }

            var employeeFromDb = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeeFromDb.MetaData));

            var employeeDto = _mapper.Map<IEnumerable<EmployeeDTO>>(employeeFromDb);

            return Ok(_dataShaper.ShapeData(employeeDto, employeeParameters.Fields));
            
        }


        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Something with id: {companyId} doesn't exist in the database");
                return NotFound();
            }  

            var employee = await _repository.Employee.GetEmployeeAsync(companyId, id);
            if (employee == null)
            {
                _logger.LogInfo($"Something with id: {id} doesn't exist in the database");
                return NotFound();
            }

            return Ok(_mapper.Map<EmployeeDTO>(employee));
        }


        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDTO employee)
        {

            var company = await _repository.Company.GetCompanyAsync(companyId);
            if (company == null)
            {
                _logger.LogInfo($"Something with id: {companyId} doesn't exist in the database");
                return NotFound();
            }


            var employeeEntity = _mapper.Map<Employee>(employee);

            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeEntity);
            return CreatedAtRoute("GetEmployeeForCompany", new {companyId, id = employeeToReturn.Id }, employeeToReturn);
        }


        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var employeeForCompany = HttpContext.Items["employee"] as Employee;
            

            _repository.Employee.DeteleEmployee(employeeForCompany);
            await _repository.SaveAsync();


            return NoContent();
        }


        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDTO employee)
        {
            var employeeForCompany = HttpContext.Items["employee"] as Employee;

            _mapper.Map(employee, employeeForCompany);
            await _repository.SaveAsync();


            return NoContent();
        }


        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }


            var employeeEntity = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDTO>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);
            await _repository.SaveAsync();

            return NoContent();
        }
    }
}

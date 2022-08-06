using Entities.Models;
using Entities.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IEmployeeRepository
    {
        Task<PageList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges = false);
        Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges = false);
        void CreateEmployeeForCompany(Guid companyId, Employee employee);
        void DeteleEmployee(Employee employee);
    }
}

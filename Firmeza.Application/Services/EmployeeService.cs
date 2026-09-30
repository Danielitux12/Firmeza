using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Services;

public class EmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _employeeRepository.GetAllAsync(cancellationToken);
    }

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _employeeRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        if (!employee.IsValid())
        {
            throw new InvalidOperationException("Employee data is not valid.");
        }

        return await _employeeRepository.AddAsync(employee, cancellationToken);
    }
}

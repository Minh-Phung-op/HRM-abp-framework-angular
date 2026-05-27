using Acme.HRM.Dtos;
using Acme.HRM.Entities;
using Acme.HRM.Enums;
using Acme.HRM.InterfaceAppService;
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Acme.HRM.ApplicationServices
{
    [Authorize] // Bạn có thể thay bằng Permissions cụ thể
    public class ContractAppService : ApplicationService, IContractAppService
    {
        private readonly IRepository<Contract, long> _contractRepository;
        private readonly IRepository<Employee, long> _employeeRepository;

        public ContractAppService(
            IRepository<Contract, long> contractRepository,
            IRepository<Employee, long> employeeRepository)
        {
            _contractRepository = contractRepository;
            _employeeRepository = employeeRepository;
        }

        // Lấy danh sách hợp đồng (chủ yếu dùng bộ lọc EmployeeId)
        public async Task<PagedResultDto<ContractDto>> GetListAsync(GetAllContractsInput input)
        {
            var queryable = await _contractRepository.GetQueryableAsync();

            // Luôn ưu tiên lọc theo EmployeeId nếu được truyền vào từ UI
            var query = queryable
                .WhereIf(input.EmployeeId.HasValue, x => x.EmployeeId == input.EmployeeId)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.ContractNumber.Contains(input.Keyword))
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

            var totalCount = await AsyncExecuter.CountAsync(query);
            var contracts = await AsyncExecuter.ToListAsync(
                query.OrderBy(input.Sorting ?? nameof(Contract.StartDate) + " desc")
                     .PageBy(input.SkipCount, input.MaxResultCount)
            );

            return new PagedResultDto<ContractDto>(
                totalCount,
                ObjectMapper.Map<List<Contract>, List<ContractDto>>(contracts)
            );
        }

        // Lấy chi tiết một hợp đồng
        public async Task<ContractDto> GetAsync(long id)
        {
            var contract = await _contractRepository.GetAsync(id);
            return ObjectMapper.Map<Contract, ContractDto>(contract);
        }

        // Tạo mới hợp đồng
        public async Task<ContractDto> CreateAsync(CreateContractDto input)
        {
            // 1. Kiểm tra nhân viên
            var employee = await _employeeRepository.GetAsync(input.EmployeeId);
            if (employee == null) 
            {
                throw new UserFriendlyException("Employee không tồn tại");
            }

            // 2. Nếu hợp đồng mới là Active, tìm và deactivate các hợp đồng cũ
            if (input.Status == ContractStatus.Active)
            {
                await DeactivateOldContracts(input.EmployeeId);
            }

            // 3. Tạo hợp đồng
            var contract = ObjectMapper.Map<CreateContractDto, Contract>(input);
            var inserted = await _contractRepository.InsertAsync(contract);

            return ObjectMapper.Map<Contract, ContractDto>(inserted);
        }

        public async Task<ContractDto> UpdateAsync(long id, UpdateContractDto input)
        {
            var contract = await _contractRepository.GetAsync(id);
            var employee = await _employeeRepository.GetAsync(contract.EmployeeId);
            if(employee == null)
            {
                throw new UserFriendlyException("Employee không tồn tại");
            }

            // Nếu chuyển trạng thái thành Active
            if (input.Status == ContractStatus.Active)
            {
                await DeactivateOldContracts(contract.EmployeeId, id);
            }

            ObjectMapper.Map(input, contract);
            var updated = await _contractRepository.UpdateAsync(contract);

            return ObjectMapper.Map<Contract, ContractDto>(updated);
        }

        /// <summary>
        /// Chuyển tất cả hợp đồng hiện tại của nhân viên về Expired/Terminated để đảm bảo duy nhất 1 bản ghi Active
        /// </summary>
        private async Task DeactivateOldContracts(long employeeId, long? currentContractId = null)
        {
            var queryable = await _contractRepository.GetQueryableAsync();
            var oldActiveContracts = queryable.Where(x =>
                x.EmployeeId == employeeId &&
                x.Status == ContractStatus.Active &&
                x.Id != currentContractId).ToList();

            foreach (var oldContract in oldActiveContracts)
            {
                oldContract.Status = ContractStatus.Expired; // Hoặc Terminated tùy logic
                await _contractRepository.UpdateAsync(oldContract);
            }
        }

        // Xóa hợp đồng
        public async Task DeleteAsync(long id)
        {
            await _contractRepository.DeleteAsync(id);
        }
    }
}

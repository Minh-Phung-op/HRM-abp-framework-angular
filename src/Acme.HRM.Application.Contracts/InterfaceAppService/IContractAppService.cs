using Acme.HRM.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.HRM.InterfaceAppService
{
    public interface IContractAppService : IApplicationService
    {
        Task<PagedResultDto<ContractDto>> GetListAsync(GetAllContractsInput input);
        Task<ContractDto> GetAsync(long id);
        Task<ContractDto> CreateAsync(CreateContractDto input);
        Task<ContractDto> UpdateAsync(long id, UpdateContractDto input);
        Task DeleteAsync(long id);
    }
}

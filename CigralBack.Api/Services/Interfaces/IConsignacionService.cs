using CigralBackend.Application.Dtos;
using CigralBackend.Domain.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Services.Interfaces
{
    public interface IConsignacionService
    {
        Task<ConsignacionResponse> AumentarConsignacion(ConsignacionRequest request);
        Task<ConsignacionResponse?> DisminuirConsignacion(int consignacionId, int cantidadADisminuir);
        Task<PagedResult<GetConsignacionResponse>> GetConsignaciones(ConsignacionFilters filters);
    }
}

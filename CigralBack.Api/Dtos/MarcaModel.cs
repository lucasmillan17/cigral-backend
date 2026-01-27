using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CigralBackend.Application.Dtos
{
    public record MarcaRequest(
        [Required]
        string Nombre
    );

    public record MarcaResponse(
        int Id,
        string Nombre
    );
}

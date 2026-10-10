using System.ComponentModel.DataAnnotations;
using IAjudinhaApi.Models;

namespace IAjudinhaApi.DTOs;

public class StatusUpdateDto
{
    [Required(ErrorMessage = "Informe o novo status.")]
    public StatusChamado? Status { get; set; }
}

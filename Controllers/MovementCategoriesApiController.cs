using Microsoft.AspNetCore.Mvc;
using midasMVC.Data;
using midasMVC.Models;

namespace midasMVC.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class MovementCategoriesApiController : ControllerBase
{
    private readonly MovementCategoryRepository _movementCategoryRepository;

    public MovementCategoriesApiController(MovementCategoryRepository movementCategoryRepository)
    {
        _movementCategoryRepository = movementCategoryRepository;
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var categories = await _movementCategoryRepository.GetMovementCategoriesByUserIdAsync(userId);

        if (categories == null || !categories.Any())
        {
            return NotFound(new { message = $"No se encontraron categorías para el usuario {userId}." });
        }

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MovementCategory movementCategory)
    {
        if (!ModelState.IsValid || movementCategory.User_id <= 0)
        {
            return BadRequest(new { message = "Los datos de la categoría son inválidos o falta el User_id." });
        }

        await _movementCategoryRepository.CreateMovementCategoryAsync(movementCategory);

        return StatusCode(201, new { message = "Categoría creada con éxito." });
    }

    // PUT: api/movementcategoriesapi/5
    [HttpPut("{categorieId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)] 
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Edit(int categorieId, [FromBody] MovementCategory movementCategory)
    {
        if (!ModelState.IsValid || categorieId <= 0)
        {
            return BadRequest(ModelState);
        }

        await _movementCategoryRepository.UpdateMovementCategoryAsync(categorieId, movementCategory);

        return NoContent();
    }

    [HttpDelete("{categorieId}")]
    public async Task<IActionResult> Delete(int categorieId)
    {
        if (categorieId <= 0)
        {
            return BadRequest(new { message = "Identificador de categoría inválido." });
        }

        await _movementCategoryRepository.DeleteMovementCategoryAsync(categorieId);

        return Ok(new { message = $"Categoría {categorieId} eliminada con éxito." });
    }
}
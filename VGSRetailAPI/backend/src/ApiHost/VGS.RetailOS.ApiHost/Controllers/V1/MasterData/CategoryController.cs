using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.MasterData.Requests;
using VGS.RetailOS.Contracts.V1.MasterData.Responses;
using VGS.RetailOS.Modules.MasterData.Category.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.MasterData;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryBL _categoryBl;

    public CategoryController(ICategoryBL categoryBl)
    {
        _categoryBl = categoryBl;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
    {
        var result = await _categoryBl.GetAllCategoriesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryBl.GetCategoryByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryBl.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryBl.UpdateCategoryAsync(id, request, cancellationToken);
        return Ok(result);
    }
}

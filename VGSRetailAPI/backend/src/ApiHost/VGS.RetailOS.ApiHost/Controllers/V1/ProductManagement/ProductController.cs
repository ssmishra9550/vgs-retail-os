using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.ProductManagement.Requests;
using VGS.RetailOS.Contracts.V1.ProductManagement.Responses;
using VGS.RetailOS.Modules.ProductManagement.Product.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.ProductManagement;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductBL _productBl;

    public ProductController(IProductBL productBl)
    {
        _productBl = productBl;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProducts(CancellationToken cancellationToken)
    {
        var result = await _productBl.GetAllProductsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productBl.GetProductByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productBl.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productBl.UpdateProductAsync(id, request, cancellationToken);
        return Ok(result);
    }
}

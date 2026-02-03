using AssetManagement.Shared.AssetTypes.Dtos;
using AssetManagement.Application.AssetTypes;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Api.Controllers;

[ApiController]
[Route("asset-types")]
[Produces("application/json")]
public sealed class AssetTypeController : ControllerBase
{
    private readonly AssetTypeService _service;

    public AssetTypeController(AssetTypeService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<AssetTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetTypeResponse>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(AssetTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetTypeResponse>> GetById(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AssetTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetTypeResponse>> Create([FromBody] CreateAssetTypeRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AssetTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetTypeResponse>> Update(long id, [FromBody] UpdateAssetTypeRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}

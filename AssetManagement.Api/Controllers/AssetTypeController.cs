using AssetManagement.Application.AssetTypes.Dtos;
using AssetManagement.Application.AssetTypes;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Api.Controllers;

[ApiController]
[Route("asset-types")]
public sealed class AssetTypesController : ControllerBase
{
    private readonly AssetTypeService _service;

    public AssetTypesController(AssetTypeService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<AssetTypeResponse>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AssetTypeResponse>> GetById(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<AssetTypeResponse>> Create([FromBody] CreateAssetTypeRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<AssetTypeResponse>> Update(long id, [FromBody] UpdateAssetTypeRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
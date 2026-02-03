using AssetManagement.Shared.Assets.Dtos;
using AssetManagement.Application.Assets;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Api.Controllers;

[ApiController]
[Route("assets")]
[Produces("application/json")]
public sealed class AssetsController : ControllerBase
{
    private readonly AssetService _service;

    public AssetsController(AssetService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AssetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetResponse>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("by-user/{userId:long}")]
    [ProducesResponseType(typeof(List<AssetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetResponse>>> GetAllByAllocatedUserId(long userId, CancellationToken ct)
        => Ok(await _service.GetAllByAllocatedUserIdAsync(userId, ct));

    [HttpGet("by-serial/{assetSerial}")]
    [ProducesResponseType(typeof(List<AssetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetResponse>>> GetByAssetSerial(string assetSerial, CancellationToken ct)
        => Ok(await _service.GetByAssetSerialAsync(assetSerial, ct));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetResponse>> GetById(long id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetResponse>> Create([FromBody] CreateAssetRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetResponse>> Update(long id, [FromBody] UpdateAssetRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpPost("{id:long}/allocate")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Allocate(long id, [FromBody] AllocateAssetRequest request, CancellationToken ct)
    {
        await _service.AllocateAsync(
            assetId: id,
            userId: request.UserId,
            ct: ct);

        return NoContent();
    }

    [HttpPost("{id:long}/return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Return(long id, CancellationToken ct)
    {
        await _service.ReturnAsync(
            assetId: id,
            ct: ct);

        return NoContent();
    }

    [HttpPost("{id:long}/maintenance")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkMaintenance(long id, CancellationToken ct)
    {
        await _service.MarkMaintenanceAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:long}/maintenance/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteMaintenance(long id, CancellationToken ct)
    {
        await _service.CompleteMaintenanceAsync(id, ct);
        return NoContent();
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(List<AssetAllocationLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetAllocationLogResponse>>> GetAllHistory(CancellationToken ct)
    {
        return Ok(await _service.GetHistoryAsync(null, null, ct));
    }

    [HttpGet("{id:long}/history")]
    [ProducesResponseType(typeof(List<AssetAllocationLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AssetAllocationLogResponse>>> GetAssetHistory(long id, CancellationToken ct)
    {
        return Ok(await _service.GetHistoryAsync(id, null, ct));
    }
}

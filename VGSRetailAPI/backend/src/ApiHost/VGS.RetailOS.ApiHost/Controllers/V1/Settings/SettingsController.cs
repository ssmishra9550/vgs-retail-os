using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGS.RetailOS.Contracts.V1.Settings.Requests;
using VGS.RetailOS.Contracts.V1.Settings.Responses;
using VGS.RetailOS.Modules.Settings.Setting.IBL;

namespace VGS.RetailOS.ApiHost.Controllers.V1.Settings;

[ApiController]
[Route("api/v1/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingBL _settingBl;

    public SettingsController(ISettingBL settingBl)
    {
        _settingBl = settingBl;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SettingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSettings([FromQuery] Guid? storeId, CancellationToken cancellationToken)
    {
        var result = await _settingBl.GetAllSettingsAsync(storeId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{key}")]
    [ProducesResponseType(typeof(SettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSetting(string key, [FromQuery] Guid? storeId, CancellationToken cancellationToken)
    {
        var result = await _settingBl.GetSettingAsync(key, storeId, cancellationToken);
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SettingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertSetting([FromBody] UpsertSettingRequest request, CancellationToken cancellationToken)
    {
        var result = await _settingBl.UpsertSettingAsync(request, cancellationToken);
        return Ok(result);
    }
}

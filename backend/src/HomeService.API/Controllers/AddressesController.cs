using HomeService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeService.API.Controllers;

[Authorize]
public class AddressesController : BaseApiController
{
    /// <summary>
    /// Get all saved addresses for the authenticated user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var query = new Application.Queries.Address.GetUserAddressesQuery
        {
            UserId = userId
        };

        var result = await Mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Create a new address
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Address.CreateAddressCommand
        {
            UserId = userId,
            Label = request.Label,
            FullAddress = request.FullAddress,
            BuildingNumber = request.BuildingNumber,
            Street = request.Street,
            District = request.District,
            City = request.City,
            Region = request.Region,
            PostalCode = request.PostalCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = request.IsDefault,
            AdditionalInfo = request.AdditionalInfo
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetAddresses), new { }, result);
    }

    /// <summary>
    /// Update an existing address
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Address.UpdateAddressCommand
        {
            AddressId = id,
            UserId = userId,
            Label = request.Label,
            FullAddress = request.FullAddress,
            BuildingNumber = request.BuildingNumber,
            Street = request.Street,
            District = request.District,
            City = request.City,
            Region = request.Region,
            PostalCode = request.PostalCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = request.IsDefault,
            AdditionalInfo = request.AdditionalInfo
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete an address
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());

        var command = new Application.Commands.Address.DeleteAddressCommand
        {
            AddressId = id,
            UserId = userId
        };

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

// Request DTOs
public record CreateAddressRequest(
    string Label,
    string FullAddress,
    string? BuildingNumber,
    string? Street,
    string? District,
    string City,
    Region Region,
    string? PostalCode,
    double Latitude,
    double Longitude,
    bool IsDefault,
    string? AdditionalInfo);

public record UpdateAddressRequest(
    string? Label,
    string? FullAddress,
    string? BuildingNumber,
    string? Street,
    string? District,
    string? City,
    Region? Region,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    bool? IsDefault,
    string? AdditionalInfo);

using Hospital.Catalog.Api.Dtos;
using Hospital.Catalog.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Catalog.Api.Controllers;

/// <summary>
/// Manages doctors in the hospital catalog.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly DoctorService _doctorService;

    public DoctorsController(DoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>
    /// Gets all doctors from the catalog. Results are cached in Redis.
    /// </summary>
    /// <returns>List of doctors.</returns>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(List<DoctorResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    /// <summary>
    /// Gets a doctor by identifier.
    /// </summary>
    /// <param name="id">Doctor identifier.</param>
    /// <returns>Doctor details.</returns>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DoctorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);

        if (doctor is null)
        {
            return NotFound();
        }

        return Ok(doctor);
    }

    /// <summary>
    /// Creates a doctor. Requires the Admin role.
    /// </summary>
    /// <param name="dto">Doctor creation data.</param>
    /// <returns>Created doctor.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(DoctorResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(CreateDoctorDto dto)
    {
        try
        {
            var doctor = await _doctorService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing doctor. Requires the Admin role.
    /// </summary>
    /// <param name="id">Doctor identifier.</param>
    /// <param name="dto">Updated doctor data.</param>
    /// <returns>No content when the update succeeds.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateDoctorDto dto)
    {
        try
        {
            var updated = await _doctorService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a doctor. Requires the Admin role.
    /// </summary>
    /// <param name="id">Doctor identifier.</param>
    /// <returns>No content when the deletion succeeds.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _doctorService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Filters doctors by specialization, department, availability and search text.
    /// </summary>
    /// <param name="specialization">Optional specialization filter.</param>
    /// <param name="department">Optional department filter.</param>
    /// <param name="available">Optional availability filter.</param>
    /// <param name="search">Optional search text over name, specialization and department.</param>
    /// <param name="sortBy">Optional sort field: fullName, specialization, department or availability.</param>
    /// <param name="descending">Sort direction.</param>
    /// <returns>Filtered list of doctors.</returns>
    [AllowAnonymous]
    [HttpGet("filter")]
    [ProducesResponseType(typeof(List<DoctorResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter(
        string? specialization,
        string? department,
        bool? available,
        string? search,
        string? sortBy,
        bool descending = false)
    {
        var doctors = await _doctorService.FilterAsync(
            specialization,
            department,
            available,
            search,
            sortBy,
            descending);

        return Ok(doctors);
    }
}

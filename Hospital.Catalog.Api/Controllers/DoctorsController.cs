using Hospital.Catalog.Api.Dtos;
using Hospital.Catalog.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly DoctorService _doctorService;

    public DoctorsController(DoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);

        if (doctor == null)
        {
            return NotFound();
        }

        return Ok(doctor);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDoctorDto dto)
    {
        var doctor = await _doctorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateDoctorDto dto)
    {
        var updated = await _doctorService.UpdateAsync(id, dto);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _doctorService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("filter")]
    public async Task<IActionResult> Filter(string? specialization, bool? available)
    {
        var doctors = await _doctorService.FilterAsync(specialization, available);
        return Ok(doctors);
    }
}
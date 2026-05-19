using System.Text.Json;
using Hospital.Catalog.Api.Data;
using Hospital.Catalog.Api.Dtos;
using Hospital.Catalog.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Hospital.Catalog.Api.Services;

public class DoctorService
{
    private readonly CatalogDbContext _context;
    private readonly IDistributedCache _cache;

    private const string DoctorsCacheKey = "catalog:doctors:all";

    public DoctorService(CatalogDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<DoctorResponseDto>> GetAllAsync()
    {
        var cachedDoctors = await _cache.GetStringAsync(DoctorsCacheKey);

        if (!string.IsNullOrEmpty(cachedDoctors))
        {
            return JsonSerializer.Deserialize<List<DoctorResponseDto>>(cachedDoctors) ?? new List<DoctorResponseDto>();
        }

        var doctors = await _context.Doctors
            .Select(doctor => ToResponseDto(doctor))
            .ToListAsync();

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(
            DoctorsCacheKey,
            JsonSerializer.Serialize(doctors),
            cacheOptions);

        return doctors;
    }

    public async Task<DoctorResponseDto?> GetByIdAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor == null)
        {
            return null;
        }

        return ToResponseDto(doctor);
    }

    public async Task<DoctorResponseDto> CreateAsync(CreateDoctorDto dto)
    {
        var doctor = new Doctor
        {
            FullName = dto.FullName,
            Specialization = dto.Specialization,
            Department = dto.Department,
            IsAvailable = true
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        await ClearDoctorsCacheAsync();

        return ToResponseDto(doctor);
    }

    public async Task<bool> UpdateAsync(int id, UpdateDoctorDto dto)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor == null)
        {
            return false;
        }

        doctor.FullName = dto.FullName;
        doctor.Specialization = dto.Specialization;
        doctor.Department = dto.Department;
        doctor.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync();

        await ClearDoctorsCacheAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor == null)
        {
            return false;
        }

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();

        await ClearDoctorsCacheAsync();

        return true;
    }

    public async Task<List<DoctorResponseDto>> FilterAsync(string? specialization, bool? available)
    {
        var query = _context.Doctors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            query = query.Where(d => d.Specialization.ToLower() == specialization.ToLower());
        }

        if (available.HasValue)
        {
            query = query.Where(d => d.IsAvailable == available.Value);
        }

        return await query
            .Select(doctor => ToResponseDto(doctor))
            .ToListAsync();
    }

    private async Task ClearDoctorsCacheAsync()
    {
        await _cache.RemoveAsync(DoctorsCacheKey);
    }

    private static DoctorResponseDto ToResponseDto(Doctor doctor)
    {
        return new DoctorResponseDto
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            Specialization = doctor.Specialization,
            Department = doctor.Department,
            IsAvailable = doctor.IsAvailable
        };
    }
}
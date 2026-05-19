using System.Text.Json;
using Hospital.Catalog.Api.Data;
using Hospital.Catalog.Api.Dtos;
using Hospital.Catalog.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Hospital.Catalog.Api.Services;

public class DoctorService
{
    private const string DoctorsCacheKey = "catalog:doctors:all";

    private readonly CatalogDbContext _context;
    private readonly IDistributedCache _cache;

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
            .AsNoTracking()
            .OrderBy(doctor => doctor.FullName)
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
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return doctor is null ? null : ToResponseDto(doctor);
    }

    public async Task<DoctorResponseDto> CreateAsync(CreateDoctorDto dto)
    {
        EnsureText(dto.FullName, "Doctor full name is required.");
        EnsureText(dto.Specialization, "Doctor specialization is required.");
        EnsureText(dto.Department, "Doctor department is required.");

        var doctor = new Doctor
        {
            FullName = dto.FullName.Trim(),
            Specialization = dto.Specialization.Trim(),
            Department = dto.Department.Trim(),
            IsAvailable = true
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        await ClearDoctorsCacheAsync();

        return ToResponseDto(doctor);
    }

    public async Task<bool> UpdateAsync(int id, UpdateDoctorDto dto)
    {
        EnsureText(dto.FullName, "Doctor full name is required.");
        EnsureText(dto.Specialization, "Doctor specialization is required.");
        EnsureText(dto.Department, "Doctor department is required.");

        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor is null)
        {
            return false;
        }

        doctor.FullName = dto.FullName.Trim();
        doctor.Specialization = dto.Specialization.Trim();
        doctor.Department = dto.Department.Trim();
        doctor.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync();
        await ClearDoctorsCacheAsync();

        return true;
    }

    public async Task<bool> SetAvailabilityAsync(int id, bool isAvailable)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor is null)
        {
            return false;
        }

        doctor.IsAvailable = isAvailable;

        await _context.SaveChangesAsync();
        await ClearDoctorsCacheAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor is null)
        {
            return false;
        }

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();
        await ClearDoctorsCacheAsync();

        return true;
    }

    public async Task<List<DoctorResponseDto>> FilterAsync(
        string? specialization,
        string? department,
        bool? available,
        string? search,
        string? sortBy,
        bool descending)
    {
        var query = _context.Doctors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            var normalizedSpecialization = specialization.Trim().ToLower();
            query = query.Where(doctor => doctor.Specialization.ToLower().Contains(normalizedSpecialization));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var normalizedDepartment = department.Trim().ToLower();
            query = query.Where(doctor => doctor.Department.ToLower().Contains(normalizedDepartment));
        }

        if (available.HasValue)
        {
            query = query.Where(doctor => doctor.IsAvailable == available.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(doctor =>
                doctor.FullName.ToLower().Contains(normalizedSearch)
                || doctor.Specialization.ToLower().Contains(normalizedSearch)
                || doctor.Department.ToLower().Contains(normalizedSearch));
        }

        query = (sortBy?.Trim().ToLower()) switch
        {
            "specialization" => descending
                ? query.OrderByDescending(doctor => doctor.Specialization).ThenBy(doctor => doctor.FullName)
                : query.OrderBy(doctor => doctor.Specialization).ThenBy(doctor => doctor.FullName),
            "department" => descending
                ? query.OrderByDescending(doctor => doctor.Department).ThenBy(doctor => doctor.FullName)
                : query.OrderBy(doctor => doctor.Department).ThenBy(doctor => doctor.FullName),
            "availability" => descending
                ? query.OrderByDescending(doctor => doctor.IsAvailable).ThenBy(doctor => doctor.FullName)
                : query.OrderBy(doctor => doctor.IsAvailable).ThenBy(doctor => doctor.FullName),
            _ => descending
                ? query.OrderByDescending(doctor => doctor.FullName)
                : query.OrderBy(doctor => doctor.FullName)
        };

        return await query
            .Select(doctor => ToResponseDto(doctor))
            .ToListAsync();
    }

    private async Task ClearDoctorsCacheAsync()
    {
        await _cache.RemoveAsync(DoctorsCacheKey);
    }

    private static void EnsureText(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }
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

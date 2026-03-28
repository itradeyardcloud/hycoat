using AutoMapper;
using AutoMapper.QueryableExtensions;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Masters;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public CustomerService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<CustomerDto>> GetAllAsync(string? search, int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                (c.ShortName != null && c.ShortName.ToLower().Contains(term)) ||
                (c.City != null && c.City.ToLower().Contains(term)) ||
                (c.GSTIN != null && c.GSTIN.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "shortname" => sortDesc ? query.OrderByDescending(c => c.ShortName) : query.OrderBy(c => c.ShortName),
            "city" => sortDesc ? query.OrderByDescending(c => c.City) : query.OrderBy(c => c.City),
            "createdat" => sortDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResponse<CustomerDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CustomerDetailDto> GetByIdAsync(int id)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .Include(c => c.Inquiries)
            .Include(c => c.WorkOrders)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");

        return _mapper.Map<CustomerDetailDto>(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, string userId)
    {
        var customer = _mapper.Map<Customer>(dto);
        customer.CreatedBy = userId;

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto, string userId)
    {
        var customer = await _db.Customers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");

        _mapper.Map(dto, customer);
        customer.UpdatedBy = userId;
        customer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var customer = await _db.Customers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Customer with ID {id} not found.");

        customer.IsDeleted = true;
        customer.UpdatedBy = userId;
        customer.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<LookupDto>> GetLookupAsync()
    {
        return await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ProjectTo<LookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}

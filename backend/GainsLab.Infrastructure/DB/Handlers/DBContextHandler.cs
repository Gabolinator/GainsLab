using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using GainsLab.Models.DataManagement.DB.Model.Handlers;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

public abstract class IdbContextHandler<TDto> : IDBHandler where TDto : class, IDto 
{
    protected IdbContextHandler(DbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public abstract DbSet<TDto> DBSet { get; }
    
    protected DbContext _context;
    protected readonly ILogger _logger;

    public abstract Task<Result<TDto>> TryGetExistingDTO(Guid guid);
   
    public async Task<Result<TDto>> TryGetExistingDTO(int id)
    {
        var existing = await DBSet
            .FirstOrDefaultAsync(e => e.Iid == id);
        var success = existing != null;
        return success ?Result<TDto>.SuccessResult(existing!) :  Result<TDto>.Failure("No existing dto found");
    }


    public async Task<Result<IDto>> AddAsync(TDto dto)
    {
        try
        {
            await DBSet.AddAsync(dto);
            await _context.SaveChangesAsync();
            await _context.Entry(dto).ReloadAsync();
            _logger.Log("DbContextHandler", $"After SaveChanges: {dto.Iid}");
            _logger.Log("DbContextHandler", $"Entity State: {_context.Entry(dto).State}");
            _logger.Log("DbContextHandler",$"Added {dto.Iguid} to db with id {dto.Iid}");
            if(dto.Iid <=0)  _logger.LogWarning("DbContextHandler",$"Added negative id to db : {dto.Iguid} with id {dto.Iid}");
            return Result<IDto>.SuccessResult(dto);

        }
        catch (Exception e)
        {
            _logger.LogError("DbContextHandler", $"Failed to Insert DTO {dto.Iguid}: {e.Message}");
            return Result<IDto>.Failure($"Database error while inserting : {e.Message}");
        }
        
      
    }

    public async Task<Result<IDto>> UpdateAsync(TDto dto)
    {
        try
        {
            DBSet.Update(dto);
            await _context.SaveChangesAsync();
            _logger.Log("DbContextHandler",$"Updated {dto.Iguid} in db");
            if(dto.Iid <=0)  _logger.LogWarning("DbContextHandler",$"Updated negative id to db : {dto.Iguid} with id {dto.Iid}");
            return Result<IDto>.SuccessResult(dto);
        }
        catch (Exception e)
        {
            _logger.LogError("DbContextHandler", $"Failed to update DTO {dto.Iguid}: {e.Message}");
            return Result<IDto>.Failure($"Database error while updating: {e.Message}");
        }
      
    }


    public async Task<Result<IDto>> AddOrUpdateAsync(IDto dto)
    {
        _logger.Log("DbContextHandler",$"Trying to add or update dto {dto.Iguid}");

        
        if (dto is not TDto tdto)
        {
            _logger.LogWarning("DbContextHandler",$"Cant add or update dto {dto.Iguid} - wrong type");
            return Result<IDto>.Failure("Invalid Dto type");
        }

        var result =  await TryGetExistingDTO(tdto.Iguid);

        if (result.Success)
        {
            var existingDto = result.Value; 
            
            //we need to update the dto 
            if(NeedUpdate(existingDto , tdto)) return await UpdateAsync(tdto);
            
            //we dont need to update it 
            _logger.Log("DbContextHandler",$"Dont need to Update dto {dto.Iguid} - already up to date");
            
            return Result<IDto>.SuccessResult(existingDto!);
        }
        
        return await AddAsync(tdto);
        
       
    }

    private bool NeedUpdate(TDto? existingDto, TDto newDto)
    {
        
        
        if (existingDto == null) return true;

        
        return existingDto.Equals(newDto);

    }

    public async Task<Result<IDto>> AddAsync(IDto dto)
    {
        if (dto is not TDto tdto) return Result<IDto>.Failure("Invalid Dto type");
        return await AddAsync(tdto);
    }

    public async Task<Result<IDto>> UpdateAsync(IDto dto)
    {
        if (dto is not TDto tdto) return Result<IDto>.Failure("Invalid Dto type");;
       return await UpdateAsync(tdto);
    }
    
}
using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class PlanOptionRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory)
{
    //Get plan by id
    public async Task<PlanOptionEntity> GetPlanOptionById(int id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var planOption = await context.PlanOptions.FirstOrDefaultAsync(x => x.Id == id);
        if (planOption is null) throw new NullReferenceException("Plan option does not exist");
        return planOption;
    }


    public async Task<List<PlanOptionEntity>> GetAllPlanOptions()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var planOptions = await context.PlanOptions.ToListAsync();
        if (planOptions is null) throw new NullReferenceException("Plan options do not exist");
        return planOptions;
    }

    public async Task<int> AddPlanOption(PlanOptionEntity planOption)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        context.PlanOptions.Add(planOption);
        await context.SaveChangesAsync();
        return planOption.Id;
    }

    // Update plan option
    public async Task UpdatePlanOption(PlanOptionEntity planOption)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        context.PlanOptions.Update(planOption);
        await context.SaveChangesAsync();
    }

    public async Task DeletePlanOption(int id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var planOption = await context.PlanOptions.FirstOrDefaultAsync(x => x.Id == id);
        if (planOption is null) throw new NullReferenceException("Plan option does not exist");
        context.PlanOptions.Remove(planOption);
        await context.SaveChangesAsync();
    }
}
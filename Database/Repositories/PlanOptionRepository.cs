using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class PlanOptionRepository(
    IDbContextFactory<GlobalEntryTrackerDbContext> contextFactory)
{
    public async Task<List<PlanOptionEntity>> GetAllPlanOptions()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var planOptions = await context.PlanOptions.ToListAsync();
        if (planOptions is null) throw new NullReferenceException("Plan options do not exist");
        return planOptions;
    }
}
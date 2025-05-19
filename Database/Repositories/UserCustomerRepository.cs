using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Database.Repositories;

public class UserCustomerRepository(
    GlobalEntryTrackerDbContext context,
    ILogger<UserCustomerRepository> logger)
{
    public async Task<UserCustomerEntity?> GetCustomerDetailsForUser(int userId)
    {
        var userCustomer = await context.UserCustomers.FirstOrDefaultAsync(x => x.UserId == userId);
        return userCustomer;
    }

    public async Task AddEditUserCustomer(UserCustomerEntity userCustomer)
    {
        try
        {
            var existingUserCustomer = await context.UserCustomers
                .FirstOrDefaultAsync(x => x.UserId == userCustomer.UserId);
            if (existingUserCustomer != null)
            {
                existingUserCustomer.CustomerId = userCustomer.CustomerId;
                existingUserCustomer.SubscriptionId = userCustomer.SubscriptionId;
            }
            else
            {
                await context.UserCustomers.AddAsync(userCustomer);
            }

            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex.Message);
            throw new DbUpdateException("Failed to add/edit user customer", ex);
        }
    }
}
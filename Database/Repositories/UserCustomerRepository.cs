using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories;

public class UserCustomerRepository(GlobalEntryTrackerDbContext context)
{
    public async Task<UserCustomerEntity?> GetCustomerDetailsForUser(int userId)
    {
        var userCustomer = await context.UserCustomers.FirstOrDefaultAsync(x => x.UserId == userId);
        return userCustomer;
    }

    public async Task AddUpdateUserCustomer(UserCustomerEntity userCustomer)
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
}
using Askify.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class ConsultationSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ConsultationSeeder(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            // Skip if already seeded
            if (await _context.Consultations.AnyAsync())
            {
                return;
            }

            // Get regular users and experts
            var regularUsers = await _userManager.GetUsersInRoleAsync("User");
            var experts = await _userManager.GetUsersInRoleAsync("Expert");
            
            // Safe-check: convert to lists to avoid multiple enumeration
            var usersList = regularUsers.ToList();
            var expertsList = experts.ToList();
            
            // Safety check - if no users or experts, skip seeding
            if (!usersList.Any() || !expertsList.Any())
            {
                return;
            }
            
            var random = new Random();
            var statuses = new[] { "Pending", "Accepted", "Completed", "Cancelled" };
            
            for (int i = 0; i < 10; i++)
            {
                // Safety checks before Random selection
                if (usersList.Count == 0 || expertsList.Count == 0) break;
                
                var user = usersList[random.Next(usersList.Count)];
                var expert = expertsList[random.Next(expertsList.Count)];
                var status = statuses[random.Next(statuses.Length)];
                
                var consultation = new Consultation
                {
                    UserId = user.Id,
                    ExpertId = expert.Id,
                    IsFree = i < 3, // First 3 are free
                    IsPaid = i >= 3, // Rest are paid
                    IsOpenRequest = false,
                    IsPublicable = random.Next(2) == 1, // 50% chance of being publicable
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                };
                
                // Add timestamps based on status
                if (status != "Pending")
                {
                    consultation.AnsweredAt = consultation.CreatedAt.AddHours(random.Next(1, 24));
                    
                    if (status == "Completed")
                    {
                        consultation.CompletedAt = consultation.AnsweredAt?.AddHours(random.Next(1, 48));
                    }
                }
                
                _context.Consultations.Add(consultation);
            }
            
            await _context.SaveChangesAsync();
        }
    }
}

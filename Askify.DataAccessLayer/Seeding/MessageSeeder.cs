using Askify.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Askify.DataAccessLayer.Seeding
{
    public class MessageSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public MessageSeeder(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            // Check if messages already exist
            if (await _context.Messages.AnyAsync())
            {
                return;
            }

            Console.WriteLine("Starting to seed messages...");

            try
            {
                // Get actual consultations from the database with their related users
                var consultations = await _context.Consultations
                    .Where(c => c.Status == "Accepted" || c.Status == "Completed")
                    .Where(c => c.UserId != null && c.ExpertId != null) // Make sure both user and expert exist
                    .ToListAsync();

                if (!consultations.Any())
                {
                    Console.WriteLine("No valid consultations found for message seeding.");
                    return;
                }

                Console.WriteLine($"Found {consultations.Count} valid consultations for message seeding.");

                var random = new Random();
                var messageStatuses = new[] { "Sent", "Delivered", "Read" };
                int messagesCreated = 0;

                // For each consultation, create messages
                foreach (var consultation in consultations)
                {
                    // Double-check that both user and expert exist in the Users table
                    var user = await _context.Users.FindAsync(consultation.UserId);
                    var expert = await _context.Users.FindAsync(consultation.ExpertId);

                    if (user == null || expert == null)
                    {
                        Console.WriteLine($"Skipping consultation {consultation.Id} due to missing user or expert");
                        continue;
                    }

                    // Create between 3-10 messages for this consultation
                    var messageCount = random.Next(3, 10);
                    
                    for (int i = 0; i < messageCount; i++)
                    {
                        // Alternate between user and expert for messages
                        var isFromUser = i % 2 == 0;
                        var senderId = isFromUser ? user.Id : expert.Id;
                        var receiverId = isFromUser ? expert.Id : user.Id;
                        
                        // Calculate message timestamp
                        var baseTime = consultation.AnsweredAt ?? consultation.CreatedAt;
                        var messageTime = baseTime.AddMinutes(random.Next(1, 30) * (i + 1));
                        
                        // Don't create messages after completion
                        if (consultation.CompletedAt.HasValue && messageTime > consultation.CompletedAt)
                        {
                            continue;
                        }
                        
                        var message = new Message
                        {
                            SenderId = senderId,
                            ReceiverId = receiverId,
                            Text = $"Sample message {i+1} in consultation {consultation.Id}",
                            Status = messageStatuses[random.Next(messageStatuses.Length)],
                            SentAt = messageTime,
                            ConsultationId = consultation.Id // Explicit setting of ConsultationId
                        };
                        
                        _context.Messages.Add(message);
                        messagesCreated++;
                        
                        // Save in smaller batches to avoid large transactions
                        if (messagesCreated % 20 == 0)
                        {
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                
                // Final save for any remaining messages
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Successfully seeded {messagesCreated} messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MessageSeeder: {ex.Message}");
                Console.WriteLine(ex.InnerException?.Message ?? "No inner exception");
                throw; // Re-throw to allow higher-level error handling
            }
        }
    }
}

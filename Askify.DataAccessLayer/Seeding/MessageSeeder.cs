using Askify.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Askify.DataAccessLayer.Seeding
{
    public class MessageSeeder
    {
        public async Task SeedAsync(DataAccessLayer.Interfaces.IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            // Check if there are already messages in the database
            var existingMessages = await unitOfWork.Messages.FindAsync(m => true);
            if (existingMessages.Any())
            {
                return; // Messages already seeded
            }

            // Get some users to create messages between them
            var users = userManager.Users.Take(3).ToList();
            if (users.Count < 2)
            {
                // Need at least 2 users to create messages
                return;
            }

            var messages = new List<Message>();

            // Create some sample messages between users
            for (int i = 0; i < 5; i++)
            {
                var sender = users[i % 2]; // Alternate between first two users
                var receiver = users[(i + 1) % 2];

                messages.Add(new Message
                {
                    SenderId = sender.Id,
                    ReceiverId = receiver.Id,
                    Text = $"This is test message {i + 1} from seeding",
                    Status = i % 2 == 0 ? "Read" : "Sent",
                    SentAt = DateTime.UtcNow.AddHours(-i),
                });
            }

            foreach (var message in messages)
            {
                await unitOfWork.Messages.AddAsync(message);
            }
            
            await unitOfWork.CompleteAsync();
        }
    }
}

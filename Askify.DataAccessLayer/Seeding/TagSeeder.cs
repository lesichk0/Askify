using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Askify.DataAccessLayer.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Askify.DataAccessLayer.Seeding
{
    public class TagSeeder
    {
        private readonly AppDbContext _context;

        public TagSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Tags.AnyAsync()) return;

            var faker = new Faker("uk");

            var tags = new List<Tag>();

            for (int i = 0; i < 6; i++)
            {
                tags.Add(new Tag
                {
                    Name = faker.Lorem.Word() + i // щоб не було повних дублікатів
                });
            }

            await _context.Tags.AddRangeAsync(tags);
            await _context.SaveChangesAsync();
        }
    }
}

using Microsoft.AspNetCore.Identity;
using HotelManagementSystem2.Models;

namespace HotelManagementSystem2.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Create roles - Admin and Guest only
            string[] roleNames = { "Admin", "Guest" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user
            var adminEmail = "admin@hotel.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed sample rooms with Unsplash images
            if (!context.Rooms.Any())
            {
                var rooms = new List<Room>
                {
                    new Room
                    {
                        RoomNumber = "101",
                        RoomType = RoomType.Single,
                        PricePerNight = 1500.00M,
                        Capacity = 1,
                        Description = "Cozy single room with basic amenities",
                        Amenities = "WiFi, Air Conditioning, TV, Hot Shower",
                        ImageUrl = "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=800&q=80",
                        Status = RoomStatus.Available
                    },
                    new Room
                    {
                        RoomNumber = "102",
                        RoomType = RoomType.Single,
                        PricePerNight = 1500.00M,
                        Capacity = 1,
                        Description = "Cozy single room with basic amenities",
                        Amenities = "WiFi, Air Conditioning, TV, Hot Shower",
                        ImageUrl = "https://images.unsplash.com/photo-1598928506311-c55ded91a20c?w=800&q=80",
                        Status = RoomStatus.Available
                    },
                    new Room
                    {
                        RoomNumber = "201",
                        RoomType = RoomType.Double,
                        PricePerNight = 2500.00M,
                        Capacity = 2,
                        Description = "Comfortable double room for couples",
                        Amenities = "WiFi, Air Conditioning, TV, Hot Shower, Mini Fridge",
                        ImageUrl = "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800&q=80",
                        Status = RoomStatus.Available
                    },
                    new Room
                    {
                        RoomNumber = "202",
                        RoomType = RoomType.Double,
                        PricePerNight = 2500.00M,
                        Capacity = 2,
                        Description = "Comfortable double room for couples",
                        Amenities = "WiFi, Air Conditioning, TV, Hot Shower, Mini Fridge",
                        ImageUrl = "https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800&q=80",
                        Status = RoomStatus.Available
                    },
                    new Room
                    {
                        RoomNumber = "301",
                        RoomType = RoomType.Suite,
                        PricePerNight = 4500.00M,
                        Capacity = 3,
                        Description = "Luxurious suite with living area",
                        Amenities = "WiFi, Air Conditioning, Smart TV, Hot Shower, Mini Fridge, Coffee Maker, Balcony",
                        ImageUrl = "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&q=80",
                        Status = RoomStatus.Available
                    },
                    new Room
                    {
                        RoomNumber = "302",
                        RoomType = RoomType.Deluxe,
                        PricePerNight = 5500.00M,
                        Capacity = 2,
                        Description = "Premium deluxe room with city view",
                        Amenities = "WiFi, Air Conditioning, Smart TV, Hot Shower, Mini Bar, Coffee Maker, Balcony, Bathtub",
                        ImageUrl = "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800&q=80",
                        Status = RoomStatus.Available
                    },
                    new Room
                    {
                        RoomNumber = "401",
                        RoomType = RoomType.Family,
                        PricePerNight = 6000.00M,
                        Capacity = 5,
                        Description = "Spacious family room with multiple beds",
                        Amenities = "WiFi, Air Conditioning, Smart TV, Hot Shower, Mini Fridge, Coffee Maker, Dining Area",
                        ImageUrl = "https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=800&q=80",
                        Status = RoomStatus.Available
                    }
                };

                await context.Rooms.AddRangeAsync(rooms);
                await context.SaveChangesAsync();
            }
        }
    }
}

using System.Diagnostics;
using HotelManagementSystem2.Models;
using HotelManagementSystem2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get all available rooms to display on home page
            var rooms = await _context.Rooms
                .Where(r => r.IsActive && r.Status == RoomStatus.Available)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
            ViewBag.AvailableRooms = rooms;
            
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalRooms = await _context.Rooms.CountAsync(r => r.IsActive);
            ViewBag.AvailableRooms = await _context.Rooms.CountAsync(r => r.IsActive && r.Status == RoomStatus.Available);
            ViewBag.OccupiedRooms = await _context.Rooms.CountAsync(r => r.Status == RoomStatus.Occupied);
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();
            ViewBag.ActiveBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.CheckedIn);
            ViewBag.TodayCheckIns = await _context.Bookings.CountAsync(b => b.CheckInDate.Date == DateTime.Today);
            ViewBag.TodayCheckOuts = await _context.Bookings.CountAsync(b => b.CheckOutDate.Date == DateTime.Today);
            
            var recentBookings = await _context.Bookings
                .Include(b => b.Room)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.RecentBookings = recentBookings;
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

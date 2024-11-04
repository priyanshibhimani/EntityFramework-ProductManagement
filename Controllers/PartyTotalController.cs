using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagemet.Context;
using ProductManagemet.Models;
using System.Threading.Tasks;

namespace ProductManagement.Controllers
{
    public class PartyTotalController : Controller
    {
        private readonly AppDbContext _context;

        public PartyTotalController(AppDbContext context)
        {
            _context = context;
        }
        #region Index
       
        [HttpGet]
        [Route("partytotal/Index")]
        public async Task<IActionResult> Index(string sortOrder = null, string searchTerm = null)
        {
            var partyTotalsQuery = _context.PartyTotal
                .Include(pt => pt.Party) // Include related Party data if necessary
                .AsQueryable();

            // Implement searching by party name if a search term is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                partyTotalsQuery = partyTotalsQuery.Where(pt => pt.Party.PartyName.Contains(searchTerm));
            }

            // Implement sorting
            switch (sortOrder)
            {
                case "name_desc":
                    partyTotalsQuery = partyTotalsQuery.OrderByDescending(pt => pt.Party.PartyName);
                    break;
                case "name_asc":
                    partyTotalsQuery = partyTotalsQuery.OrderBy(pt => pt.Party.PartyName);
                    break;
                default:
                    break;
            }

            var partyTotals = await partyTotalsQuery.ToListAsync();

            ViewBag.CurrentSort = sortOrder; // Keep track of the current sort order
            ViewBag.CurrentSearch = searchTerm; // Keep track of the current search term

            return View(partyTotals);
        }


        #endregion

    }
}




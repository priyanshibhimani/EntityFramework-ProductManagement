using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagemet.Context;
using ProductManagemet.Models;

namespace ProductManagemet.Controllers
{
    public class PartyController : Controller
    {
        private readonly AppDbContext _context;

        public PartyController(AppDbContext context)
        {
            _context = context;
        }
        #region Get
        [Route("Party")]
        [HttpGet]
        public async Task<IActionResult> Party(string sortOrder = null, string searchTerm = null)
        {
            var partiesQuery = _context.Parties.AsQueryable();

            // Apply search if there's a search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                partiesQuery = partiesQuery.Where(p => p.PartyName.Contains(searchTerm));
            }

            // Apply sorting based on the sortOrder parameter
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                partiesQuery = sortOrder switch
                {
                    "name_desc" => partiesQuery.OrderByDescending(p => p.PartyName),
                    _ => partiesQuery.OrderBy(p => p.PartyName), // Default sorting by Party Name ascending
                };
            }

            var parties = await partiesQuery.ToListAsync();

            ViewBag.CurrentSort = sortOrder; // Keep track of the current sort order
            ViewBag.CurrentFilter = searchTerm; // Keep track of the current search term

            return View(parties);
        }
        public async Task<IActionResult> Details(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null)
            {
                return NotFound(); 
            }
            return View(party);
        }
        #endregion
        #region Insert
        [HttpGet]
        [Route("Party/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Party/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Party party)
        {
            if (ModelState.IsValid)
            {
                _context.Parties.Add(party);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(party);
        }
        #endregion
        #region Edit
        [HttpGet]
        [Route("Party/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null)
            {
                return NotFound(); 
            }
            return View(party);
        }
   
        [HttpPost]
        [Route("Party/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Party party)
        {
            if (id != party.PartyId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(party);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartyExists(party.PartyId))
                    {
                        return NotFound();
                    }
                    throw; 
                }
                return RedirectToAction("Party"); 
            }
            return View(party); 
        }


        private bool PartyExists(int id)
        {
            return _context.Parties.Any(e => e.PartyId == id);
        }

        #endregion
        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null)
            {
                return NotFound(); 
            }
            return View(party); 
        }

   
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party != null)
            {
                _context.Parties.Remove(party);
                await _context.SaveChangesAsync(); 
            }
            return RedirectToAction("Party"); 
        }
        #endregion
    }
}

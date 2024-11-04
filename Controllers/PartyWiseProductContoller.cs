using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagemet.Context;
using ProductManagemet.Models;

namespace ProductManagemet.Controllers
{
    [Route("partywiseproduct")]
    public class PartyWiseProductController : Controller
    {
        private readonly AppDbContext _context;

        public PartyWiseProductController(AppDbContext context)
        {
            _context = context;
        }

        #region create
        // GET: partywiseproduct/create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewBag.Parties = _context.Parties.ToList();
            ViewBag.Products = _context.Products.ToList();
            return View();
        }
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PartyId,ProductId")] PartyWiseProduct partyWiseProduct)
        {
            if (ModelState.IsValid)
            {
                // Fetch the product to get the rate
                var product = await _context.Products.FindAsync(partyWiseProduct.ProductId);
               

                _context.Add(partyWiseProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); 
            }

            ViewBag.Parties = await _context.Parties.ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            return View(partyWiseProduct);
        }
        #endregion

        #region Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var partyWiseProducts = await _context.PartyWiseProducts
                .Include(pwp => pwp.Product)  // Include related Product
                .Include(pwp => pwp.Party)     // Include related Party
                .ToListAsync();

            return View(partyWiseProducts); // Pass the list to the view
        }
        #endregion

        #region Delete
        // GET: partywiseproduct/delete/5
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var partyWiseProduct = await _context.PartyWiseProducts
                .Include(pwp => pwp.Product)
                .Include(pwp => pwp.Party)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (partyWiseProduct == null)
            {
                return NotFound(); 
            }

            return View(partyWiseProduct); 
        }

        // POST: partywiseproduct/delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
    

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var partyWiseProduct = await _context.PartyWiseProducts.FindAsync(id);
            if (partyWiseProduct != null)
            {
                _context.PartyWiseProducts.Remove(partyWiseProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index)); 
        }
        #endregion

        #region Edit
        // GET: PartyWiseProduct/Edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var partyWiseProduct = await _context.PartyWiseProducts
                .Include(pwp => pwp.Product) 
                .Include(pwp => pwp.Party)    
                .FirstOrDefaultAsync(m => m.Id == id);

            if (partyWiseProduct == null)
            {
                return NotFound(); 
            }

            ViewBag.Parties = await _context.Parties.ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            return View(partyWiseProduct); 
        }

        // POST: PartyWiseProduct/Edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PartyId,ProductId,ProductRate")] PartyWiseProduct partyWiseProduct)
        {
            if (id != partyWiseProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FindAsync(partyWiseProduct.ProductId);
                   
                    _context.Update(partyWiseProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartyWiseProductExists(partyWiseProduct.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; 
                    }
                }
                return RedirectToAction(nameof(Index)); 
            }

            ViewBag.Parties = await _context.Parties.ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            return View(partyWiseProduct);
        }

        // Helper method to check if a PartyWiseProduct exists
        private bool PartyWiseProductExists(int id)
        {
            return _context.PartyWiseProducts.Any(e => e.Id == id);
        }
        #endregion


    }
}

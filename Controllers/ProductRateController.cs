using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagemet.Context;
using ProductManagemet.Models;

namespace ProductManagemet.Controllers
{
    [Route("productRate")]
    public class ProductRateController : Controller
    {
        
        private readonly AppDbContext _context;

        public ProductRateController(AppDbContext context)
        {
            _context = context;
        }

        #region Index
        public async Task<IActionResult> Index()
        {
            var productrates = await _context.ProductRates
             .Include(pwp => pwp.Product)    
             .ToListAsync();

            return View(productrates); 
            
        }
        #endregion

    }
}

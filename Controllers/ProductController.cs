using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagemet.Context;
using ProductManagemet.Models;

namespace ProductManagemet.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
      
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        #region Index
        public async Task<IActionResult> Index(string sortOrder = null, string searchTerm = null)
        {
            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(searchTerm));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.ProductName);
                    break;
                case "name_asc":
                    productsQuery = productsQuery.OrderBy(p => p.ProductName);
                    break;
                default:
                    break;
            }

            var products = await productsQuery.ToListAsync();

            ViewBag.CurrentSort = sortOrder; // Keep track of the current sort order
            ViewBag.CurrentSearch = searchTerm; // Keep track of the current search term

            return View(products);
        }
        #endregion
        #region Details

        [Route("Product/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        #endregion
        #region Create
        [HttpGet]
        [Route("Product/Create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Route("Product/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // Set CreatedAt and UpdatedAt fields
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(product);
        }
        #endregion
        #region Edit
        [HttpGet]
        [Route("Product/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [HttpPost]
        [Route("Product/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Create a new ProductRate entry with the old rate
                var productRate = new ProductRate
                {
                    ProductId = existingProduct.ProductId,
                    NewRate = existingProduct.ProductRate,
                    UpdatedDate = DateTime.Now

                };

                // Add the new product rate to the ProductRate table
                _context.ProductRates.Add(productRate);
                existingProduct.ProductName = product.ProductName;
                existingProduct.ProductDescription = product.ProductDescription;
                existingProduct.ProductRate = product.ProductRate; // Set the new rate
                existingProduct.UpdatedAt = DateTime.Now; // Update the modified date

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(product);
        }



        private bool PartyExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        #endregion
        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        #endregion



    }
}

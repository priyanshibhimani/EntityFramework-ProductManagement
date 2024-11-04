using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ProductManagemet.Context;
using ProductManagemet.Models;
using System.Reflection.Metadata;
using System.Threading.Tasks;


namespace ProductManagemet.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _context;

        public InvoiceController(AppDbContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // GET: Invoices
        [HttpGet]
        [Route("invoice")]
 
        #region Index
        public async Task<IActionResult> Index(int? partyId)
        {
            ViewBag.PartyId = partyId;

            var invoices = await _context.Invoices
                .Include(i => i.Parties)
                .Include(i => i.Product)
                .Where(i => !partyId.HasValue || i.PartyId == partyId)  
                .ToListAsync();
            var totalAmount = invoices.Sum(i => (i.Product?.ProductRate ?? 0) * i.Quantity);
            ViewBag.TotalAmount = totalAmount;
            return View(invoices);
        }
   
        // GET: invoice/index
        public async Task<IActionResult> Index(int partyId)
        {
            var invoices = await _context.Invoices
                .Where(i => i.PartyId == partyId)
                .Include(i => i.Product)
                .ToListAsync();
            var totalAmount = invoices.Sum(i => i.TotalAmount);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.PartyId = partyId;
            return View(invoices);
        }
        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int partyId)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            var products = await _context.Products.ToListAsync();

            ViewBag.Products = new SelectList(products, "ProductId", "ProductName");
            ViewBag.PartyId = partyId;

            return View(invoice);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                _context.Update(invoice);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { partyId = invoice.PartyId });
            }

            ViewBag.PartyId = invoice.PartyId;
            return View(invoice);
        }

        // Helper method to check if an invoice exists
        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }
        #endregion

        #region Generate Invoice
        [HttpPost]
        [Route("invoice/generatetotal/{partyId}")]

        public async Task<IActionResult> GenerateTotal(int partyId)
        {
            var invoices = await _context.Invoices
                .Where(i => i.PartyId == partyId)
                .Include(i => i.Product)
                .ToListAsync();

            if (invoices.Any())
            {
                // Calculate total amount and total number of products
                var totalAmount = invoices.Sum(i => i.TotalAmount);
                var totalProducts = invoices.Count; // Total distinct products


                // Copy each Invoice record to InvoiceEntry before deletion
                foreach (var invoice in invoices)
                {
                    var invoiceEntry = new InvoiceEntry
                    {
                        PartyId = invoice.PartyId,
                        InvoiceDate = invoice.InvoiceDate,
                        Quantity = invoice.Quantity,
                        ProductId = invoice.ProductId,
                        TotalAmount = invoice.TotalAmount
                    };
                    _context.InvoiceEntry.Add(invoiceEntry);
                }

                // Delete the original invoices
                _context.Invoices.RemoveRange(invoices);
                await _context.SaveChangesAsync();

                // Create a new PartyTotal record
                var partyTotal = new PartyTotal
                {
                    PartyId = partyId,
                    TotalAmount = totalAmount,
                    TotalProducts = totalProducts,

                };

                _context.PartyTotal.Add(partyTotal);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "PartyTotal");
        }
        #endregion

        #region Create
        [HttpGet]
        public async Task<IActionResult> Create(int partyId)
        {
            // Fetch all products assigned to the party
            var assignedProducts = await _context.PartyWiseProducts
                .Where(pwp => pwp.PartyId == partyId)
                .Include(pwp => pwp.Product) 
                .ToListAsync();

            var invoice = new Invoice
            {
                PartyId = partyId,
                ProductId = null, 
                Quantity = 1 
            };

            ViewBag.Products = assignedProducts.Select(pwp => pwp.Product).ToList();

            return View(invoice);
        }

        // POST: invoice/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(invoice.ProductId);
                if (product != null)
                {
                    invoice.TotalAmount = invoice.Quantity * product.ProductRate; 
                }

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { partyId = invoice.PartyId });
            }

            ViewBag.Products = await _context.Products.ToListAsync(); 
            return View(invoice);
        }
        #endregion

        #region Excel
        public async Task<IActionResult> DownloadExcel(int partyId)
        {
            var invoices = await _context.Invoices
                .Where(pwp => pwp.PartyId == partyId)
                .Include(i => i.Product)
                .ToListAsync();

            // Create an Excel package
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Invoices");
                worksheet.Cells[1, 1].Value = "Sr.No";
                worksheet.Cells[1, 2].Value = "Product Name";
                worksheet.Cells[1, 3].Value = "Product Rate";
                worksheet.Cells[1, 4].Value = "Quantity";
                worksheet.Cells[1, 5].Value = "Total Amount";


                int row = 2;
                int column = 1;
                foreach (var invoice in invoices)
                {
                    worksheet.Cells[row, 1].Value = column;
                    worksheet.Cells[row, 2].Value = invoice.Product?.ProductName;
                    worksheet.Cells[row, 3].Value = invoice.Product?.ProductRate;
                    worksheet.Cells[row, 4].Value = invoice.Quantity;
                    worksheet.Cells[row, 5].Value = invoice.TotalAmount;
                    row++;
                    column++;
                }

                var excelData = package.GetAsByteArray();
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Invoices.xlsx");
            }
        }
        #endregion

        #region Delete
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id, int partyId)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }

            // Redirect to Index with PartyId
            return RedirectToAction("Index", new { partyId });
        }
        #endregion

    }
}

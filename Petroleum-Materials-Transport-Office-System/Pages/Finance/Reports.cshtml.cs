using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Petroleum_Materials_Transport_Office_System.Data;
using Petroleum_Materials_Transport_Office_System.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Petroleum_Materials_Transport_Office_System.Pages.Finance
{
    public class ReportsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReportsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ReportInputModel Input { get; set; }

        // Dropdown Lists
        public SelectList FuelOptions { get; set; }
        public SelectList CustomerOptions { get; set; }
        public SelectList ProviderOptions { get; set; }
        public SelectList LocationOptions { get; set; }

        public void OnGet()
        {
            PopulateDropdowns();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return Page();
            }

            return RedirectToPage("ReportViewer", new
            {
                reportType = Input.ReportType,
                fromDate = Input.FromDate?.ToString("yyyy-MM-dd"),
                toDate = Input.ToDate?.ToString("yyyy-MM-dd"),
                fuelId = Input.FuelId,
                customerId = Input.CustomerId,
                providerId = Input.ProviderId,
                locationCode = Input.LocationCode
            });
        }

        private void PopulateDropdowns()
        {
            // Now these lines will work because we updated the DbContext
            var fuels = _context.Fuel_Type
                .Where(x => x.Status == "Active")
                .Select(x => new { x.Fuel_ID, x.Type_Name })
                .ToList();
            FuelOptions = new SelectList(fuels, "Fuel_ID", "Type_Name");

            var customers = _context.Company
                .Where(x => x.Status == "Active")
                .Select(x => new { x.Company_ID, x.Company_Name })
                .ToList();
            CustomerOptions = new SelectList(customers, "Company_ID", "Company_Name");

            var providers = _context.Provider
                .Where(x => x.Status == "Active")
                .Select(x => new { x.Provider_ID, x.Provider_Name })
                .ToList();
            ProviderOptions = new SelectList(providers, "Provider_ID", "Provider_Name");

            var locations = _context.Location
                .Where(x => x.Status == "Active")
                .Select(x => new { x.Location_Code, x.Location_Name })
                .ToList();
            LocationOptions = new SelectList(locations, "Location_Code", "Location_Name");
        }

        public class ReportInputModel
        {
            [Required(ErrorMessage = "يرجى اختيار نوع التقرير")]
            public string ReportType { get; set; }

            [DataType(DataType.Date)]
            public DateTime? FromDate { get; set; }

            [DataType(DataType.Date)]
            public DateTime? ToDate { get; set; }

            public int? FuelId { get; set; }
            public int? CustomerId { get; set; }
            public int? ProviderId { get; set; }
            public string LocationCode { get; set; }
        }
    }
}
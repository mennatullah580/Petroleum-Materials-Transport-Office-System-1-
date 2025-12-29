using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Petroleum_Materials_Transport_Office_System.Data;
using System;
using System.Collections.Generic;
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
            // Initialize default dates
            Input = new InputModel
            {
                FromDate = DateTime.Today.AddDays(-30),
                ToDate = DateTime.Today
            };
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList FuelOptions { get; set; }
        public SelectList CustomerOptions { get; set; }
        public SelectList ProviderOptions { get; set; }
        public SelectList LocationOptions { get; set; }

        public void OnGet()
        {
            LoadDropdowns();
        }

        public IActionResult OnPost()
        {
            // Always reload dropdowns first, in case we have to return to this page (if there is an error)
            LoadDropdowns();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // SUCCESS: Redirect to the ReportViewer page
            // We pass the user's choices as "Query String" parameters
            return RedirectToPage("ReportViewer", new
            {
                reportType = Input.ReportType,
                // Format dates safely for the URL
                fromDate = Input.FromDate.ToString("yyyy-MM-dd"),
                toDate = Input.ToDate.ToString("yyyy-MM-dd"),

                // Pass optional IDs
                fuelId = Input.FuelId,
                customerId = Input.CustomerId,
                providerId = Input.ProviderId,
                locationCode = Input.LocationCode
            });
        }

        private void LoadDropdowns()
        {
            // Fuel (Fuel_ID is int)
            FuelOptions = new SelectList(_context.Fuel_Type.ToList(), "Fuel_ID", "Type_Name");

            // Customers (Company_ID is int)
            CustomerOptions = new SelectList(_context.Company.ToList(), "Company_ID", "Company_Name");

            // Providers (Provider_ID is int)
            ProviderOptions = new SelectList(_context.Provider.ToList(), "Provider_ID", "Provider_Name");

            // Locations (Location_Code is STRING)
            LocationOptions = new SelectList(_context.Location.ToList(), "Location_Code", "Location_Name");
        }

        public class InputModel
        {
            [Required(ErrorMessage = "يرجى اختيار نوع التقرير")]
            public string ReportType { get; set; }

            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }

            public int? FuelId { get; set; }
            public int? CustomerId { get; set; }
            public int? ProviderId { get; set; }

            // Nullable string so the field is not mandatory
            public string? LocationCode { get; set; }
        }
    }
}
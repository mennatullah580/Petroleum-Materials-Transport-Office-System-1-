using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // Required for .Include()
using Petroleum_Materials_Transport_Office_System.Data;
using Petroleum_Materials_Transport_Office_System.Models;
using System;
using System.Data;
using System.Linq;

namespace Petroleum_Materials_Transport_Office_System.Pages.Finance
{
    public class ReportViewerModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReportViewerModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string ReportTitle { get; set; }
        public DataTable ReportData { get; set; }
        public decimal TotalSum { get; set; }

        public void OnGet(string reportType, DateTime? fromDate, DateTime? toDate, string customerName, string providerName)
        {
            ReportData = new DataTable();
            TotalSum = 0;

            // 1. Join Tables using .Include
            var query = _context.Invoice
                .Include(i => i.Company)   // Join with Company table
                .Include(i => i.Provider)  // Join with Provider table
                .AsQueryable();

            // 2. Filter Logic
            if (fromDate.HasValue)
            {
                // Note: The property is now correctly mapped to 'Issue_Date' in the DB
                query = query.Where(x => x.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.Date <= toDate.Value);
            }

            // Filter by Customer Name (Searching inside the related Company table)
            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(x => x.Company.Company_Name.Contains(customerName));
            }

            // Filter by Provider Name
            if (!string.IsNullOrEmpty(providerName))
            {
                query = query.Where(x => x.Provider.Provider_Name.Contains(providerName));
            }

            // 3. Generate Report
            switch (reportType)
            {
                case "details":
                default:
                    ReportTitle = "تقرير الفواتير التفصيلي";
                    GenerateDetailsReport(query);
                    break;
            }
        }

        private void GenerateDetailsReport(IQueryable<Invoice> query)
        {
            // Define Columns
            ReportData.Columns.Add("رقم الفاتورة");
            ReportData.Columns.Add("التاريخ");
            ReportData.Columns.Add("العميل");
            ReportData.Columns.Add("المقاول");
            ReportData.Columns.Add("الحالة");
            ReportData.Columns.Add("الصافي (EGP)");

            var result = query.ToList();

            foreach (var item in result)
            {
                // Safely handle nulls using '?'
                string custName = item.Company != null ? item.Company.Company_Name : "غير معروف";
                string provName = item.Provider != null ? item.Provider.Provider_Name : "-";

                ReportData.Rows.Add(
                    item.InvoiceNumber,
                    item.Date.ToString("yyyy-MM-dd"),
                    custName,
                    provName,
                    item.Status,
                    item.Net.ToString("N2")
                );
            }

            TotalSum = result.Sum(x => x.Net);
        }
    }
}
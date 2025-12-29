using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Petroleum_Materials_Transport_Office_System.Data;
using Petroleum_Materials_Transport_Office_System.Models;
using System;
using System.Collections.Generic;
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
        public List<ReportItem> ReportData { get; set; } = new List<ReportItem>();
        public decimal TotalAmount { get; set; }
        public string FromDateStr { get; set; }
        public string ToDateStr { get; set; }

        public void OnGet(string reportType, string fromDate, string toDate, int? fuelId, int? customerId, int? providerId, string? locationCode)
        {
            if (string.IsNullOrEmpty(fromDate)) fromDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
            if (string.IsNullOrEmpty(toDate)) toDate = DateTime.Today.ToString("yyyy-MM-dd");

            FromDateStr = fromDate;
            ToDateStr = toDate;

            DateTime start = DateTime.Parse(fromDate);
            DateTime end = DateTime.Parse(toDate).AddDays(1).AddSeconds(-1); // Includes the full end day

            if (reportType == "orders_detailed")
            {
                LoadOrdersReport(start, end, fuelId, customerId, providerId, locationCode);
            }
            else if (reportType == "debts" || reportType == "invoices_list")
            {
                LoadInvoicesReport(start, end, customerId, reportType);
            }
            else
            {
                ReportTitle = "تقرير غير محدد";
            }
        }

        private void LoadOrdersReport(DateTime start, DateTime end, int? fuelId, int? customerId, int? providerId, string? locationCode)
        {
            ReportTitle = "تفاصيل النقل والماليات";

            var query = _context.Financials
                .Include(f => f.Order)
                    .ThenInclude(o => o.Provider)
                .Include(f => f.Order)
                    .ThenInclude(o => o.FuelType)
                .Where(f => f.Order.OrderDate >= start && f.Order.OrderDate <= end);

            if (fuelId.HasValue) query = query.Where(f => f.Order.PetroleumTypeId == fuelId.Value);
            if (providerId.HasValue) query = query.Where(f => f.Order.ProviderId == providerId.Value);
            if (!string.IsNullOrEmpty(locationCode))
                query = query.Where(f => f.Order.LoadingLocation == locationCode || f.Order.UnloadingLocation == locationCode);

            var list = query.ToList();

            ReportData = list.Select(item => new ReportItem
            {
                Id = item.Order_ID.ToString(),
                // FIX: Pass the DateTime object directly, NOT a string
                Date = item.Order?.OrderDate ?? DateTime.MinValue,
                Name = item.Order?.Provider?.Provider_Name ?? "غير محدد",
                Type = (item.Order?.FuelType?.Type_Name ?? "وقود") + " (" + (item.Order?.LoadingQuantity.ToString() ?? "0") + " طن)",
                Amount = item.Company_Total,
                Notes = item.Order?.Status ?? "-"
            }).ToList();

            TotalAmount = ReportData.Sum(x => x.Amount);
        }

        private void LoadInvoicesReport(DateTime start, DateTime end, int? customerId, string type)
        {
            ReportTitle = (type == "debts") ? "ديون العملاء" : "قائمة الفواتير";

            var query = _context.Invoice
                .Include(i => i.Company)
                .Where(i => i.Date >= start && i.Date <= end);

            if (customerId.HasValue) query = query.Where(i => i.CompanyId == customerId.Value);

            var list = query.ToList();

            ReportData = list.Select(i => new ReportItem
            {
                Id = i.InvoiceNumber,
                // FIX: Pass the DateTime object directly
                Date = i.Date,
                Name = i.Company?.Company_Name ?? "-",
                Type = "فاتورة بيع",
                Amount = i.CompanyAmount,
                Notes = i.Status
            }).ToList();

            TotalAmount = ReportData.Sum(x => x.Amount);
        }

        // --- THE MOST IMPORTANT FIX ---
        // Change 'Date' from 'string' to 'DateTime'
        public class ReportItem
        {
            public string Id { get; set; }
            public DateTime Date { get; set; } // This allows .ToString("yyyy-MM-dd") in HTML
            public string Name { get; set; }
            public string Type { get; set; }
            public decimal Amount { get; set; }
            public string Notes { get; set; }
        }
    }
}
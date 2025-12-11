using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Petroleum_Materials_Transport_Office_System.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.OrdersManagement
{
    public class IndexModel : PageModel
    {
        // قائمة الطلبات (مؤقتة - لحد ما نربط بـ Database)
        public List<Order> Orders { get; set; } = new List<Order>();

        // الفلاتر
        [BindProperty(SupportsGet = true)]
        public string SearchInvoiceNumber { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchLoadingLocation { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchUnloadingLocation { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchProviderName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchPetroleumType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? SearchDateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? SearchDateTo { get; set; }

        // بيانات الطلب الجديد
        [BindProperty]
        public Order NewOrder { get; set; }

        // القوائم المنسدلة
        public List<string> LoadingLocations { get; set; } = new List<string>
        {
            "مستودع القاهرة",
            "مستودع الإسكندرية",
            "مستودع أسيوط",
            "مستودع الغردقة"
        };

        public List<string> UnloadingLocations { get; set; } = new List<string>
        {
            "محطة المعادي",
            "محطة مدينة نصر",
            "محطة الجيزة",
            "محطة الشيخ زايد"
        };

        public List<string> PetroleumTypes { get; set; } = new List<string>
        {
            "بنزين 80",
            "بنزين 92",
            "بنزين 95",
            "سولار"
        };

        public List<string> Providers { get; set; } = new List<string>
        {
            "شركة النقل الأولى",
            "شركة النقل السريع",
            "شركة المواصلات الحديثة"
        };

        public List<string> Vehicles { get; set; } = new List<string>
        {
            "أ س ب 1234",
            "ل م ن 5678",
            "ق ر ش 9012"
        };

        public void OnGet()
        {
            // بيانات تجريبية
            Orders = new List<Order>
            {
                new Order
                {
                    OrderId = 1,
                    InvoiceNumber = "INV-2024-001",
                    OrderDate = DateTime.Now.AddDays(-5),
                    LoadingLocation = "مستودع القاهرة",
                    UnloadingLocation = "محطة المعادي",
                    PetroleumType = "بنزين 92",
                    LoadingQuantity = 10000,
                    UnloadingQuantity = 9950,
                    Shortage = 50,
                    ProviderName = "شركة النقل الأولى",
                    VehiclePlateNumber = "أ س ب 1234",
                    CompanyPrice = 12.5m,
                    ProviderPrice = 10.0m,
                    CompanyTotal = 124375,
                    ProviderTotal = 99500,
                    NetAmount = 95000,
                    Balance = 4500,
                    Status = "مكتمل",
                    DeliveryDate = DateTime.Now.AddDays(-2)
                },
                new Order
                {
                    OrderId = 2,
                    InvoiceNumber = "INV-2024-002",
                    OrderDate = DateTime.Now.AddDays(-3),
                    LoadingLocation = "مستودع الإسكندرية",
                    UnloadingLocation = "محطة مدينة نصر",
                    PetroleumType = "سولار",
                    LoadingQuantity = 15000,
                    UnloadingQuantity = 14900,
                    Shortage = 100,
                    ProviderName = "شركة النقل السريع",
                    VehiclePlateNumber = "ل م ن 5678",
                    CompanyPrice = 11.0m,
                    ProviderPrice = 9.0m,
                    CompanyTotal = 163900,
                    ProviderTotal = 134100,
                    NetAmount = 130000,
                    Balance = 4100,
                    Status = "قيد التنفيذ",
                    DeliveryDate = DateTime.Now.AddDays(1)
                }
            };

            // تطبيق الفلاتر
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (!string.IsNullOrEmpty(SearchInvoiceNumber))
                Orders = Orders.Where(o => o.InvoiceNumber.Contains(SearchInvoiceNumber)).ToList();

            if (!string.IsNullOrEmpty(SearchLoadingLocation))
                Orders = Orders.Where(o => o.LoadingLocation == SearchLoadingLocation).ToList();

            if (!string.IsNullOrEmpty(SearchUnloadingLocation))
                Orders = Orders.Where(o => o.UnloadingLocation == SearchUnloadingLocation).ToList();

            if (!string.IsNullOrEmpty(SearchProviderName))
                Orders = Orders.Where(o => o.ProviderName == SearchProviderName).ToList();

            if (!string.IsNullOrEmpty(SearchPetroleumType))
                Orders = Orders.Where(o => o.PetroleumType == SearchPetroleumType).ToList();

            if (!string.IsNullOrEmpty(SearchStatus))
                Orders = Orders.Where(o => o.Status == SearchStatus).ToList();

            if (SearchDateFrom.HasValue)
                Orders = Orders.Where(o => o.OrderDate >= SearchDateFrom.Value).ToList();

            if (SearchDateTo.HasValue)
                Orders = Orders.Where(o => o.OrderDate <= SearchDateTo.Value).ToList();
        }

        public IActionResult OnPostCreateOrder()
        {
            if (NewOrder == null)
                return Page();

            // حساب العجز تلقائياً
            NewOrder.Shortage = NewOrder.LoadingQuantity - NewOrder.UnloadingQuantity;

            // حساب المبالغ تلقائياً (أمثلة)
            NewOrder.CompanyTotal = NewOrder.UnloadingQuantity * NewOrder.CompanyPrice;
            NewOrder.ProviderTotal = NewOrder.UnloadingQuantity * NewOrder.ProviderPrice;

            // حساب الخصومات
            NewOrder.TaxAmount = NewOrder.ProviderTotal * 0.05m; // 5% ضريبة مثلاً
            NewOrder.StampFee = 50; // رسم دمغة ثابت
            NewOrder.TotalDeductions = NewOrder.TaxAmount + NewOrder.StampFee + NewOrder.GPSFee;

            // حساب الصافي والمتبقي
            NewOrder.NetAmount = NewOrder.ProviderTotal - NewOrder.TotalDeductions - NewOrder.AdvancePayment - NewOrder.CustodyAmount;
            NewOrder.Balance = NewOrder.NetAmount;

            // إضافة الطلب (مؤقت - لحد ما نربط بـ Database)
            // في الواقع هتحفظ في Database

            TempData["Success"] = "تم إنشاء الطلب بنجاح";
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteOrder(int orderId)
        {
            // حذف الطلب من Database
            TempData["Success"] = "تم حذف الطلب بنجاح";
            return RedirectToPage();
        }
    }
}
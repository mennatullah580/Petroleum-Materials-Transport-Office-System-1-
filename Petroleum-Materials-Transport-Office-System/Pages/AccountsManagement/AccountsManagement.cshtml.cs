using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

// UPDATED NAMESPACE TO MATCH YOUR PROJECT
namespace Petroleum_Materials_Transport_Office_System.Pages.Accounts
{
    public class AccountsManagementModel : PageModel
    {
        public string ActiveTab { get; set; } = "customers";
        public string SearchText { get; set; }

        public List<AccountRecord> Customers = new List<AccountRecord>();
        public List<AccountRecord> Suppliers = new List<AccountRecord>();
        public List<AccountRecord> DisplayedList { get; set; } = new List<AccountRecord>();

        public AccountsManagementModel()
        {
            // Dummy Data
            Customers.Add(new AccountRecord { ID = 1, CompanyName = "التوحيد والنور", Address = "الحي الاول", ContactInfo = "111", Balance = 200 });
            Customers.Add(new AccountRecord { ID = 2, CompanyName = "الايمان والمستقبل", Address = "الحي الثاني", ContactInfo = "222", Balance = 400 });

            Suppliers.Add(new AccountRecord { ID = 10, CompanyName = "احمد", Address = "شارع 10", ContactInfo = "555", Balance = 900 });
            Suppliers.Add(new AccountRecord { ID = 11, CompanyName = "محمد", Address = "شارع 20", ContactInfo = "666", Balance = 300 });
        }

        public void OnGet(string tab, string search)
        {
            if (!string.IsNullOrEmpty(tab)) ActiveTab = tab;
            SearchText = search;

            var targetList = (ActiveTab == "customers") ? Customers : Suppliers;
            DisplayedList = FilterList(targetList);
        }

        private List<AccountRecord> FilterList(List<AccountRecord> list)
        {
            if (string.IsNullOrEmpty(SearchText)) return list;

            var result = new List<AccountRecord>();
            foreach (var rec in list)
            {
                if (rec.CompanyName != null &&
                    rec.CompanyName.ToLower().Contains(SearchText.ToLower()))
                {
                    result.Add(rec);
                }
            }
            return result;
        }
    }

    // Class definition included here for safety
    public class AccountRecord
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string ContactInfo { get; set; }
        public decimal Balance { get; set; }
    }
}
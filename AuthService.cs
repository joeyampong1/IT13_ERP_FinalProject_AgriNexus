namespace AgriNexus.Services
{
    public enum UserRole
    {
        SuperAdmin,
        TenantAdmin,
        BranchManager,
        InventoryOfficer,
        LaborOfficer,
        SalesOfficer,
        Accountant
    }

    public class AuthService
    {
        public UserRole CurrentRole { get; private set; } = UserRole.SuperAdmin;
        public string CurrentUserName { get; private set; } = "Super Admin";
        public string AccessLevel { get; private set; } = "Platform-wide access";
        public bool IsAuthenticated { get; private set; } = true;

        public event Action? OnChange;

        public void Logout()
        {
            CurrentRole = UserRole.SuperAdmin;
            CurrentUserName = "Guest";
            AccessLevel = "None";
            IsAuthenticated = false;
            NotifyStateChanged();
        }

        public void Login(string email)
        {
            // 🛡️ Safety net: Kung walay gi-type nga email, i-default sa Super Admin
            if (string.IsNullOrWhiteSpace(email))
            {
                email = "admin@agrinexus.com";
            }

            // Mock logic base sa email nga i-type
            if (email.Contains("owner")) { CurrentRole = UserRole.TenantAdmin; CurrentUserName = "Business Owner / Tenant Admin"; AccessLevel = "Business-wide access"; }
            else if (email.Contains("branch")) { CurrentRole = UserRole.BranchManager; CurrentUserName = "Branch Manager"; AccessLevel = "Assigned branch access"; }
            else if (email.Contains("inventory")) { CurrentRole = UserRole.InventoryOfficer; CurrentUserName = "Inventory Officer"; AccessLevel = "Assigned branch access"; }
            else if (email.Contains("labor")) { CurrentRole = UserRole.LaborOfficer; CurrentUserName = "Labor & Harvest Officer"; AccessLevel = "Assigned branch access"; }
            else if (email.Contains("sales")) { CurrentRole = UserRole.SalesOfficer; CurrentUserName = "Sales & Cashier Officer"; AccessLevel = "Assigned branch access"; }
            else if (email.Contains("finance")) { CurrentRole = UserRole.Accountant; CurrentUserName = "Accountant / Finance Officer"; AccessLevel = "Business-wide access"; }
            else { CurrentRole = UserRole.SuperAdmin; CurrentUserName = "Super Admin"; AccessLevel = "Platform-wide access"; }

            IsAuthenticated = true;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
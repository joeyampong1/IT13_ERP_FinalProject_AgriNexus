using System;
using AgriNexus.DataLayer.Models; 

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

        // Updated Login method: Now accepts the actual User model from the database
        public void Login(User user)
        {
            CurrentUserName = user.FullName;

            // Convert the string role from the database to the Enum
            if (Enum.TryParse<UserRole>(user.Role, true, out var parsedRole))
            {
                CurrentRole = parsedRole;
            }
            else
            {
                CurrentRole = UserRole.TenantAdmin; // Default fallback
            }

            // Set the access level based on the role
            AccessLevel = CurrentRole switch
            {
                UserRole.SuperAdmin => "Platform-wide access",
                UserRole.TenantAdmin => "Business-wide access",
                UserRole.Accountant => "Business-wide access",
                _ => "Assigned branch access"
            };

            IsAuthenticated = true;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
using System;
using System.Linq;
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

        public void Login(User user)
        {
            // Safely combine name parts, ignoring empty middle names to prevent double spaces
            var nameParts = new[] { user.FName, user.MName, user.LName };
            CurrentUserName = string.Join(" ", nameParts.Where(part => !string.IsNullOrWhiteSpace(part)));

            // Convert the string role from the database to the Enum
            if (Enum.TryParse<UserRole>(user.Role, true, out var parsedRole))
            {
                CurrentRole = parsedRole;
            }
            else
            {
                CurrentRole = UserRole.TenantAdmin; // Default fallback
            }

            // Explicitly assign access levels based on the multi-tenant hierarchy
            AccessLevel = CurrentRole switch
            {
                // Level 1: Platform Level
                UserRole.SuperAdmin => "Platform-wide access",

                // Level 2: Tenant Level (Business-wide)
                UserRole.TenantAdmin => "Business-wide access",
                UserRole.Accountant => "Business-wide access",

                // Level 3 & 4: Branch Level (Assigned branch access)
                UserRole.BranchManager => "Assigned branch access",
                UserRole.InventoryOfficer => "Assigned branch access",
                UserRole.LaborOfficer => "Assigned branch access",
                UserRole.SalesOfficer => "Assigned branch access",

                // Fallback for any unhandled roles
                _ => "Assigned branch access"
            };

            IsAuthenticated = true;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
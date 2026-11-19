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
        public long CurrentUserId { get; private set; }
        public long? CurrentBusinessId { get; private set; }
        public string CurrentUserEmail { get; private set; } = "";
        public UserRole CurrentRole { get; private set; } = UserRole.SuperAdmin;
        public string CurrentUserName { get; private set; } = "Super Admin";
        public string AccessLevel { get; private set; } = "Platform-wide access";
        public bool IsAuthenticated { get; private set; } = true;

        public event Action? OnChange;

        public void Logout()
        {
            CurrentUserId = 0;
            CurrentBusinessId = null;
            CurrentUserEmail = "";
            CurrentRole = UserRole.SuperAdmin;
            CurrentUserName = "Guest";
            AccessLevel = "None";
            IsAuthenticated = false;
            NotifyStateChanged();
        }

        public void Login(User user)
        {
            CurrentUserId = user.UserId;
            CurrentBusinessId = user.BusinessId;
            CurrentUserEmail = user.Email ?? "";

            var nameParts = new[] { user.FName, user.MName, user.LName };
            CurrentUserName = string.Join(" ", nameParts.Where(part => !string.IsNullOrWhiteSpace(part)));

            if (Enum.TryParse<UserRole>(user.Role, true, out var parsedRole))
            {
                CurrentRole = parsedRole;
            }
            else
            {
                CurrentRole = UserRole.TenantAdmin;
            }

            AccessLevel = CurrentRole switch
            {
                UserRole.SuperAdmin => "Platform-wide access",
                UserRole.TenantAdmin => "Business-wide access",
                UserRole.Accountant => "Business-wide access",
                UserRole.BranchManager => "Assigned branch access",
                UserRole.InventoryOfficer => "Assigned branch access",
                UserRole.LaborOfficer => "Assigned branch access",
                UserRole.SalesOfficer => "Assigned branch access",
                _ => "Assigned branch access"
            };

            IsAuthenticated = true;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
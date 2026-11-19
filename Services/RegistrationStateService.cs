using AgriNexus.DataLayer.Models;

namespace AgriNexus.Services
{
    public class RegistrationStateService
    {
        // Step 1: Account & Business Details
        public string BusinessName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public bool AgreeToTerms { get; set; } = false;

        // Step 2: Plan Selection
        public bool HasSelectedPlan { get; set; } = false;
        public bool IsFreeTrial { get; set; } = false;
        public SubscriptionPlan? SelectedPlan { get; set; }

        // Step 3: Payment Details (for Paid Plans)
        public string PaymentMethod { get; set; } = "GCash";
        public string ReferenceNumber { get; set; } = "";
        public string AccountSenderName { get; set; } = "";

        public void Reset()
        {
            BusinessName = "";
            FirstName = "";
            LastName = "";
            Email = "";
            Password = "";
            AgreeToTerms = false;
            HasSelectedPlan = false;
            IsFreeTrial = false;
            SelectedPlan = null;
            PaymentMethod = "GCash";
            ReferenceNumber = "";
            AccountSenderName = "";
        }
    }
}
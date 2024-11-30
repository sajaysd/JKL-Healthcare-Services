using Microsoft.AspNetCore.Http;

namespace JKLHealthcareSystem.Helpers
{
    public class SessionHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsLoggedIn => 
            _httpContextAccessor.HttpContext?.Session.GetString("isLoggedIn") == "true";
    }
}

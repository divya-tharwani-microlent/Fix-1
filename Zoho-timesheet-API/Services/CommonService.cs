using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Zoho_timesheet_API.Models;
using Zoho_timesheet_EFC;

namespace Zoho_timesheet_API.Services
{
    public class CommonService
    {
        private readonly ZohoTimesheetDBContext _context;
        private readonly IConfiguration _configuration;

        public CommonService(ZohoTimesheetDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<UserModel> GetUserDetailsbyID(string zuid)
        {
            var dbuser = _context.Users.Where(u => u.ZUID == zuid).FirstOrDefaultAsync().Result;
            if (dbuser != null)
            {
                return new UserModel()
                {
                    access_token = dbuser.access_token,
                    refresh_token = dbuser.refresh_token
                };
            }
            return null;
        }

        public async Task<string> RefreshToken(string refreshToken, string zuid)
        {
            string clientId = _configuration["Zoho:ClientId"];
            string clientSecret = _configuration["Zoho:ClientSecret"];

            var tokenResponse = await new HttpClient().PostAsync(
                $"{_configuration["Zoho:OAuthBaseUrl"]}/token?grant_type=refresh_token&client_id={clientId}&client_secret={clientSecret}&refresh_token={refreshToken}",
                null
            );

            if (!tokenResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var tokenData = await tokenResponse.Content.ReadAsStringAsync();
            var tokenJson = JObject.Parse(tokenData);

            string newAccessToken = tokenJson["access_token"]?.ToString();

            // Update access token in the database
            await UpdateAccessTokenInDatabase(zuid, newAccessToken);

            return newAccessToken;
        }

        private async Task<bool> UpdateAccessTokenInDatabase(string zuid, string new_access_token)
        {
            var existingUser = await _context.Users.Where(u => u.ZUID == zuid).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                existingUser.access_token = new_access_token;
                existingUser.Createddate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            else return false;
        }
    }
}

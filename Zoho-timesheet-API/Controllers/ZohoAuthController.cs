using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net.Http.Headers;
using Zoho_timesheet_API.Models;
using Zoho_timesheet_Core.Entities;
using Zoho_timesheet_EFC;

[Route("api/zoho")]
[ApiController]
public class ZohoController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ZohoTimesheetDBContext _context;
    private readonly ILogger<ZohoController> _logger;


    public ZohoController(IConfiguration configuration, ZohoTimesheetDBContext context, ILogger<ZohoController> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger=logger;
    }

    [HttpPost("getToken")]
    public async Task<IActionResult> GetToken([FromBody] ZohoAuthRequest data)
    {
        Log.Information("Starting GetToken API...");
        string code = data.Code;
        string clientId = _configuration["Zoho:ClientId"];
        string clientSecret = _configuration["Zoho:ClientSecret"];
        string redirectUri = _configuration["Zoho:RedirectUri"];

        _logger.LogInformation("Fetching access token from Zoho...");

        using var httpClient = new HttpClient();

        // Request access token
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var tokenResponse = await httpClient.PostAsync($"{_configuration["Zoho:OAuthBaseUrl"]}/token", tokenRequest);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return BadRequest("Error retrieving access token.");
        }

        var tokenData = await tokenResponse.Content.ReadAsStringAsync();
        var tokenJson = JObject.Parse(tokenData);
        string accessToken = tokenJson["access_token"]?.ToString();
        string refreshToken = tokenJson["refresh_token"]?.ToString();


        _logger.LogError("Access token retrieved successfully.",accessToken);

        // Fetch user details
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var userResponse = await httpClient.GetAsync(_configuration["Zoho:UserApiUrl"]);

        if (!userResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve user details. Response: {Response}", await userResponse.Content.ReadAsStringAsync());
            return BadRequest("Error retrieving user details.");
        }

        var userData = await userResponse.Content.ReadAsStringAsync();
        var userJson = JObject.Parse(userData);
        var userDetails = JsonConvert.DeserializeObject<UserModel>(userJson.ToString());

        // Save user details to database
        await SaveUserDetails(userDetails, accessToken, refreshToken);
        var user = await GetUserDetailsbyID(userDetails.ZUID);

        return Ok(new
        {
            message = "User authenticated successfully.",
            userId = userDetails.ZUID,
            user.access_token,
            user.refresh_token,
            userDetails.Display_Name,
            userDetails.Email
        });
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] dynamic data)
    {
        string refreshToken = data.refreshToken;
        string zuid = data.zuid;
        string clientId = _configuration["Zoho:ClientId"];
        string clientSecret = _configuration["Zoho:ClientSecret"];

        var tokenResponse = await new HttpClient().PostAsync(
            $"{_configuration["Zoho:OAuthBaseUrl"]}/token?grant_type=refresh_token&client_id={clientId}&client_secret={clientSecret}&refresh_token={refreshToken}",
            null
        );

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return Unauthorized("Error refreshing access token.");
        }

        var tokenData = await tokenResponse.Content.ReadAsStringAsync();
        var tokenJson = JObject.Parse(tokenData);

        string newAccessToken = tokenJson["access_token"]?.ToString();

        // Update access token in the database
        await UpdateAccessTokenInDatabase(zuid, newAccessToken);

        return Ok(new { accessToken = newAccessToken });
    }


    #region DbFunctions
    private async Task<UserModel> SaveUserDetails(UserModel item, string access_token, string refresh_token)
    {
        var existingUser = await _context.Users.Where(u => u.ZUID == item.ZUID).FirstOrDefaultAsync();
        if (existingUser == null)
        {
            Users u = new Users();

            u.First_Name = item.First_Name;
            u.Last_Name = item.Last_Name;
            u.Display_Name = item.Display_Name;
            u.Email = item.Email;
            u.ZUID = item.ZUID;
            u.access_token = access_token;
            u.Createddate = DateTime.UtcNow;
            u.refresh_token = refresh_token;

            await _context.Users.AddAsync(u);
            await _context.SaveChangesAsync();
            item.ID = u.ID;
        }
        else if (existingUser.access_token != access_token)
        {
            existingUser.access_token = access_token;
            existingUser.refresh_token = refresh_token;
            await _context.SaveChangesAsync();
        }
        return item;
    }

    private async Task<bool> UpdateAccessTokenInDatabase(string zuid, string new_access_token)
    {
        var existingUser = await _context.Users.Where(u => u.ZUID == zuid).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            existingUser.access_token = new_access_token;
            await _context.SaveChangesAsync();
            return true;
        }
        else return false;
    }

    private async Task<UserModel> GetUserDetailsbyID(string zuid)
    {
        var dbuser = await _context.Users.Where(u => u.ZUID == zuid).FirstOrDefaultAsync();
        if (dbuser != null)
        {
            return new UserModel()
            {
                access_token = dbuser.access_token,
                refresh_token = dbuser.refresh_token
            };
        }
        else return null;
    }
    #endregion
}


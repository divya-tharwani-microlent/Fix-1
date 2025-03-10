using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Web;
using Zoho_timesheet_API.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

[Route("api/zoho")]
[ApiController]
public class ZohoProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;


    public ZohoProxyController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("portals")]
    public async Task<IActionResult> GetZohoPortals()
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues accessToken))
        {
            return Unauthorized("Access token is missing.");
        }

        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, _configuration["Zoho:ApiBaseUrl"] + "portals/");
        request.Headers.Add("Authorization", accessToken.ToString()); // Forward token to Zoho

        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        return StatusCode((int)response.StatusCode, "Error fetching data from Zoho API");
    }

    // Get User Timesheet
    [HttpGet("timesheet-logs")]
    public async Task<IActionResult> GetUserTimesheetLogs(string userId, DateTime start_date, DateTime end_date)
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues accessToken))
        {
            return Unauthorized("Access token is missing.");
        }

        var client = _httpClientFactory.CreateClient();

        // Step 1️: Fetch Portal ID
        var portalRequest = new HttpRequestMessage(HttpMethod.Get, _configuration["Zoho:ApiBaseUrl"] + "portals/");
        portalRequest.Headers.Add("Authorization", accessToken.ToString());

        var portalResponse = await client.SendAsync(portalRequest);
        if (!portalResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)portalResponse.StatusCode, "Failed to fetch Zoho portals.");
        }

        var portalContent = await portalResponse.Content.ReadAsStringAsync();
        var portalData = JsonSerializer.Deserialize<ZohoPortalsResponse>(portalContent);

        if (portalData?.portals == null || portalData.portals.Count == 0)
        {
            return BadRequest("No portals found.");
        }

        string portalId = portalData.portals[0].id.ToString(); // Use the first portal ID

        // Step 2️: Fetch Timesheet Logs Using Portal ID

        DateTime parsedStartDate = DateTime.Parse(start_date.ToString());  // Convert to DateTime
        string formatted_start_date = parsedStartDate.ToString("MM-dd-yyyy");
        DateTime parsedEndDate = DateTime.Parse(end_date.ToString());
        string formatted_end_date = parsedEndDate.ToString("MM-dd-yyyy");

        var customDateObject = new
        {
            start_date = formatted_start_date,
            end_date = formatted_end_date
        };
        // Serialize JSON object to a string and URL-encode it
        string jsonString = JsonConvert.SerializeObject(customDateObject);
        string encodedJson = HttpUtility.UrlEncode(jsonString);
        string logsUrl = $"{_configuration["Zoho:ApiBaseUrl"]}portal/{portalId}/logs?users_list={userId}&view_type=custom_date&date={formatted_start_date}&custom_date={encodedJson}&bill_status=All&component_type=task";

        var logsRequest = new HttpRequestMessage(HttpMethod.Get, logsUrl);
        logsRequest.Headers.Add("Authorization", accessToken.ToString());

        var logsResponse = await client.SendAsync(logsRequest);
        return await HandleZohoResponse(logsResponse);
    }


    //Handle Zoho API Response
    private async Task<IActionResult> HandleZohoResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        return StatusCode((int)response.StatusCode, "Error fetching data from Zoho API");
    }
}

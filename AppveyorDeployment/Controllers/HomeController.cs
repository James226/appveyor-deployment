using Microsoft.AspNetCore.Mvc;

namespace AppveyorDeployment.Controllers
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AppveyorPayload value)
        {
            using (var client = new HttpClient())
            {
                var authHeader = HttpContext.Request.Headers["Authorization"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authHeader[0].Substring("Bearer ".Length));

                var payload = value.Branch == "master"
                    ? (object) new
                    {
                        value.AccountName,
                        ProjectSlug = value.EnvironmentVariables["deploy_project_slug"],
                        value.Branch,
                        value.EnvironmentVariables
                    }
                    : new
                    {
                        value.AccountName,
                        ProjectSlug = value.EnvironmentVariables["deploy_project_slug"],
                        value.Branch,
                        value.CommitId,
                        value.EnvironmentVariables
                    };
                var response = await client.PostAsync(
                    "https://ci.appveyor.com/api/builds",
                    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                return this.Ok(await response.Content.ReadAsStringAsync());
            }
        }
    }

    public class AppveyorPayload
    {
        public string AccountName { get; set; }

        public int BuildNumber { get; set; }

        public string Branch { get; set; }

        public string CommitId { get; set; }

        public Dictionary<string,string> EnvironmentVariables { get; set; }
    }
}

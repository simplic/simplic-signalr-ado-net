using System.Web.Http;

namespace Simplic.SignalR.Ado.Net.Host.Controller
{
    public class StatusController : ApiController
    {
        public IHttpActionResult Get()
        {
            return Ok(new { status = "up and running!" });
        }
    }
}

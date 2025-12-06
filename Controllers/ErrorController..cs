using Microsoft.AspNetCore.Mvc;
using Serilog;

public class ErrorController : Controller
{
    [Route("Error/404")]
    public IActionResult PageNotFound()
    {
        Log.Warning("404 error occurred");
        return View("404");
    }

    [Route("Error/500")]
    public IActionResult ServerError()
    {
        Log.Error("500 Internal Server Error occurred");
        return View("500");
    }
}
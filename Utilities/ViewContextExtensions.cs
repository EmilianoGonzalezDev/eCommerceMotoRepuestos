using Microsoft.AspNetCore.Mvc.Rendering;

namespace eCommerceMotoRepuestos.Utilities;

public static class ViewContextExtensions
{
    public static string CurrentPathAndQuery(this ViewContext viewContext)
    {
        var request = viewContext.HttpContext.Request;
        return $"{request.Path}{request.QueryString}";
    }
}

using Microsoft.AspNetCore.Mvc;

namespace TestBaza.Factories
{
    public interface IResponseFactory
    {
        BadRequestResult BadRequest(Controller controller);
        BadRequestObjectResult BadRequest(Controller controller, object result);
        OkResult Ok(Controller controller);
        OkObjectResult Ok(Controller controller, object result);
        StatusCodeResult StatusCode(Controller controller, int statusCode);
        ObjectResult StatusCode(Controller controller, int statusCode, object result);
        ForbidResult Forbid(Controller controller);
        NotFoundResult NotFound(Controller controller);
        ConflictResult Conflict(Controller controller);
        NoContentResult NoContent(Controller controller);
        ViewResult View(Controller controller);
        ViewResult View(Controller controller, object model);
        ViewResult View(Controller controller, string viewName);
        ViewResult View(Controller controller, string viewName, object model);
        RedirectToActionResult RedirectToAction(Controller controller, string actionName, string controllerName, object? routeValues);
    }
}
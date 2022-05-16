using Microsoft.AspNetCore.Mvc;

namespace TestBaza.Factories
{
    public class ResponseFactory : IResponseFactory
    {
        public BadRequestResult BadRequest(Controller controller) => controller.BadRequest();
        public BadRequestObjectResult BadRequest(Controller controller, object result) 
            => controller.BadRequest(new { result });
        public ForbidResult Forbid(Controller controller) => controller.Forbid();
        public NotFoundResult NotFound(Controller controller) => controller.NotFound();
        public ConflictResult Conflict(Controller controller) => controller.Conflict();
        public NoContentResult NoContent(Controller controller) => controller.NoContent();
        public OkResult Ok(Controller controller) => controller.Ok();
        public OkObjectResult Ok(Controller controller, object result) => controller.Ok(new { result });
        public StatusCodeResult StatusCode(Controller controller, int statusCode) => controller.StatusCode(statusCode);
        public ObjectResult StatusCode(Controller controller, int statusCode, object result)
            => controller.StatusCode(statusCode, new { result });
        public ViewResult View(Controller controller) => controller.View();
        public ViewResult View(Controller controller, object model) => controller.View(model: model);
        public ViewResult View(Controller controller, string viewName) => controller.View(viewName);
        public ViewResult View(Controller controller, string viewName, object model) 
            => controller.View(viewName, model);
        public RedirectToActionResult RedirectToAction(Controller controller, 
            string actionName, 
            string controllerName, 
            object? routeValues)
            => controller.RedirectToAction(actionName, controllerName, routeValues);
    }
}
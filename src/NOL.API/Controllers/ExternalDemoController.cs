using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.API.Extensions;

namespace NOL.API.Controllers
{
    [ApiController]
    [Route("api/external-demo")]
    public class ExternalDemoController : ControllerBase
    {
        private readonly JsonPlaceholderApi _api;

        public ExternalDemoController(JsonPlaceholderApi api)
        {
            _api = api;
        }

        // GET: /api/external-demo/todo/1
        [HttpGet("todo/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTodo(int id)
        {
            var todo = await _api.GetTodoAsync(id);
            return Ok(todo);
        }
    }
}



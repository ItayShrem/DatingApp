using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected readonly ILogger<UsersController> _logger;

        protected readonly DataContext _context;

        public BaseApiController(ILogger<UsersController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }
    }
}
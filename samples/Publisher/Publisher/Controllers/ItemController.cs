using System.Net;
using EventBus.Base.Standard;
using Microsoft.AspNetCore.Mvc;
using Publisher.IntegrationEvents.Events;

namespace Publisher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IEventBus _eventBus;

        public ItemController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        public IActionResult Publish()
        {
            var message = new ItemCreatedIntegrationEvent("Item title", "Item description");

            _eventBus.Publish(message);

            return Ok("A message has been published.");
        }
    }
}

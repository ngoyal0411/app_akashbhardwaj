using Ecommerce.Contracts;
using Ecommerce.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _service;

        public ShoppingCartController(IShoppingCartService service)
        {
            _service = service;
        }

        // GET api/shoppingcart
        [HttpGet]
        public ActionResult<IEnumerable<ShoppingItem>> Get()
        {
            var items = _service.GetAllItems();
            return Ok(items);
        }

        // GET api/shoppingcart/5
        [HttpGet("{id}")]
        public ActionResult<ShoppingItem> Get(int id)
        {
            var item = _service.GetById(id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // POST api/shoppingcart
        [HttpPost]
        public ActionResult Post([FromBody] ShoppingItem value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = _service.Add(value);
            return CreatedAtAction("Get", new { id = item.Id }, item);
        }
    }
}

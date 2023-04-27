using Api.FurnitureStore.Data;
using Api.FurnitureStore.Share;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Api.FurnitureStore.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly APIFurnitureStoreContext _context;

        public OrdersController(APIFurnitureStoreContext context)
        {
            _context = context;
        }


        [HttpGet]

        public async Task<IEnumerable<Order>> Get()
        {
            return await _context.Orders.Include(o => o.orderDetails).ToListAsync();
        }


        [HttpGet("{Id}")]
        public async Task<IActionResult> GetDetails(int Id)
        {
            var order = await _context.Orders.Include(o => o.orderDetails).FirstOrDefaultAsync(o => o.Id == Id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]

        public async Task<IActionResult> Post(Order order)
        {
            if (order.orderDetails == null)
            {

                return BadRequest("Order Should have at least one details");

            }

            await _context.Orders.AddAsync(order);
            await _context.OrderDetails.AddRangeAsync(order.orderDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Post", order.Id, order);
        }


        [HttpPut]

        public async Task<IActionResult> Put(Order order)
        {
            if (order == null)
            {
                return NotFound();
            }
            if (order.Id <= 0)
            {
                return NotFound();
            }

            var existingOrder = await _context.Orders.Include(o => o.orderDetails).FirstOrDefaultAsync(o => o.Id == order.Id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            existingOrder.OrderNumber = order.OrderNumber;
            existingOrder.OrderDate = order.OrderDate;
            existingOrder.DeliveryDate = order.DeliveryDate;
            existingOrder.ClientId = order.ClientId;

            _context.OrderDetails.RemoveRange(existingOrder.orderDetails);

            _context.Orders.Update(existingOrder);
            _context.OrderDetails.AddRange(order.orderDetails);

            await _context.SaveChangesAsync();

            return NoContent();

        }



        [HttpDelete]

        public async Task<IActionResult> Delete(Order orders)
        {
            if (orders== null) 
            {
                return NotFound();  
            }
             var existingOrder = await _context.Orders.Include(o => o.orderDetails).FirstOrDefaultAsync (o => o.Id == orders.Id);
            if (existingOrder == null) 
            {
                return NotFound();  
            }

            _context.OrderDetails.RemoveRange(existingOrder.orderDetails);
            _context.Orders.Remove(existingOrder);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }

}

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;


namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public OrderController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _httpClient = new HttpClient();
            var productServiceUrl = configuration.GetValue<string>("ProductServiceUrl") ?? "http://localhost:8081/";
            _httpClient.BaseAddress = new Uri(productServiceUrl);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return Ok(await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync());
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = $"Order with ID {id} not found" });

            return Ok(order);
        }

        // [HttpGet("products")]
        // public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        // {
        //     var response = await _httpClient.GetAsync("http://localhost:8081/api/product");
        //     var productsJson = await response.Content.ReadAsStringAsync();
        //     var products = JsonSerializer.Deserialize<List<ProductDto>>(productsJson, new JsonSerializerOptions
        //     {
        //         PropertyNameCaseInsensitive = true
        //     });
        //     return Ok(products);
        // }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            // Validated each product with Product Service
            foreach (var item in request.Items)
            {
                // Calling Product Service
                var response = await _httpClient.GetAsync($"api/product/{item.ProductId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { message = $"Product with ID {item.ProductId} not found!" });
                }

                var productJson = await response.Content.ReadAsStringAsync();
                
                var product = JsonSerializer.Deserialize<ProductDto>(productJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (product == null)
                {
                    return BadRequest(new { message = $"Product with ID {item.ProductId} not found" });
                }

                // Check stock
                if (product.ProductQuantity < item.Quantity)
                    return BadRequest(new { 
                        message = $"Insufficient stock for product: {product.Name}", 
                        available = product.ProductQuantity,
                        requested = item.Quantity
                    });

                // Update stock in Product Service
                await _httpClient.PatchAsync($"api/product/{item.ProductId}/stock?quantity={-item.Quantity}", null);

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
                totalAmount += orderItems.Last().Subtotal;
            }

            // Create the order
            var order = new Order
            {
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                Status = "Confirmed",
                TotalAmount = totalAmount,
                Items = orderItems,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.Items)  // ← ADD THIS to include items
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound(new { message = $"Order with ID {id} not found" });

            order.Status = request.Status.ToString();
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found" });
            }

            // Restoring stock
            foreach (var item in order.Items)
            {
                await _httpClient.PatchAsync($"api/product/{item.ProductId}/stock?quantity={item.Quantity}", null);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/orders/customer/{email}
        [HttpGet("customer/{email}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(string email)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.CustomerEmail == email)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

    }

    public class CreateOrderRequest
    {
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
    }

     public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int ProductQuantity { get; set; }
    }

    public class UpdateStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
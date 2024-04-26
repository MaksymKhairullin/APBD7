using System.Data.SqlClient;
using System.Transactions;
using System.Web.Http.Results;
using DB_API.DTOs;
using DB_API.Services;
//using DB_API.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DB_API;

[ApiController]
[Route("api/warehouse")]
public class WarehouseController: ControllerBase
{
    
   
    private static IDbService _dbService;

     public WarehouseController(IDbService dbService)
    {
        _dbService = dbService;
        
    }

    

    [HttpPost]
    public  async Task<IActionResult> AddProductToWarehouse(NewProductRequest newProductRequest)
    {
        
        var productExists = await _dbService.DoesProductExist(newProductRequest.IdProduct);
            if (!productExists)
            {
                return NotFound("Product does not exist.");
            }
            if (newProductRequest is null)
            {
                return BadRequest(nameof(newProductRequest));
            }
            if (newProductRequest.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }
            if (!await _dbService.DoesProductExist(newProductRequest.IdProduct))
            {
                return NotFound($"Product with ID {newProductRequest.IdProduct} not found");
            }
            if (!await _dbService.DoesWarehouseExist(newProductRequest.IdWarehouse))
            {
                return NotFound($"Warehouse with ID {newProductRequest.IdWarehouse} not found");
            }
            if (!await _dbService.IsOrderFulfilled(newProductRequest.IdProduct, newProductRequest.Amount))
            {
                return BadRequest("The order is not fulfilled yet");
            }
            
            int key = await _dbService.InsertProductWarehouse(newProductRequest.IdProduct, newProductRequest.IdWarehouse, newProductRequest.CreatedAt);
            return Ok(key);
                
    }
}
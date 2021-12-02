using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HPlusSport.API.Classes;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContex _contex;

        public ProductsController(ShopContex contex)
        {
            _contex = contex;
            _contex.Database.EnsureCreated();
        }
        [HttpGet]
        public async Task<IActionResult>  GetAllProducts([FromQuery] ProductQueryParameters queryParameters)
        {
            // IQueryable permite que se faça a filtragem e a paginação
            IQueryable<Product> products = _contex.Products;

            // Filtrando por Preço entre minimo e maximo
            if (queryParameters.MinPrice != null && queryParameters.MaxPrice != null)
            {
                products = products.Where(
                    p => p.Price >= queryParameters.MinPrice.Value &&
                         p.Price <= queryParameters.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                products = products.Where(
                    p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                         p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower()));
            }
            
            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
            }

            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(queryParameters.Name.ToLower())
                    );
            }

            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);
                } 
            }
            
            // Skip e Take usado para paginação.
            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);
            
            return Ok(await products.ToArrayAsync());
        }
        
        [HttpGet("{id:int}")]
        public async Task<IActionResult>  GetProduct(int id)
        {
            var product = await  _contex.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}
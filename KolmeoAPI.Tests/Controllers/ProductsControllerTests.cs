using KolmeoAPI.Controllers;
using KolmeoAPI.DTOs;
using KolmeoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KolmeoAPI.Tests.Controllers;

public class ProductsControllerTests
{
    private ProductsController _controller;
    private List<ProductDTO> _testProductsDTO;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase("ProductDatabase")
            .Options;
        var context = new ProductContext(options);

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var testProducts = new List<Product>()
        {
            new Product{ 
                Id = 1,
                Name = "Test Product 1",
                Description = "Test Description 1",
                Price = 0,
            },
            new Product{
                Id = 2,
                Name = "Test Product 2",
                Description = "Test Description 2",
                Price = 10,
            },
            new Product{
                Id = 3,
                Name = "Test Product 3",
                Description = "Test Description 3",
                Price = 20,
            },
            new Product{
                Id = 4,
                Name = "Test Product 4",
                Description = "Test Description 4",
                Price = 100,
            }
        };

        _testProductsDTO = testProducts.Select(product => new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price
        }).ToList();

        context.Products.AddRange(testProducts);

        context.SaveChanges();

        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        var factory = serviceProvider.GetService<ILoggerFactory>();

        var logger = factory.CreateLogger<ProductsController>();

        _controller = new ProductsController(context, logger);
    }

    [Test]
    public async Task GetById_IdExists()
    {
        foreach (var productDTO in _testProductsDTO) {
            var result = await _controller.GetProduct(productDTO.Id);

            var retrievedProductDTO = result.Value;
            Assert.That(retrievedProductDTO, Is.EqualTo(productDTO));
        }
    }

    [Test]
    public async Task GetById_IdDoesNotExist()
    {
        var result = await _controller.GetProduct(5);
        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task GetProducts()
    {
        var result1 = await _controller.GetProducts();
        var products1 = result1.Value;

        //Tests that get products endpoint works with no optional parameters
        Assert.That(products1, Is.EquivalentTo(_testProductsDTO));

        var result2 = await _controller.GetProducts(5, 50);
        var products2 = result2.Value;

        //Tests that min and max price parameters constrain retrieved products
        Assert.That(products2, Is.EquivalentTo(_testProductsDTO.Skip(1).Take(2)));

        var result3 = await _controller.GetProducts(500, 1000);
        var products3 = result3.Value;

        //Tests that when no products are between price constraints an empty list is returned
        Assert.That(products3, Is.EquivalentTo(new List<ProductDTO>()));
    }

    [Test]
    public async Task DeleteProduct_IdExists()
    {
        //Make sure product exists first
        var getResult = await _controller.GetProduct(1);
        var retrievedProductDTO = getResult.Value;
        Assert.That(retrievedProductDTO, Is.EqualTo(_testProductsDTO[0]));

        var deleteResult = await _controller.DeleteProduct(1);
        //Delete endpoint returns no content if deletion is successful
        Assert.That(deleteResult, Is.InstanceOf<NoContentResult>());

        //Assert that product does not exist after deletion
        getResult = await _controller.GetProduct(1);
        Assert.That(getResult.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteProduct_IdDoesNotExist()
    {
        var deleteResult = await _controller.DeleteProduct(5);
        //Delete endpoint returns not found if product with id is not found
        Assert.That(deleteResult, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task PostProduct()
    {
        var productDTO = new ProductDTO
        {
            Name = "Test Product 5",
            Description = "Test Description 5",
            Price = 200
        };

        var postResult = await _controller.PostProduct(productDTO);
        // Post endpoint returns created at action result if successful
        Assert.That(postResult.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdId = ((ProductDTO)((CreatedAtActionResult) postResult.Result).Value).Id;


        //Retrieve newly created item with id from result
        var createdProductDTO = productDTO with { Id = createdId };
        var getResult = await _controller.GetProduct(createdId);

        var retrievedProductDTO = getResult.Value;
        Assert.That(retrievedProductDTO, Is.EqualTo(createdProductDTO));
    }

    [Test]
    public async Task PutProduct_IdExists()
    {
        var id = 1;
        var productDTO = new ProductDTO
        {
            Id = id,
            Name = "Test Product 1 Version 2",
            Description = "Test Description 1 Version 2",
            Price = 150
        };

        var putResult = await _controller.PutProduct(id, productDTO);
        //Put Endpoint returns no content if update is successful
        Assert.That(putResult, Is.InstanceOf<NoContentResult>());

        //Test whether the product has been properly updated by using the get endpoint
        var result = await _controller.GetProduct(id);

        var retrievedProductDTO = result.Value;
        Assert.That(retrievedProductDTO, Is.EqualTo(productDTO));
    }

    [Test]
    public async Task PutProduct_IdParameterDoesNotMatchDTOId()
    {
        var productDTO = _testProductsDTO[0];
        var idParameter = 2; //Does not match productDTO Id of 1

        var putResult = await _controller.PutProduct(idParameter, productDTO);
        Assert.That(putResult, Is.InstanceOf<BadRequestResult>());
    }

    [Test]
    public async Task PutProduct_IdDoesNotExist()
    {
        //Id does not exist
        var id = 5;
        var productDTO = new ProductDTO
        {
            Id = id,
            Name = "Test Product 5",
            Description = "Test Description 5",
            Price = 200
        };

        var putResult = await _controller.PutProduct(id, productDTO);
        Assert.That(putResult, Is.InstanceOf<NotFoundResult>());
    }
    //////////////////////////////////////ADDDDDDDDDDDDDDDDDDDDDDDDDD VALIDATION/////////////////////////////////
    /////////////////////////////////////////ADDDDDDDDDDDDDDDDDDDDDDDDDD VALIDATION/////////////////////////////////
    /////////////////////////////////////////ADDDDDDDDDDDDDDDDDDDDDDDDDD VALIDATION/////////////////////////////////
    /////////////////////////////////////////ADDDDDDDDDDDDDDDDDDDDDDDDDD VALIDATION/////////////////////////////////
    /////////////////////////////////////////ADDDDDDDDDDDDDDDDDDDDDDDDDD VALIDATION FOR PRICE
    /////////////////////////////////////////Possibly add automapping//////////////////////////////////////////////////////////////////
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCMS.API.Controllers;
using SCMS.API.DTOs;
using SCMS.Domain.Entities;
using SCMS.Infrastructure.Data;
using Xunit;

namespace SCMS.API.Tests;

public class DepartmentsControllerTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetAll_ReturnsAllDepartments()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new DepartmentsController(context);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var departments = Assert.IsAssignableFrom<IEnumerable<DepartmentResponseDto>>(okResult.Value);
        Assert.NotEmpty(departments);
    }

    [Fact]
    public async Task GetById_ReturnsDepartment_WhenFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new DepartmentsController(context);

        // Act
        var result = await controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dept = Assert.IsType<DepartmentResponseDto>(okResult.Value);
        Assert.Equal(1, dept.Id);
        Assert.Equal("CS", dept.Code);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNotFound()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new DepartmentsController(context);

        // Act
        var result = await controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}

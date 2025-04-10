using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xprema.Managment.Application.Contracts.Procedures.Dtos;
using Xprema.Managment.Application.Procedures;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.EntityFrameworkCore;
using Xunit;

namespace Xprema.Managment.UnitTests;

public class ProcedureAppServiceTests : IDisposable
{
    private readonly IMapper _mapper;
    private readonly ManagmentDbContext _dbContext;
    private readonly FlowProcedureAppService _procedureAppService;
    private readonly List<FlowProcedure> _sampleProcedures;
    private readonly string _testUserId = "test-user-1";

    public ProcedureAppServiceTests()
    {
        // Setup AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            // Add your mapping configurations here
            cfg.CreateMap<FlowProcedure, FlowProcedureDto>();
            cfg.CreateMap<CreateUpdateFlowProcedureDto, FlowProcedure>();
            cfg.CreateMap<CreateUpdateFlowProcedureDto, FlowProcedure>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ManagmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ManagmentDbContext(options);

        // Setup sample data
        _sampleProcedures = new List<FlowProcedure>
        {
            new FlowProcedure
            {
                Id = Guid.NewGuid(),
                ProcedureName = "Test Procedure 1",
                Description = "Test Description 1",
                IsSystem = false,
                IsActive = true,
                CreationTime = DateTime.Now.AddDays(-10),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedBy = _testUserId
            },
            new FlowProcedure
            {
                Id = Guid.NewGuid(),
                ProcedureName = "Test Procedure 2",
                Description = "Test Description 2",
                IsSystem = true,
                IsActive = true,
                CreationTime = DateTime.Now.AddDays(-5),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedBy = _testUserId
            }
        };

        // Seed the database
        _dbContext.FlowProcedures.AddRange(_sampleProcedures);
        _dbContext.SaveChanges();

        // Create service with test user ID
        _procedureAppService = new FlowProcedureAppService(_dbContext, _mapper, _testUserId);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnMappedProcedure_WhenProcedureExists()
    {
        // Arrange
        var existingProcedure = _sampleProcedures.First();

        // Act
        var result = await _procedureAppService.GetAsync(existingProcedure.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingProcedure.Id, result.Id);
        Assert.Equal(existingProcedure.ProcedureName, result.ProcedureName);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowException_WhenProcedureDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            async () => await _procedureAppService.GetAsync(nonExistentId));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnNewProcedure()
    {
        // Arrange
        var createDto = new CreateUpdateFlowProcedureDto
        {
            ProcedureName = "New Procedure",
            Description = "New Description",
            IsSystem = false,
            IsActive = true
        };

        // Act
        var result = await _procedureAppService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.ProcedureName, result.ProcedureName);
        Assert.Equal(createDto.Description, result.Description);

        // Verify it's in the database
        var savedProcedure = await _dbContext.FlowProcedures.FindAsync(result.Id);
        Assert.NotNull(savedProcedure);
        Assert.Equal(createDto.ProcedureName, savedProcedure.ProcedureName);
        Assert.Equal(_testUserId, savedProcedure.CreatedBy);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndReturnUpdatedProcedure()
    {
        // Arrange
        var existingProcedure = _sampleProcedures.First();
        var updateDto = new CreateUpdateFlowProcedureDto
        {
            ProcedureName = "Updated Name",
            Description = "Updated Description",
            IsSystem = existingProcedure.IsSystem,
            IsActive = existingProcedure.IsActive,
            ConcurrencyStamp = existingProcedure.ConcurrencyStamp
        };

        // Act
        var result = await _procedureAppService.UpdateAsync(existingProcedure.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.ProcedureName, result.ProcedureName);
        Assert.Equal(updateDto.Description, result.Description);

        // Verify it's updated in the database
        var updatedProcedure = await _dbContext.FlowProcedures.FindAsync(existingProcedure.Id);
        Assert.NotNull(updatedProcedure);
        Assert.Equal(updateDto.ProcedureName, updatedProcedure.ProcedureName);
        Assert.Equal(_testUserId, updatedProcedure.CreatedBy);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
} 
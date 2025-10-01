// See https://aka.ms/new-console-template for more information

using GainsLab.Core.Models.Core.Factory;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;
using GainsLab.Models.Factory;
using Microsoft.EntityFrameworkCore;
using Results = Microsoft.AspNetCore.Http.Results;


EntityFactory _entityFactory = new();

var equipments = _entityFactory.CreateBaseEquipments();
var equipmentsDtos = equipments.Select(e => (EquipmentDTO)e.ToDTO()!);

var b = WebApplication.CreateBuilder(args);

b.Services.AddDbContext<GainLabPgDBContext>(o =>
    o.UseNpgsql(b.Configuration.GetConnectionString("Default")));

b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen();

var app = b.Build();
app.UseSwagger(); app.UseSwaggerUI();



// simple seed
app.MapPost("/seed", async (GainLabPgDBContext db) =>
{
    if (!await db.Equipments.AnyAsync())
    {
        db.Equipments.AddRange(
            equipmentsDtos!
        );
        await db.SaveChangesAsync();
    }
    return Results.Ok();
});


app.Run();
using BuckingshireImporter;
using FamilyHub.DataImporter.Web;
using FamilyHub.DataImporter.Web.Data;
using HounslowconnectImporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpenActiveImporter;
using PlacecubeImporter;
using PluginBase;
using PublicPartnershipImporter;
using SalfordImporter;
using SouthamptonImporter;
using SportEngland;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureHost();

builder.Services.RegisterApplicationComponents(builder.Configuration);

// Add services to the container.
builder.Services.RegisterApplicationComponents(builder.Configuration);


var app = builder.Build();

await app.ConfigureWebApplication();

await app.RunAsync();

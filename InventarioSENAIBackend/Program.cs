
using InventarioSENAIBackend.Repository;
using InventarioSENAIBackend.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<StorageService>();

builder.Services.AddScoped<UsuarioStorageRepository>();
builder.Services.AddScoped<SalaStorageRepository>();
builder.Services.AddScoped<ProdutoStorageRepository>();
builder.Services.AddScoped<EmprestimoStorageRepository>();
builder.Services.AddScoped<DevolucaoStorageRepository>();
builder.Services.AddScoped<MovimentacaoStorageRepository>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

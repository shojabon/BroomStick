using BroomStick.DataClasses;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<BroomStick.DataClasses.BroomStick>();
builder.Services.AddSingleton<Authenticator>();
var app = builder.Build();
var broomStick = new BroomStick.DataClasses.BroomStick(app.Environment);
var authenticator = new Authenticator(app.Environment);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

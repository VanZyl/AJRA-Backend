using Microsoft.EntityFrameworkCore;
using AJRAApis.Data;
using AJRAApis.Interfaces;
using AJRAApis.Repository;
using AJRAApis;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy",
        policy =>
        {
            // policy.WithOrigins("http://localhost");
            policy.WithOrigins("http://192.168.0.181:4201")
                .AllowAnyHeader()
                .AllowAnyMethod();
                // .AllowCredentials();
        });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<ApplicationDBContext>(options =>  // Need to add this on your own
{
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DockerConnectionString"));
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<iEmployeeRepository,EmployeeRepository>();   
builder.Services.AddScoped<iPayslipRepository,PayslipRepository>();
builder.Services.AddScoped<iRedbookRepository,RedbookRepository>();
builder.Services.AddScoped<iEmployeeLeave,EmployeeLeaveRepository>();

builder.Services.AddWindowsService();
// builder.Services.AddHostedService<ServiceA>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
    {
    if (app.Environment.IsDevelopment())
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API V1");
    }
    else
    {
        // To deploy on IIS
        c.SwaggerEndpoint("/AJRAApis/swagger/v1/swagger.json", "Web API V1");
    }

    });

// app.Use(async (context, next) =>
// {
//     Console.WriteLine($"Incoming request: {context.Request.Method} {context.Request.Path}");
//     foreach (var header in context.Request.Headers)
//     {
//         Console.WriteLine($"{header.Key}: {header.Value}");
//     }
//     await next.Invoke();
// });
app.UseCors("MyPolicy");
app.UseAuthorization();
app.MapControllers();
app.Run();
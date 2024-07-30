var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddWebOptimizer(pipeline =>
    {
        pipeline.AddCssBundle("/css/bundle.css", "css/*.css");
        pipeline.AddJavaScriptBundle("/js/bundle.js", "js/*.js");
    }
);

var app = builder.Build();

app.UseWebOptimizer();

// app.UseHttpsRedirection(); // HTTPS
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Login}/{id?}"
);

app.Run();

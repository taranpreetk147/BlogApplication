using BlogApplication;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
    options.SlidingExpiration = true;
    options.LoginPath = "/Users/Login";
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Privacy");
    options.Conventions.AuthorizePage("/Blog/Createblog");
});

builder.Services.AddSession(options =>
{
    // Set a short timeout for easy testing
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    // Make the session cookie essential
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));


// Add IHttpContextAccessor to the service container
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId= "119460238133-sash8mnoi1me3ho0hmd5on207r7u49fi.apps.googleusercontent.com";
    options.ClientSecret= "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";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];


app.UseSession(); // Add session middleware before authentication and authorization middleware

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});

app.Run();

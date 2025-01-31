using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiscordStats.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using DiscordStats.DAL.Abstract;
using DiscordStats.DAL.Concrete;
using DiscordStats.Models;
using DiscordStats.ViewModels;

var builder = WebApplication.CreateBuilder(args);

//for local use
var ConnectionString =
//for local discordIdentitydb use
    builder.Configuration.GetConnectionString("DiscordStatsContextConnection");
builder.Services.AddDbContext<DiscordStatsIdentityDbContext>(options => options.UseSqlServer(ConnectionString).UseLazyLoadingProxies());
builder.Services.AddDbContext<DiscordStatsContext>(options => options.UseSqlServer(ConnectionString).UseLazyLoadingProxies());
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
   .AddEntityFrameworkStores<DiscordStatsContext>();
//for local discordDatadb use
var ConnectionString2 =
    builder.Configuration.GetConnectionString("DiscordDataConnection");
builder.Services.AddDbContext<DiscordDataDbContext>(options => options.UseSqlServer(ConnectionString2).UseLazyLoadingProxies());

// for azure use
//var connectionString = builder.Configuration.GetConnectionString("DiscordDataConnection");
//builder.Services.AddDbContext<DiscordDataDbContext>(options =>
//     options.UseSqlServer(connectionString));

//var identityString = builder.Configuration.GetConnectionString("DiscordStatsIdentityDbContextConnection");
//builder.Services.AddDbContext<DiscordStatsIdentityDbContext>(options =>
//    options.UseSqlServer(identityString));
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<DiscordStatsIdentityDbContext>();


// Register an IHttpClientFactory to enable injection of HttpClients
builder.Services.AddHttpClient();
builder.Services.AddMvc().AddRazorRuntimeCompilation();
// Add our repositories and services
builder.Services.AddScoped<IDiscordService, DiscordService>();
builder.Services.AddScoped<IServerRepository, ServerRepository>();
builder.Services.AddScoped<ServerOwnerViewModel, ServerOwnerViewModel>();
builder.Services.AddScoped<IServerUserJoinRepository, ServerUserJoinRepository>();
builder.Services.AddScoped<IDiscordUserAndUserWebSiteInfoRepository, DiscordUserAndUserWebSiteInfoRepository>();
builder.Services.AddScoped<IDiscordServicesForChannels, DiscordServicesForChannels>();  
builder.Services.AddScoped<IPresenceRepository, PresenceRepository>();
builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
builder.Services.AddScoped<IMessageInfoRepository, MessageInfoRepository>();
builder.Services.AddScoped<IVoiceChannelRepository, VoiceChannelRepository>();
builder.Services.AddScoped<IVoiceStateRepository, VoiceStateRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IServerMemberRepository, ServerMemberRepository>();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(o => o.LoginPath = new PathString("/Identity/Account/Login"))
    .AddOAuth("Discord",
            options =>
            {

                options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
                options.Scope.Add("identify");
                options.Scope.Add("guilds");
                options.Scope.Add("guilds.members.read");
                options.Scope.Add("guilds.join");
                //Can add more here
                options.CallbackPath = new PathString("/auth/oauthCallback");
                options.ClientId = builder.Configuration["API:ClientId"];
                options.ClientSecret = builder.Configuration["API:ClientSecret"];
                options.TokenEndpoint = "https://discord.com/api/oauth2/token";
                options.UserInformationEndpoint = "https://discord.com/api/users/@me";
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
                options.AccessDeniedPath = "/Home/DiscordAuthFailed";
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {

                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var claims = new List<Claim>
                        {
                        new Claim(ClaimTypes.Role, context.AccessToken)
                        };

                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();
                        var appIdentity = new ClaimsIdentity(claims);
                        var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                        context.Principal.AddIdentity(appIdentity);
                        context.RunClaimActions(user);
                    }

                };


            });
;
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.Use(async (context, next) =>
{
    context.Response.GetTypedHeaders().CacheControl =
        new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
        {
            MaxAge = TimeSpan.FromSeconds(0),
            NoCache = true,
            NoStore = true,
            MustRevalidate = true
            
        };
    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
        new string[] { "Accept-Encoding" };

    await next(context);
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});


app.Run();

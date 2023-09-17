
using Microsoft.Identity.Web;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add the reverse proxy capability to the server and initialize the reverse proxy from the "ReverseProxy" section of configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    // Add a custom transform to add a header to requests proxied to the backend
    .AddTransforms(builderContext =>
    {
        // Conditionally add a transform for routes that require auth.
        if (!string.IsNullOrEmpty(builderContext.Route.AuthorizationPolicy))
        {
            builderContext.AddRequestTransform(async transformContext =>
            {
                transformContext.ProxyRequest.Headers.Add("CustomHeader", "CustomValue");
            });
        }
    });

// Add authentication
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

// Add policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("customPolicy", policy =>
        policy.RequireAuthenticatedUser());
});

var app = builder.Build();

// Enable endpoint routing, required for the reverse proxy
app.UseRouting();
            
// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();;
        
// Register the reverse proxy routes
app.UseEndpoints(endpoints =>
{
    endpoints.MapReverseProxy();
});

app.Run();
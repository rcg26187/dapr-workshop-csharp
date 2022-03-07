// create web-app
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IFineCalculator, HardCodedFineCalculator>();

builder.Services.AddSingleton<VehicleRegistrationService>(_ => 
    new VehicleRegistrationService(DaprClient.CreateInvokeHttpClient(
        "vehicleregistrationservice", "http://localhost:3601")));
        
// Now you need to make sure that Dapr knows this controller and also knows 
//  which pub/sub topics the controller subscribes to. 
// To determine this, Dapr will call your service on a default endpoint
//  to retrieve the subscriptions. 
// To make sure your service handles this request and returns the correct information, 
//  you need to add some statements to the Startup class:

// The AddDapr method adds Dapr integration for ASP.NET MVC.
builder.Services.AddControllers().AddDapr();


var app = builder.Build();

// configure web-app
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// configure routing
app.MapControllers();

// As you saw earlier, Dapr uses the cloud event message-format standard when sending messages over pub/sub. 
// To make sure cloud events are automatically unwrapped, add the following line just after 
//      the call to app.MapControllers() in the file:

app.UseCloudEvents();


//To register every controller that uses pub/sub as a subscriber
//  add a call to app.MapSubscribeHandler() just after the call to app.UseCloudEvent() in the file:
//      By adding this, the /dapr/subscribe endpoint is automatically implemented by Dapr. 
//
//      It will collect all the controller methods that are decorated with the Dapr Topic attribute and
//           return the corresponding subscriptions.
app.MapSubscribeHandler();

// let's go!
app.Run("http://localhost:6001");

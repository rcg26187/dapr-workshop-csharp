namespace FineCollectionService.Controllers;

[ApiController]
[Route("")]
public class CollectionController : ControllerBase
{
    private static string? _fineCalculatorLicenseKey = null;
    private readonly ILogger<CollectionController> _logger;
    private readonly IFineCalculator _fineCalculator;
    private readonly VehicleRegistrationService _vehicleRegistrationService;

    public CollectionController(IConfiguration config, ILogger<CollectionController> logger,
        IFineCalculator fineCalculator, VehicleRegistrationService vehicleRegistrationService)
    {
        _logger = logger;
        _fineCalculator = fineCalculator;
        _vehicleRegistrationService = vehicleRegistrationService;

        // set finecalculator component license-key
        if (_fineCalculatorLicenseKey == null)
        {
            _fineCalculatorLicenseKey = config.GetValue<string>("fineCalculatorLicenseKey");
        }
    }

    // The Dapr ASP.NET Core integration library offers an elegant way of linking an 
    //      ASP.NET Core WebAPI method to a pub/sub topic. 
    // For every message sent to that topic, the WebAPI method is invoked and
    //      the payload of the message is delivered as request body. 
    // You don't have to poll for messages on the message broker.
    // 
    // [Topic("pubsub", "speedingviolations")]

    [Route("collectfine")]
    [Topic("pubsub", "speedingviolations")]  // auto-subscribe to topic
    public async Task<ActionResult> CollectFine(SpeedingViolation speedingViolation)
    {
        decimal fine = _fineCalculator.CalculateFine(_fineCalculatorLicenseKey!, speedingViolation.ViolationInKmh);

        // get owner info
        var vehicleInfo = await _vehicleRegistrationService.GetVehicleInfo(speedingViolation.VehicleId);

        // log fine
        string fineString = fine == 0 ? "tbd by the prosecutor" : $"{fine} Euro";
        _logger.LogInformation($"Sent speeding ticket to {vehicleInfo.OwnerName}. " +
            $"Road: {speedingViolation.RoadId}, Licensenumber: {speedingViolation.VehicleId}, " +
            $"Vehicle: {vehicleInfo.Brand} {vehicleInfo.Model}, " +
            $"Violation: {speedingViolation.ViolationInKmh} Km/h, Fine: {fineString}, " +
            $"On: {speedingViolation.Timestamp.ToString("dd-MM-yyyy")} " +
            $"at {speedingViolation.Timestamp.ToString("hh:mm:ss")}.");

        // send fine by email
        // TODO

        return Ok();
    }

/*
    // Subscribing to pub/sub events the programmatic way. 
    // Dapr will call your service on the well known endpoint /dapr/subscribe 
    // to retrieve the subscriptions for that service. 
    // We will implement this endpoint and return the subscription for the speedingviolations topic.
    
    [Route("/dapr/subscribe")]
    [HttpGet()]
    public object Subscribe()
    {
        return new object[]
        {
        new
        {
            pubsubname = "pubsub",
            topic = "speedingviolations",
            route = "/collectfine"
        }
        };
    }
    */
}
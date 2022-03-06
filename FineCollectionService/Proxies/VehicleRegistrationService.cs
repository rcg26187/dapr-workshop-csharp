namespace FineCollectionService.Proxies;

public class VehicleRegistrationService
{
    private HttpClient _httpClient;

    public VehicleRegistrationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<VehicleInfo> GetVehicleInfo(string licenseNumber)
    {
        // http://localhost:<daprPort>/v1.0/invoke/<appId>/method/<method-name>

        // port 3601 is the dapr port assigned to the FineCollectionService side-car
        // this means the FineCollectionService call its own side-car to resolve the 
        // vehicle registration service based on appid=vehicleregistrationservice
        return await _httpClient.GetFromJsonAsync<VehicleInfo>(
            $"/vehicleinfo/{licenseNumber}"
        );
    }
}

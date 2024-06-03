using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


public class TagApiDataService
{
    private readonly HttpClient _httpClient;

    public Dictionary<string, object> CurrentData { get; private set; }

    public event Action OnDataUpdated;

    public TagApiDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        CurrentData = new Dictionary<string, object>();
    }

    public async Task FetchDataAsync()
    {
        var response = await _httpClient.GetAsync("http://163.243.48.68:5003/Logix/read-tags");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            CurrentData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            OnDataUpdated?.Invoke();
        }
        else
        {
            // Handle errors here
        }
    }
}



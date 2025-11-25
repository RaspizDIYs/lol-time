using System.IO;
using System.Text.Json;
using LolTime.App.Models;

namespace LolTime.App.Services;

public class DataService
{
    private const string FileName = "loltime_data.json";
    private readonly string _filePath;
    private readonly object _lock = new();

    public AppData Data { get; private set; }

    public DataService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "LolTime");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, FileName);
        
        Data = LoadData();
    }

    private AppData LoadData()
    {
        if (!File.Exists(_filePath))
            return new AppData();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
        }
        catch
        {
            return new AppData();
        }
    }

    public void SaveData()
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    public void AddSession(Session session)
    {
        lock (_lock)
        {
            Data.Sessions.Add(session);
            SaveData();
        }
    }

    public List<Session> GetSessionsSnapshot()
    {
        lock (_lock)
        {
            return new List<Session>(Data.Sessions);
        }
    }
}

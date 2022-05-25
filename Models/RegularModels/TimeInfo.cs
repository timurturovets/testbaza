using System.ComponentModel.DataAnnotations;

namespace TestBaza.Models.RegularModels;

public class TimeInfo
{
    public TimeInfo()
    {
    }

    public TimeInfo(bool isTimeLimited, int seconds)
    {
        IsTimeLimited = isTimeLimited;
        Hours = seconds / 3600;
        Minutes = seconds % 3600 / 60;
        Seconds = seconds % 60;
    }

    public bool IsTimeLimited { get; set; }

    [Range(0, 48)] public int Hours { get; set; }

    [Range(0, 59)] public int Minutes { get; set; }

    [Range(0, 59)] public int Seconds { get; set; }

    public int ConvertToSeconds()
    {
        return Hours * 3600 + Minutes * 60 + Seconds;
    }
}
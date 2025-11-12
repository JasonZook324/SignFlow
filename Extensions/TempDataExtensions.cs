using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SignFlow;

public static class TempDataExtensions
{
    public static void Success(this ITempDataDictionary tempData, string message)
        => tempData["StatusMessage"] = message;

    public static void Error(this ITempDataDictionary tempData, string message)
        => tempData["ErrorMessage"] = message;

    public static void Info(this ITempDataDictionary tempData, string message)
        => tempData["InfoMessage"] = message;

    public static void Warning(this ITempDataDictionary tempData, string message)
        => tempData["WarningMessage"] = message;
}

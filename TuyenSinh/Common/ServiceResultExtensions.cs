using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TuyenSinh.Common
{
    public static class ServiceResultExtensions
    {
        public static void SetNotification(this ServiceResult result, ITempDataDictionary tempData)
        {
            if (result.Success)
            {
                tempData["Success"] = result.Message;
            }
            else
            {
                tempData["Error"] = result.Message;
            }
        }
    }
}

namespace TuyenSinh.Common
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult Ok(string message = "Thành công")
        {
            return new ServiceResult { Success = true, Message = message };
        }

        public static ServiceResult Fail(string message)
        {
            return new ServiceResult { Success = false, Message = message };
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "Thành công")
        {
            return new ServiceResult<T> { Success = true, Message = message, Data = data };
        }

        public static new ServiceResult<T> Fail(string message)
        {
            return new ServiceResult<T> { Success = false, Message = message, Data = default };
        }
    }
}

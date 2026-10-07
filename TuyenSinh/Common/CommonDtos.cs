namespace TuyenSinh.Common
{
    public sealed record ImportResultData(int SuccessCount, int SkipCount, int ErrorCount = 0);

    public sealed record FileDownloadDto(
        byte[] Content,
        string FileName,
        string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}

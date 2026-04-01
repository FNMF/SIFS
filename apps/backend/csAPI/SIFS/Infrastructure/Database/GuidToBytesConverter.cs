using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SIFS.Infrastructure.Database
{
    public class GuidToBytesConverter : ValueConverter<Guid, byte[]>
    {
        public GuidToBytesConverter()
            : base(
                guid => guid.ToByteArray(),  // 写入数据库
                bytes => new Guid(bytes)     // 从数据库读取
            )
        { }
    }
}

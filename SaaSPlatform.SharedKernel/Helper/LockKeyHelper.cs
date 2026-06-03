// CoreKit.SharedKernel/Helpers/LockKeyHelper.cs
namespace CoreKit.SharedKernel.Helpers;

public static class LockKeyHelper
{
    public static long GuidToLockKey(Guid guid)
    {
        var bytes = guid.ToByteArray();
        // Use unsigned halves to avoid sign/endian issues
        uint high = BitConverter.ToUInt32(bytes, 0);
        uint low = BitConverter.ToUInt32(bytes, 4);
        // Combine into a signed long (safe for pg_advisory_lock)
        return ((long)high << 32) | (long)low;
    }
}
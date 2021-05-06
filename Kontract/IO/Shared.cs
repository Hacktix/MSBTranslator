using System.IO;

/**
 * Author: IcySon55/Kuriimu on GitHub
 * https://github.com/IcySon55/Kuriimu
 */

namespace Kontract.IO
{
    public enum ByteOrder : ushort
    {
        LittleEndian = 0xFEFF,
        BigEndian = 0xFFFE
    }
}
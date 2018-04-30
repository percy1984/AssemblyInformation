using System;
using System.IO;

namespace AssemblyInformation
{
    internal static class Platform
    {
        /// <summary>
        /// PE header starts with "PE\0\0" = 0x50 0x45 0x00 0x00 (little endian).
        /// </summary>
        private const int PeHeaderLittleEndian = 0x00004550;

        public static bool IsRunningAs64Bit => Environment.Is64BitProcess;

        public static MachineType GetDllMachineType(string dllPath)
        {
            // see http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            // offset to PE header is always at 0x3C
            // PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00
            // followed by 2-byte machine type field (see document above for enum)
            using (var fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read))
            {
                MachineType machineType;
                using (var br = new BinaryReader(fs))
                {
                    fs.Seek(0x3c, SeekOrigin.Begin);
                    var peOffset = br.ReadInt32();
                    fs.Seek(peOffset, SeekOrigin.Begin);
                    var peHead = br.ReadUInt32();
                    if (peHead != PeHeaderLittleEndian)
                    {
                        throw new BadImageFormatException("Unable to determine the assembly's type. Can't find PE header");
                    }

                    machineType = (MachineType)br.ReadUInt16();
                }

                return machineType;
            }
        }

        /// <summary>
        /// Checks if assembly is 64-bit.
        /// </summary>
        /// <param name="dllPath">Path to the assembly.</param>
        /// <returns>
        /// Returns <c>true</c> if the dll is 64-bit, <c>false</c> if 32-bit, and <c>null</c> if unknown.
        /// </returns>
        public static bool? Is64BitAssembly(string dllPath)
        {
            switch (GetDllMachineType(dllPath))
            {
                case MachineType.IMAGE_FILE_MACHINE_AMD64:
                case MachineType.IMAGE_FILE_MACHINE_IA64:
                    return true;
                case MachineType.IMAGE_FILE_MACHINE_I386:
                    return false;
                default:
                    return null;
            }
        }
    }
}

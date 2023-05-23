using Ryujinx.Memory;
using System;

namespace Ryujinx.Cpu.AppleHv
{
    static class HvVm
    {
        // This alignment allows us to use larger blocks on the page table.
        private const ulong AsIpaAlignment = 1UL << 30;

        private static int _addressSpaces;
        private static HvIpaAllocator _ipaAllocator;
        private static object _lock = new object();

        public static (ulong, HvIpaAllocator) CreateAddressSpace(MemoryBlock block)
        {
            HvIpaAllocator ipaAllocator;

            lock (_lock)
            {
                if (++_addressSpaces == 1)
                {
                    HvApi.hv_vm_create(IntPtr.Zero).ThrowOnError();
                    _ipaAllocator = ipaAllocator = new HvIpaAllocator();
                }
                else
                {
                    ipaAllocator = _ipaAllocator;
                }
            }

            ulong baseAddress;

            lock (ipaAllocator)
            {
                baseAddress = ipaAllocator.Allocate(block.Size, AsIpaAlignment);
            }

            var rwx = hv_memory_flags_t.HV_MEMORY_READ | hv_memory_flags_t.HV_MEMORY_WRITE | hv_memory_flags_t.HV_MEMORY_EXEC;

            HvApi.hv_vm_map((ulong)block.Pointer, baseAddress, block.Size, rwx).ThrowOnError();

            return (baseAddress, ipaAllocator);
        }

        public static void DestroyAddressSpace(ulong address, ulong size)
        {
            HvApi.hv_vm_unmap(address, size);

            HvIpaAllocator ipaAllocator;

            lock (_lock)
            {
                if (--_addressSpaces == 0)
                {
                    HvApi.hv_vm_destroy().ThrowOnError();
                }

                ipaAllocator = _ipaAllocator;
            }

            lock (ipaAllocator)
            {
                ipaAllocator.Free(address, size);
            }
        }
    }
}
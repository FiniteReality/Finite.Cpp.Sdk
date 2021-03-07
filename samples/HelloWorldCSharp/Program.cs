using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HelloWorldCSharp
{
    public static class Program
    {
        public static unsafe void Main(string[] args)
        {
            var ptr = (IntPtr)Native.GetHelloString();
            var message = Marshal.PtrToStringUTF8(ptr);

            Console.WriteLine($"{message}\nThis is: C# application");
        }
    }

    internal static unsafe class Native
    {
        [DllImport("HelloWorldLibraryC",
            EntryPoint = "get_hello_string",
            ExactSpelling = true,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GetHelloString();
    }
}

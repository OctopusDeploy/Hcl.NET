using System;

namespace Octopus.Ocl
{
    /// <summary>
    /// This class is not threadsafe while it is being used to serialize/deserialize
    /// </summary>
    public class OclSerializerOptions
    {
        public char IndentChar { get; set; } = ' ';
        public int IndentDepth { get; set; } = 4;
        public string DefaultHeredocTag { get; set; } = "EOT";
    }
}
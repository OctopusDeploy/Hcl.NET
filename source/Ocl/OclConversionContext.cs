using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Ocl.Converters;

namespace Octopus.Ocl
{
    public class OclConversionContext
    {
        readonly IReadOnlyList<IOclConverter> converters;

        internal OclConversionContext(OclSerializerOptions options)
        {
            converters = options
                .Converters
                .Concat(
                    new IOclConverter[]
                    {
                        new DefaultAttributeOclConverter(),
                        new DefaultCollectionOclConverter(),
                        new DefaultBlockOclConverter()
                    })
                .ToArray();
        }

        public IEnumerable<IOclElement> ToElements(string name, object? value)
        {
            if (value == null)
                return new IOclElement[0];

            return GetConverterFor(value.GetType())
                .ToElements(this, name, value);
        }

        public object? FromElement(Type type, IOclElement element, object? getCurrentValue)
            => GetConverterFor(type)
                .FromElement(this, type, element, getCurrentValue);

        public IOclConverter GetConverterFor(Type type)
        {
            foreach (var converter in converters)
                if (converter.CanConvert(type))
                    return converter;

            throw new Exception("Could not find a converter for " + type.FullName);
        }
    }
}
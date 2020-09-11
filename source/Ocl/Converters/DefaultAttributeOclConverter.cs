using System;
using System.Collections.Generic;

namespace Octopus.Ocl.Converters
{
    public class DefaultAttributeOclConverter : OclConverter
    {
        public override bool CanConvert(Type type)
            => OclAttribute.IsSupportedValueType(type);

        protected override IOclElement ConvertInternal(OclConversionContext context, string name, object obj)
             => new OclAttribute(GetName(name, obj), obj);
    }
}
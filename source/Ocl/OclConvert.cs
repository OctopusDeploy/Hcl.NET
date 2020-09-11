using System;
using System.Text;
using Octopus.Ocl.Converters;

namespace Octopus.Ocl
{
    public class OclConvert
    {
        public static string Serialize(OclDocument document, OclSerializerOptions? options = null)
        {
            options ??= new OclSerializerOptions();
            var sb = new StringBuilder();
            // ReSharper disable once ConvertToUsingDeclaration
            using (var writer = new OclWriter(sb, options))
            {
                writer.Write(document);
            }

            return sb.ToString();
        }

        public static string Serialize(object? obj, OclSerializerOptions? options = null)
            => Serialize(ToOclDocument(obj, options));

        public static OclDocument ToOclDocument(object? obj, OclSerializerOptions? options = null)
            => new OclDocumentOclConverter().Convert(obj, new OclConversionContext(options ?? new OclSerializerOptions()));

        public static T Deserialize<T>(OclDocument document, OclSerializerOptions? options = null)
            where T : notnull
        {
            var context = new OclConversionContext(options ?? new OclSerializerOptions());
            var result = context.FromElement(typeof(T), document, () => null);
            if (result == null)
                throw new OclException("Document conversion resulted in null, which is not valid");
            return (T)result;
        }
    }
}
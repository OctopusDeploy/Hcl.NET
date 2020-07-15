using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Octopus.Hcl
{
    /// <summary>
    /// This class is not threadsafe
    /// </summary>
    public class HclWriter : IDisposable
    {
        private readonly TextWriter writer;
        private int currentIndent;
        private bool isFirstLine = true;
        private bool lastWrittenWasBlock;

        public HclWriter(StringBuilder sb, HclSerializerOptions? options = null)
            : this(new StringWriter(sb), options)
        {
        }

        public HclWriter(TextWriter writer, HclSerializerOptions? options = null)
        {
            this.writer = writer;
            Options = options ?? new HclSerializerOptions();
        }

        public HclSerializerOptions Options { get; }

        public void Write(IHElement element)
        {
            switch (element)
            {
                case HAttribute attribute:
                    Write(attribute);
                    return;
                case HBlock block:
                    Write(block);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(element.GetType().Name);
            }
        }

        public void Write(HBody body)
        {
            foreach (var item in body)
                Write(item);
        }

        public void Write(HAttribute attribute)
        {
            WriteNextLine();
            WriteIndent();
            WriteIdentifier(attribute.Name);
            writer.Write(" = ");
            WriteValue(attribute.Value);
        }

        public void Write(HBlock block)
        {
            if (!isFirstLine && !lastWrittenWasBlock)
                writer.WriteLine();
            WriteNextLine();
            WriteIndent();
            WriteIdentifier(block.Name);

            foreach (var label in block.Labels)
            {
                writer.Write(' ');
                WriteSingleLineStringLiteral(label);
            }

            writer.Write(" {");

            currentIndent += Options.IndentDepth;
            foreach (var child in block)
                Write(child);
            currentIndent -= Options.IndentDepth;

            writer.WriteLine();

            WriteIndent();
            writer.Write("}");

            lastWrittenWasBlock = true;
        }

        private void WriteNextLine()
        {
            if (!isFirstLine)
                writer.WriteLine();
            if (lastWrittenWasBlock)
                writer.WriteLine();
            isFirstLine = false;
            lastWrittenWasBlock = false;
        }

        private void WriteIndent()
        {
            for (var x = 0; x < currentIndent; x++)
                writer.Write(Options.IndentChar);
        }

        private void WriteIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier cannot be blank");

            if (char.IsDigit(identifier[0]))
                writer.Write('_');

            foreach (var c in identifier)
                if (
                    char.IsLetterOrDigit(c) ||
                    c == '_' ||
                    c == '-'
                )
                    writer.Write(c);
                else
                    writer.Write('_');
        }

        private void WriteValue(object? value)
        {
            if (value == null)
            {
                writer.Write("null");
                return;
            }

            switch (value)
            {
                case bool b:
                    writer.Write(b ? "true" : "false");
                    return;
                case byte b:
                    writer.Write(b);
                    return;
                case ushort s:
                    writer.Write(s);
                    return;
                case short s:
                    writer.Write(s);
                    return;
                case uint i:
                    writer.Write(i);
                    return;
                case int i:
                    writer.Write(i);
                    return;
                case ulong l:
                    writer.Write(l);
                    return;
                case long l:
                    writer.Write(l);
                    return;
                case float f:
                    writer.Write(f);
                    return;
                case double d:
                    writer.Write(d);
                    return;
                case decimal d:
                    writer.Write(d);
                    return;
                case char c:
                    WriteSingleLineStringLiteral(c.ToString());
                    return;
                case string s:
                    var literal = HclStringLiteral.Create(s);
                    literal.HeredocIdentifier = Options.DefaultHeredocIdentifier;
                    WriteValue(literal);
                    return;
                case HclStringLiteral s:
                    WriteValue(s);
                    return;
            }

            if (value is IEnumerable enumerable)
            {
                writer.Write('[');
                var isFirst = true;
                foreach (var item in enumerable)
                {
                    if (!isFirst)
                        writer.Write(", ");
                    isFirst = false;
                    WriteValue(item);
                }

                writer.Write(']');
                return;
            }

            throw new InvalidOperationException($"The type {value.GetType().FullName} is not a valid attribute value and can not be serialized");
        }

        private void WriteValue(HclStringLiteral literal)
        {
            if (literal.Format == HclStringLiteralFormat.SingleLine)
            {
                WriteSingleLineStringLiteral(literal.Value);
                return;
            }

            var isIndented = literal.Format == HclStringLiteralFormat.IndentedHeredoc;

            writer.Write("<<");
            if (isIndented)
                writer.Write("-");
            writer.WriteLine(literal.HeredocIdentifier);

            foreach (var line in literal.Value.Split('\n'))
            {
                if (isIndented)
                    WriteIndent();
                writer.WriteLine(line.TrimEnd('\r'));
            }

            if (isIndented)
                WriteIndent();
            writer.Write(literal.HeredocIdentifier);
        }

        private void WriteSingleLineStringLiteral(string s)
        {
            writer.Write('"');
            s = s.Replace(@"\", @"\\")
                .Replace("\r", @"\r")
                .Replace("\n", @"\n")
                .Replace("\t", @"\t")
                .Replace("\"", @"\""");
            writer.Write(s);
            writer.Write('"');
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
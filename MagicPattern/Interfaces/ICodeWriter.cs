using System.IO;

namespace MagicPattern.Core
{
    public interface ICodeWriter
    {
        string GetTypeName(ClassInfo type, IGeneratorConfig config);
        void WriteClass(TextWriter sw, ClassInfo type, bool useFieldProperty);
        void WriteNamespaceStart(IGeneratorConfig config, TextWriter sw, bool root);
        void WriteNamespaceEnd(TextWriter sw, bool root);
        void WriteServiceRequest(TextWriter sw, ClassInfo type);
        void WriteInterfaceServiceRequest(TextWriter sw, ClassInfo type);
        void WriteFromDtoMapper(TextWriter sw, ClassInfo type);
    }
}

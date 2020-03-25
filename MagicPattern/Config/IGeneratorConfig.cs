using System.IO;

namespace MagicPattern.Core
{
    public interface IGeneratorConfig
    {
        string Example { get; set; }
        string Namespace { get; set; }
        string MainClass { get; set; }
        string PropertyAttribute { get; set; }
        bool AlwaysUseNullableValues { get; set; }
        bool AlwaysUsePublicValues { get; set; }
        ICodeWriter CodeWriter { get; set; }
    }
}

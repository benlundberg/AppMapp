using System;
using System.IO;

namespace MagicPattern.Core
{
    public class CSharpCodeWriter : ICodeWriter
    {
        public string GetTypeName(ClassInfo type, IGeneratorConfig config)
        {
            switch (type.Type)
            {
                case JsonTypeEnum.Anything: return "object";
                case JsonTypeEnum.Array: return "IList<" + GetTypeName(type.InternalType, config) + ">";
                case JsonTypeEnum.Dictionary: return "Dictionary<string, " + GetTypeName(type.InternalType, config) + ">";
                case JsonTypeEnum.Boolean: return "bool";
                case JsonTypeEnum.Float: return "double";
                case JsonTypeEnum.Integer: return "int";
                case JsonTypeEnum.Long: return "long";
                case JsonTypeEnum.Date: return "DateTime";
                case JsonTypeEnum.NonConstrained: return "object";
                case JsonTypeEnum.NullableBoolean: return "bool?";
                case JsonTypeEnum.NullableFloat: return "double?";
                case JsonTypeEnum.NullableInteger: return "int?";
                case JsonTypeEnum.NullableLong: return "long?";
                case JsonTypeEnum.NullableDate: return "DateTime?";
                case JsonTypeEnum.NullableSomething: return "object";
                case JsonTypeEnum.Object: return type.AssignedName;
                case JsonTypeEnum.String: return "string";
                default: throw new NotSupportedException("Unsupported json type");
            }
        }

        public void WriteClass(TextWriter sw, ClassInfo type, bool useFieldProperty)
        {
            var visibility = type.InternalVisibility || useFieldProperty ? "internal" : "public";

            sw.WriteLine("    {0} class {1}", visibility, type.AssignedName);
            sw.WriteLine("    {");

            WriteClassMembers(sw, type, useFieldProperty);

            sw.WriteLine("    }");
            sw.WriteLine();
        }

        private void WriteClassMembers(TextWriter sw, ClassInfo type, bool useFieldProperty)
        {
            foreach (var field in type.Fields)
            {
                if (!string.IsNullOrEmpty(type.Documentation))
                {
                    sw.WriteLine(prefix + "/// <summary>");
                    sw.WriteLine(prefix + "/// " + field.ClassInfo.Documentation);
                    sw.WriteLine(prefix + "/// </summary>");
                }

                if (type.UseJsonProperty && useFieldProperty)
                {
                    sw.WriteLine(prefix + "[JsonProperty" + "(\"{0}\")]", field.JsonMemberName);
                }

                sw.WriteLine(prefix + "public {0} {1} {{ get; set; }}", field.ClassInfo.TypeName, field.MemberName);

                sw.WriteLine();
            }
        }

        public void WriteNamespaceEnd(TextWriter sw, bool root)
        {
            sw.WriteLine("}");
        }

        public void WriteNamespaceStart(IGeneratorConfig config, TextWriter sw, bool root)
        {
            sw.WriteLine();
            sw.WriteLine("namespace {0}", config.Namespace);
            sw.WriteLine("{");
        }

        public void WriteServiceRequest(TextWriter sw, ClassInfo type)
        {
            sw.WriteLine("    public class Get{0}Service : BaseService, IGet{0}Service", type.AssignedName.Replace("Dto", ""));
            sw.WriteLine("    {");

            if (type.UseOfflineSupport)
            {
                sw.WriteLine(prefix + "public async Task<{0}> ExecuteAsync(bool isConnected)", type.TypeName.Replace("Dto", ""));
            }
            else
            {
                sw.WriteLine(prefix + "public async Task<{0}> ExecuteAsync()", type.TypeName.Replace("Dto", ""));
            }

            sw.WriteLine(prefix + "{");

            if (type.UseMemoryChache)
            {
                WriteReadFromMemory(sw, type);
            }

            if (type.UseOfflineSupport)
            {
                WriteReadFromDisk(sw, type);
            }

            sw.WriteLine(prefix2 + "// string url = ;");
            sw.WriteLine(prefix2 + "{0} response = await MakeRequestAsync<{0}>(url, HttpMethod.Get);", type.TypeName);
            sw.WriteLine();
            sw.WriteLine(prefix2 + "{0} result = response.To{0}();", type.TypeName.Replace("Dto", ""));
            sw.WriteLine();

            if (type.UseMemoryChache)
            {
                WriteSaveToMemory(sw, type);
                sw.WriteLine();
            }

            if (type.UseOfflineSupport)
            {
                WriteSaveToDisk(sw, type);
                sw.WriteLine();
            }

            sw.WriteLine(prefix2 + "return res;");
            sw.WriteLine(prefix + "}");
            sw.WriteLine("    }");
        }

        public void WriteInterfaceServiceRequest(TextWriter sw, ClassInfo type)
        {
            sw.WriteLine("    public interface IGet{0}Service", type.AssignedName.Replace("Dto", ""));
            sw.WriteLine("    {");

            if (type.UseOfflineSupport)
            {
                sw.WriteLine(prefix + "Task<{0}> ExecuteAsync(bool isConnected);", type.TypeName.Replace("Dto", ""));
            }
            else
            {
                sw.WriteLine(prefix + "Task<{0}> ExecuteAsync();", type.TypeName.Replace("Dto", ""));
            }

            sw.WriteLine("    }");
            sw.WriteLine();
        }

        private void WriteReadFromMemory(TextWriter sw, ClassInfo type)
        {
            sw.WriteLine(prefix2 + "var localMemory = await MemoryRepository.Current.LoadAsync<{0}>(MemoryKey.{0});", type.TypeName.Replace("Dto", ""));
            sw.WriteLine();
            sw.WriteLine(prefix2 + "if (localMemory?.Any() == true)");
            sw.WriteLine(prefix2 + "{");
            sw.WriteLine(prefix3 + "return localMemory;");
            sw.WriteLine(prefix2 + "}");
            sw.WriteLine();
        }

        private void WriteReadFromDisk(TextWriter sw, ClassInfo type)
        {
            sw.WriteLine(prefix2 + "if (!isConnected)");
            sw.WriteLine(prefix2 + "{");
            if (type.RepositoryType == RepositoryType.Storage)
            {
                sw.WriteLine(prefix3 + "var local = await StorageRepository.Current.LoadAsync<{0}>(StorageKey.{0});", type.TypeName.Replace("Dto", ""));
            }
            else if (type.RepositoryType == RepositoryType.Database)
            {
                sw.WriteLine(prefix3 + "var local = await DatabaseRepository.Current.LoadAsync<{0}>(/*param*/);", type.TypeName.Replace("Dto", ""));
            }

            sw.WriteLine();
            sw.WriteLine(prefix3 + "if (local?.Any() == true)");
            sw.WriteLine(prefix3 + "{");
            sw.WriteLine(prefix4 + "return local;");
            sw.WriteLine(prefix3 + "}");
            sw.WriteLine(prefix2 + "}");
            sw.WriteLine();
        }

        private void WriteSaveToMemory(TextWriter sw, ClassInfo type)
        {
            sw.WriteLine(prefix2 + "await MemoryRepository.Current.SaveAsync(MemoryKey.{0}, result);", type.TypeName.Replace("Dto", ""));
        }

        private void WriteSaveToDisk(TextWriter sw, ClassInfo type)
        {
            if (type.RepositoryType == RepositoryType.Storage)
            {
                sw.WriteLine(prefix2 + "await StorageRepository.Current.SaveAsync(StorageKey.{0}, result);", type.TypeName.Replace("Dto", ""));
            }
            else if (type.RepositoryType == RepositoryType.Database)
            {
                sw.WriteLine(prefix2 + "await DatabaseRepository.Current.InsertOrReplaceAsync(result);", type.TypeName.Replace("Dto", ""));
            }
        }

        public void WriteFromDtoMapper(TextWriter sw, ClassInfo type)
        {
            sw.WriteLine(prefix2 + "public static {0} To{0}(this {1} source)", type.TypeName.Replace("Dto", ""), type.TypeName);
            sw.WriteLine(prefix2 + "{");
            sw.WriteLine();
            sw.WriteLine(prefix3 + "// Map here");
            sw.WriteLine();
            sw.WriteLine(prefix2 + "}");
        }

        private const string prefix = "        ";
        private const string prefix2 = prefix + prefix;
        private const string prefix3 = prefix + prefix + prefix;
        private const string prefix4 = prefix + prefix + prefix + prefix;
    }
}

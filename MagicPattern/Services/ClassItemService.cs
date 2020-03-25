using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MagicPattern.Core
{
    public class ClassItemService
    {
        public IList<ClassItem> HandleGeneratedClasses(GeneratorService service)
        {
            var classItems = new List<ClassItem>();

            // Loop through all types and create class text infos
            foreach (var type in service.Types)
            {
                var item = new ClassItem
                {
                    ClassInfo = type
                };

                // Creates the dto object
                using (var sw = new StringWriter())
                {
                    service.CodeWriter.WriteClass(sw, type, useFieldProperty: true);
                    sw.Flush();

                    item.DtoFileInfo = new FileInfoItem
                    {
                        ClassText = sw.ToString(),
                        TargetPath = "Models/Dtos"
                    };
                }

                // Create the service interface if type uses service
                if (type.UseService)
                {
                    using (var sw = new StringWriter())
                    {
                        service.CodeWriter.WriteInterfaceServiceRequest(sw, type);
                        sw.Flush();

                        item.InterfaceServiceFileInfo = new FileInfoItem
                        {
                            ClassText = sw.ToString(),
                            TargetPath = "Interfaces"
                        };
                    }
                }

                // Create the service if type uses service
                if (type.UseService)
                {
                    using (var sw = new StringWriter())
                    {
                        service.CodeWriter.WriteServiceRequest(sw, type);
                        sw.Flush();

                        item.ClassServiceFileInfo = new FileInfoItem
                        {
                            ClassText = sw.ToString(),
                            TargetPath = "Services"
                        };
                    }
                }

                // Create model class if mapper is used
                if (type.UseMapping)
                {
                    using (var sw = new StringWriter())
                    {
                        service.CodeWriter.WriteClass(sw, type, useFieldProperty: false);
                        sw.Flush();

                        item.ModelObjectFileInfo = new FileInfoItem
                        {
                            ClassText = sw.ToString(),
                            TargetPath = "Models"
                        };
                    }

                }

                // Create mapper if mapper is used
                if (type.UseMapping)
                {
                    using (var sw = new StringWriter())
                    {
                        service.CodeWriter.WriteFromDtoMapper(sw, type);
                        sw.Flush();

                        item.MapperFileInfo = new FileInfoItem
                        {
                            ClassText = sw.ToString(),
                            TargetPath = "Mapper"
                        };
                    }

                }

                classItems.Add(item);
            }

            return classItems;
        }
    }
}

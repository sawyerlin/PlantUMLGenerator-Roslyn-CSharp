using System;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlantUMLGenerator.Core.Test
{
    [TestClass]
    public class UnitTestController
    {
        [TestMethod]
        public void TestGenerateClass()
        {
            var input = File.ReadAllText("..\\..\\..\\input\\Test.cs");
            var output = new StringBuilder();
            using (var writer = new StringWriter(output))
            {
                var gen = new ControllerGenerator(writer);
                gen.Generate(new [] {input});
                Console.Write(output);
            }
        }

        [TestMethod]
        public void TestGenerateNode()
        {
            var rootFolder = @"C:\Dev\Innova\Backend\InnovaApiSoap\Jellies\Controllers";
            var rootNameSpace = "InnovaApiSoap.Controller";

            var directories = Directory.GetDirectories(rootFolder);

            var output = new StringBuilder();
            using (var writer = new StringWriter(output))
            {
                var gen = new ControllerGenerator(writer, rootNameSpace);
                var query = from directory in directories
                    from file in Directory.GetFiles(directory)
                    select File.ReadAllText(file);
                gen.Generate(query.ToArray());
                string outputPath = "C:\\Dev\\PlantUmlGenerator\\test.plantuml";

                using (var stream = new StreamWriter(File.Create(outputPath)))
                {
                   stream.Write(output); 
                }
            }
        }
    }
}

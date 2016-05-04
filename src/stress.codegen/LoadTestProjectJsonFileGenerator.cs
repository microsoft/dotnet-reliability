using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    public class LoadTestProjectJsonFileGenerator : ISourceFileGenerator
    {
        public void GenerateSourceFile(LoadTestInfo loadTest)
        {
            var loadTestRefInfo = loadTest.PackageInfo;

            var srcFilePath = Path.Combine(loadTest.SourceDirectory, "project.json");

            loadTestRefInfo.ToFile(srcFilePath);

            loadTest.SourceFiles.Add(new SourceFileInfo(srcFilePath, SourceFileAction.None));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeScriptAutomation.CodeGeneratorStrategies
{
    internal abstract class BaseCodeGenerator
    {
        abstract public string Generate(RecipeConfiguration config);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeScriptAutomation.CodeGeneratorStrategies
{
    internal interface IRecipeCodeGenerator
    {
        string Generate(RecipeConfiguration config);
    }
}

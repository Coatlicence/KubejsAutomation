using KubeScriptAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace KubeScriptAutomation
{
    public readonly struct MachineType(string name, uint liquidCount, uint itemCount, uint liquidOut, uint itemOut)
    {
        public string Name { get; } = name;

        public uint ItemCount { get; } = itemCount;

        public uint ItemOut { get; } = itemOut;

        public uint LiquidCount { get; } = liquidCount;

        public uint LiquidOut { get; } = liquidOut;

    }
}




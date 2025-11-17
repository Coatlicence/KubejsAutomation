using KubeScriptAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace KubeScriptAutomation
{
    public struct MachineType
    {
        public MachineType(string name, uint liquidCount, uint itemCount, uint liquidOut, uint itemOut)
        {
            Name = name;
            LiquidCount = liquidCount;
            ItemCount = itemCount;
            LiquidOut = liquidOut;
            ItemOut = itemOut;
        }

        public string Name { get; }

        public uint ItemCount { get; }

        public uint ItemOut { get; }

        public uint LiquidCount { get; }

        public uint LiquidOut { get; }

    }
}




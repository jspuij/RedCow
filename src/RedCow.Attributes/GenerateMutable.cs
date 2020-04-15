using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace RedCow
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute("RedCow.Generators.MutableClassGenerator, RedCow.Generators")]
    [Conditional("CodeGeneration")]
    public class GenerateMutable : Attribute
    {
    }
}

using System;
using Microsoft.Extensions.Logging;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests
{
    public class TestBuilder : ObjectBuilder
    {
        public TestBuilder(ILogger<TestBuilder> logger) : base(logger) { }

        protected override Type GetTypeByName(string typeName)
        {
            if (typeName == "Test_Nested") return typeof(Test_Nested);
            if (typeName == "Test_Object") return typeof(Test_Object);

            throw new NotSupportedException($"Type '{typeName}' is not supported in TestBuilder.");
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace ReactiveUI.Fody.Analyzer.Test
{
    [TestClass]
    public class ReactiveObjectAnalyzerTest : DiagnosticVerifier
    {
        [TestMethod]
        public void Test1()
        {
            var test = @"";
            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void ShouldGiveAnErrorWhenClassDoesNotImplement()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    namespace ConsoleApplication1
    {
        public class TypeName
        {   
            [Reactive] public string Prop { get; set; }
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = "RUI_0001",
                Message = $"Type 'TypeName' does not implement IReactiveObject",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 15, 14),
                    }
            };
            VerifyCSharpDiagnostic(test, expected);

        }

        [TestMethod]
        public void ShouldNotGiveAnErrorWhenClassInherits()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    namespace ConsoleApplication1
    {
        public class TypeName : ReactiveObject
        {   
            [Reactive] public string Prop { get; set; }
        }
    }";
            VerifyCSharpDiagnostic(test);

        }

        [TestMethod]
        public void ShouldNotGiveAnErrorWhenClassImplements()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    namespace ConsoleApplication1
    {
        public class TypeName : IReactiveObject
        {   
            [Reactive] public string Prop { get; set; }
        }
    }";


            VerifyCSharpDiagnostic(test);

        }


        [TestMethod]
        public void ShouldGiveErrorForNonAutoProperty()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using ReactiveUI;
    using ReactiveUI.Fody.Helpers;

    namespace ConsoleApplication1
    {
        public class TypeName : IReactiveObject
        {   
            [Reactive] public string Prop
            {
                get => _prop;
                set => _prop = value;
            }
            private string _prop;
        }
    }";

            var expected = new DiagnosticResult
            {
                Id = "RUI_0002",
                Message = $"Property 'Prop' on 'TypeName' should be an auto property",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 15, 14),
                    }
            };
            VerifyCSharpDiagnostic(test, expected);

        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReactiveObjectAnalyzer();
        }
    }
}
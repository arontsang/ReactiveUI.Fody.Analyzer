using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReactiveUI.Fody.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReactiveObjectAnalyzer : DiagnosticAnalyzer
    {
        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization



        private static DiagnosticDescriptor InheritanceRule = new DiagnosticDescriptor(
            "RUI_0001",
            "Type must implement IReactiveObject",
            "Type '{0}' does not implement IReactiveObject", 
            "Fody", 
            DiagnosticSeverity.Error, 
            isEnabledByDefault: true, 
            description: "[Reactive] may only be applied to a IReactiveObject");

        private static DiagnosticDescriptor AutoPropertyRule = new DiagnosticDescriptor(
            "RUI_0002",
            "[Reactive] properties should be an auto property",
            "Property '{0}' on '{1}' should be an auto property",
            "Fody",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "[Reactive] properties should be an auto property");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(InheritanceRule, AutoPropertyRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attr = context.ContainingSymbol.GetAttributes().FirstOrDefault(a => a.ApplicationSyntaxReference.Span == context.Node.Span);

            if (attr.AttributeClass.ToDisplayString() != "ReactiveUI.Fody.Helpers.ReactiveAttribute")
                return;


            if (!(context.Node.Parent.Parent is PropertyDeclarationSyntax property))
                return;

            var reactiveObject = context.ContainingSymbol.ContainingType;
            if (!reactiveObject.AllInterfaces.Any(interfaceTypeSymbol => interfaceTypeSymbol.ToDisplayString() == "ReactiveUI.IReactiveObject"))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        InheritanceRule,
                        context.Node.GetLocation(), 
                        reactiveObject.Name)
                    );
            }



            if (HasBackingField(property))
            {
                var propertySymbol = context.ContainingSymbol;
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        AutoPropertyRule,
                        context.Node.GetLocation(),
                        propertySymbol.Name, propertySymbol.ContainingType.Name)
                );
            }
        }

        internal static bool HasBackingField(PropertyDeclarationSyntax property)
        {
            var getter = property.AccessorList?.Accessors.FirstOrDefault(x => x.IsKind(SyntaxKind.GetAccessorDeclaration));
            var setter = property.AccessorList?.Accessors.FirstOrDefault(x => x.IsKind(SyntaxKind.SetAccessorDeclaration));

            if ((setter?.Body == null || getter?.Body == null))
            {
                return setter?.ExpressionBody != null && getter?.ExpressionBody != null;
            }

            bool setterHasBodyStatements = setter.Body.Statements.Any();
            bool getterHasBodyStatements = getter.Body.Statements.Any();

            return setterHasBodyStatements && getterHasBodyStatements;
        }
    }
}

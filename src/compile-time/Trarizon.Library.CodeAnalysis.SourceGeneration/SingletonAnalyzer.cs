using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.Library.Collections;
using Trarizon.Library.Functional;
using Trarizon.Library.Roslyn.Extensions;
using static Trarizon.Library.CodeAnalysis.SourceGeneration.SingletonGenerator;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class SingletonAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        Descriptors.OnlyClassCanBeSingleton,
        Descriptors.SingletonShouldBeSealed,
        Descriptors.InvalidIdentifier,
        Descriptors.SingletonShouldHaveCorrectCtor,
        Descriptors.InstancePropertyNameAndSingletonProviderNameShouldBeDifferent,
        Descriptors.CallSingletonConstrucotrMauallyIsNotAllowed,
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(context =>
        {
            var compilation = context.Compilation;
            if (compilation.GetTypeByMetadataName(RuntimeAttribute.TypeFullName) is not { } singletonAttributeSymbol) {
                return;
            }

            context.RegisterSymbolAction(context =>
            {
                var symbol = (INamedTypeSymbol)context.Symbol;
                if (!symbol.TryGetAttributeData(singletonAttributeSymbol, out var attrData))
                    return;

                // Singleton type must be a class
                if (!symbol.IsReferenceType || symbol.IsRecord) {
                    context.ReportDiagnostic(
                        Descriptors.OnlyClassCanBeSingleton,
                        symbol.Locations[0]);
                }

                // Singleton type must be sealed,
                // sealed keyword will be auto generated, 
                // so we just avoid abstract and static
                if (symbol.IsAbstract || symbol.IsStatic) {
                    context.ReportDiagnostic(
                        Descriptors.SingletonShouldBeSealed,
                        symbol.Locations[0]);
                }

                var attr = new RuntimeAttribute(attrData);
                var propertyIdentifier = attr.GetInstancePropertyName();
                if (!Utils.IsValidInstancePropertyIdentifier(propertyIdentifier, out var actualPropertyIdentifier)) {
                    context.ReportDiagnostic(
                        Descriptors.InvalidIdentifier,
                        symbol.Locations[0],
                        propertyIdentifier);
                }

                var providerIdentifier = attr.GetSingletonProviderName();
                if (providerIdentifier is not null) {
                    if (providerIdentifier is not null && !Utils.IsValidInstancePropertyIdentifier(providerIdentifier, out _)) {
                        context.ReportDiagnostic(
                            Descriptors.InvalidIdentifier,
                            symbol.Locations[0],
                            providerIdentifier);
                    }
                    if (providerIdentifier == actualPropertyIdentifier) {
                        context.ReportDiagnostic(
                            Descriptors.InstancePropertyNameAndSingletonProviderNameShouldBeDifferent,
                            symbol.Locations[0]);
                    }
                }

                // constructors
                switch (symbol.Constructors) {
                    case []:
                        break;
                    case [var first]:
                        if (first.IsImplicitlyDeclared)
                            break;
                        if (first is not
                            {
                                DeclaredAccessibility: Accessibility.NotApplicable or Accessibility.Private,
                                Parameters.Length: 0
                            }) {
                            // Not private non-param ctor
                            context.ReportDiagnostic(
                                Descriptors.SingletonShouldHaveCorrectCtor,
                                symbol.Locations[0]);
                        }
                        break;
                    default:
                        context.ReportDiagnostic(
                            Descriptors.SingletonShouldHaveCorrectCtor,
                            symbol.Locations[0]);
                        break;
                }


            }, SymbolKind.NamedType);

        });

        context.RegisterOperationAction(
            DoNotCallCtorManually,
            OperationKind.ObjectCreation);
    }

    private void DoNotCallCtorManually(OperationAnalysisContext context)
    {
        if (context.IsGeneratedCode)
            return;

        var operation = (IObjectCreationOperation)context.Operation;

        Optional.OfNotNull(operation.Constructor)
            .Where(ctor => IsSingletonType(ctor.ContainingType))
            .MatchValue(_ => context.ReportDiagnostic(
                Descriptors.CallSingletonConstrucotrMauallyIsNotAllowed,
                operation.Syntax.GetLocation()));
    }

    private bool IsSingletonType(ITypeSymbol? symbol)
    {
        if (symbol is not INamedTypeSymbol type)
            return false;

        if (type.IsValueType || type.IsRecord)
            return false;

        if (type.IsAbstract || type.IsStatic)
            return false;

        return type.GetAttributes()
            .Any(attr => attr.AttributeClass.MatchDisplayString(RuntimeAttribute.TypeFullName));
    }
}

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using Trarizon.Library.CodeAnalysis.SourceGeneration.Literals;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
partial class SingletonGenerator
{
    internal readonly struct RuntimeAttribute(AttributeData attribute)
    {
        public const string TypeFullName = Namespaces.TrarizonGeneration + ".SingletonAttribute";

        public const string DefaultInstancePropertyName = "Instance";
        public const string DefaultSingletonProviderName = "__SingletonProvider";

        public string? GetInstancePropertyName() => attribute
            .GetNamedArgument("InstancePropertyName")
            .Cast<string>();

        public string? GetSingletonProviderName() => attribute
            .GetNamedArgument("SingletonProviderName")
            .Cast<string>();

        public SingletonOptions GetOptions() => attribute
            .GetNamedArgument("Options")
            .Cast<SingletonOptions>();
    }

    [Flags]
    internal enum SingletonOptions
    {
        None = 0,
        /// <summary>
        /// Generate a singleton provider to keep the instance.
        /// </summary>
        /// <remarks>
        /// When use a provider, the instance will be created only when you access the instance property.
        /// Otherwise, once you access the type, instance will be created, and all other static member will all be created
        /// </remarks>
        GenerateProvider = 1,
        /// <summary>
        /// Mark the instance as internal
        /// </summary>
        IsInternalAccessibility = 1 << 1,
    }

    internal static class Descriptors
    {
        public static ImmutableArray<DiagnosticDescriptor> GetAnalyzerDiagnostics() => [CallSingletonConstrucotrMauallyIsNotAllowed];

        public static readonly DiagnosticDescriptor InvalidIdentifier = new(
            DiagnosticIds.Singleton + "00",
            "Invalid identifier",
            "Invalid singleton member identifier: {0}",
            Categories.Default,
            DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor OnlyClassCanBeSingleton = new(
            DiagnosticIds.Singleton + "01",
            "Only class can be a singleton",
            "Only class can be a singleton",
            Categories.Default,
            DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor SingletonShouldBeSealed = new(
            DiagnosticIds.Singleton + "02",
            "Singleton should be sealed",
            "Singleton should be sealed, and cannot be abstract or static",
            Categories.Default,
            DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor SingletonShouldHaveCorrectCtor = new(
            DiagnosticIds.Singleton + "03",
            "Singleton should have correct constructor",
            "Can only declare private constructor with no parameter in singleton type, or do not declare instance constructor",
            Categories.Default,
            DiagnosticSeverity.Error, true);

        internal static readonly DiagnosticDescriptor InstancePropertyNameAndSingletonProviderNameShouldBeDifferent = new(
            DiagnosticIds.Singleton + "04",
            "Singleton member name repeat",
            "Singleton provider cannot have same name with instance property",
            Categories.Default,
            DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor CallSingletonConstrucotrMauallyIsNotAllowed = new(
            DiagnosticIds.Singleton + "05",
            "Do not call singleton constructor",
            "Do not call singleton constructor manually, use instance property instead",
            Categories.Default,
            DiagnosticSeverity.Error, true);
    }
}

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.SourceGeneration.Literals;
using Trarizon.Library.Roslyn.Extensions;
using Trarizon.Library.Roslyn.SourceInfos;

namespace Trarizon.Library.CodeAnalysis.SourceGeneration;
partial class SingletonGenerator
{
    internal static class Utils
    {
        public static bool IsValidInstancePropertyIdentifier([NotNullWhen(false)] string? identifier, [MaybeNullWhen(false)] out string actualIdentifier)
        {
            if (identifier is null) {
                actualIdentifier = "Instance";
                return true;
            }

            if (CodeValidation.IsValidIdentifier(identifier)) {
                actualIdentifier = identifier;
                return true;
            }

            actualIdentifier = null;
            return false;
        }

        public static bool IsValidProviderIdentifier(string identifier, [MaybeNullWhen(false)] out string actualIdentifier)
        {
            if (identifier == "") {
                actualIdentifier = "__SingletonProvider";
                return true;
            }

            if (CodeValidation.IsValidIdentifier(identifier)) {
                actualIdentifier = identifier;
                return true;
            }

            actualIdentifier = null;
            return false;
        }
    }

    internal readonly struct RuntimeAttribute(AttributeData attribute)
    {
        public const string DefaultInstancePropertyName = "Instance";
        public const string DefaultSingletonProviderName = "__SingletonProvider";

        public string? GetInstancePropertyName() => attribute
            .GetNamedArgument("InstancePropertyName")
            .CastValue<string>();

        public string? GetSingletonProviderName() => attribute
            .GetNamedArgument("SingletonProviderName")
            .CastValue<string>();

        public SingletonAccessibility GetInstanceAccessibility() => attribute
            .GetNamedArgument("InstanceAccessibility")
            .CastValue<SingletonAccessibility>();
    }

    internal enum SingletonAccessibility
    {
        Public = 0,
        Internal,
        Protected,
        Private,
        PrivateProtected,
        ProtectedInternal,
    }

    internal static class Descriptors
    {
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

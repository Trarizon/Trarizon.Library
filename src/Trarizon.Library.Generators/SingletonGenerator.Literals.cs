using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Roslyn.Extensions;

namespace Trarizon.Library.Generators;
partial class SingletonGenerator
{
    public const string Instance_Field_Identifier = "Instance";

    public readonly struct AttributeProxy(AttributeData attribute)
    {
        public const string Type_FullName = $"{Literals.NS_CodeGeneration}.SingletonAttribute";

        public const string DefaultInstancePropertyIdentifier = "Instance";
        public const string DefaultSingletonPropertyIdentifier = "__SingletonProvider";

        public static bool TryGet([NotNullWhen(true)]ISymbol? symbol, out AttributeProxy attribute)
        {
            var res = symbol.TryGetAttributeData(Type_FullName, out var attr);
            attribute = new(attr!);
            return res;
        }

        public string? InstancePropertyIdentifier => attribute
            .GetNamedArgument("InstancePropertyName")
            .Cast<string>();

        public string? SingletonProviderIdentifier => attribute
            .GetNamedArgument("SingletonProviderName")
            .Cast<string?>();

        public SingletonOptionsMirror Options => attribute
            .GetNamedArgument("Options")
            .Cast<SingletonOptionsMirror>();
    }

    [Flags]
    public enum SingletonOptionsMirror
    {
        None = 0,
        /// <summary>
        /// Do not use provider to create instance
        /// </summary>
        /// <remarks>
        /// By default, we create a nested class with a public field as provider <br/>
        /// If this option set, we directly create and assign to instance property, 
        /// in which case when you use this instance, all other static fields in this
        /// type will be initialized.
        /// </remarks>
        NoProvider = 1 << 0,
        /// <summary>
        /// Mark Instance property internal
        /// </summary>
        IsInternalInstance = 1 << 1,
    }

    public static class Diag
    {
        public static readonly DiagnosticDescriptor InvalidIdentifier_0Identifier = new(
            $"TRA{Literals.SingletonGenerator_Id}00",
            nameof(InvalidIdentifier_0Identifier),
            "Invalid Identifier(s): {0}",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor SingletonIsClassOnly = new(
                $"TRA{Literals.SingletonGenerator_Id}01",
                nameof(SingletonIsClassOnly),
                "Only class type can be singleton",
                Literals.Category,
                DiagnosticSeverity.Error,
                true);

        public static readonly DiagnosticDescriptor SingletonCannotBeAbstract = new(
            $"TRA{Literals.SingletonGenerator_Id}02",
            nameof(SingletonCannotBeAbstract),
            "Singleton cannot be abstract of static, because they are sealed",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor SingletonShouldHaveProperCtor = new(
            $"TRA{Literals.SingletonGenerator_Id}03",
            nameof(SingletonShouldHaveProperCtor),
            "Singleton can only have private non-parametered constructor or no explicit constructor",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor D_SingletonHasOneOrNoneCtor = new(
            $"TRA{Literals.SingletonGenerator_Id}04",
            nameof(D_SingletonHasOneOrNoneCtor),
            "constructor should have single or no constructor",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor SingletonMemberNameRepeat = new(
            $"TRA{Literals.SingletonGenerator_Id}05",
            nameof(SingletonMemberNameRepeat),
            "Provider and intance cannot have same name",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);

        // Analyzer
        public static readonly DiagnosticDescriptor SingletonCtorIsNotAccessable = new(
            $"TRA{Literals.SingletonGenerator_Id}06",
            nameof(SingletonCtorIsNotAccessable),
            "Do not call singleton ctor manually",
            Literals.Category,
            DiagnosticSeverity.Error,
            true);
    }
}

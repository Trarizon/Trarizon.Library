using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Trarizon.Library.Collections;

namespace Trarizon.Library.Roslyn.Extensions;
public static class CompilationExtensions
{
    public static ITypeSymbol? GetTypeSymbolByRuntimeType(this Compilation compilation, Type type)
        => RuntimeHelper.GetTypeSymbolByRuntimeType(compilation, type);

    #region RuntimeHelpers

    public static bool IsRuntimeType(this Compilation compilation, ITypeSymbol symbol, Type runtimeType, bool includeNullability = false)
    {
        var comparer = includeNullability ? SymbolEqualityComparer.IncludeNullability : SymbolEqualityComparer.Default;
        return comparer.Equals(symbol, compilation.GetTypeSymbolByRuntimeType(runtimeType));
    }

    public static bool IsRuntimeType<T>(this Compilation compilation, ITypeSymbol symbol, bool includeNullability = false)
        => compilation.IsRuntimeType(symbol, typeof(T), includeNullability);

    #endregion

    private static class RuntimeHelper
    {
        private static readonly Dictionary<Compilation, Dictionary<Type, ITypeSymbol>> _dict = new();

        public static ITypeSymbol? GetTypeSymbolByRuntimeType(Compilation compilation, Type type)
        {
            var res = GetSpecialTypeSymbol(compilation, type) ?? GetTypeSymbolByRuntimeTypeNonCache(compilation, type);
            if (res != null) {
                _dict[compilation][type] = res;
            }
            return res;
        }

        private static ITypeSymbol? GetSpecialTypeSymbol(Compilation compilation, Type type)
        {
            SpecialType spt;
            if (type == typeof(object))
                spt = SpecialType.System_Object;
            else if (type == typeof(Enum))
                spt = SpecialType.System_Enum;
            else if (type == typeof(MulticastDelegate))
                spt = SpecialType.System_MulticastDelegate;
            else if (type == typeof(Delegate))
                spt = SpecialType.System_Delegate;
            else if (type == typeof(ValueType))
                spt = SpecialType.System_ValueType;
            else if (type == typeof(void))
                spt = SpecialType.System_Void;
            else if (type == typeof(bool))
                spt = SpecialType.System_Boolean;
            else if (type == typeof(char))
                spt = SpecialType.System_Char;
            else if (type == typeof(sbyte))
                spt = SpecialType.System_SByte;
            else if (type == typeof(byte))
                spt = SpecialType.System_Byte;
            else if (type == typeof(short))
                spt = SpecialType.System_Int16;
            else if (type == typeof(ushort))
                spt = SpecialType.System_UInt16;
            else if (type == typeof(int))
                spt = SpecialType.System_Int32;
            else if (type == typeof(uint))
                spt = SpecialType.System_UInt32;
            else if (type == typeof(long))
                spt = SpecialType.System_Int64;
            else if (type == typeof(ulong))
                spt = SpecialType.System_UInt64;
            else if (type == typeof(decimal))
                spt = SpecialType.System_Decimal;
            else if (type == typeof(float))
                spt = SpecialType.System_Single;
            else if (type == typeof(double))
                spt = SpecialType.System_Double;
            else if (type == typeof(string))
                spt = SpecialType.System_String;
            else if (type == typeof(nint))
                spt = SpecialType.System_IntPtr;
            else if (type == typeof(nuint))
                spt = SpecialType.System_UIntPtr;
            else if (type == typeof(Array))
                spt = SpecialType.System_Array;
            else if (type == typeof(IEnumerable))
                spt = SpecialType.System_Collections_IEnumerable;
            else if (type == typeof(IEnumerable<>))
                spt = SpecialType.System_Collections_Generic_IEnumerable_T;
            else if (type == typeof(IList<>))
                spt = SpecialType.System_Collections_Generic_IList_T;
            else if (type == typeof(ICollection<>))
                spt = SpecialType.System_Collections_Generic_ICollection_T;
            else if (type == typeof(IEnumerator))
                spt = SpecialType.System_Collections_IEnumerator;
            else if (type == typeof(IEnumerator<>))
                spt = SpecialType.System_Collections_Generic_IEnumerator_T;
            else if (type == typeof(IReadOnlyList<>))
                spt = SpecialType.System_Collections_Generic_IReadOnlyList_T;
            else if (type == typeof(IReadOnlyCollection<>))
                spt = SpecialType.System_Collections_Generic_IReadOnlyCollection_T;
            else if (type == typeof(Nullable<>))
                spt = SpecialType.System_Nullable_T;
            else if (type == typeof(DateTime))
                spt = SpecialType.System_DateTime;
            else if (type == typeof(IsVolatile))
                spt = SpecialType.System_Runtime_CompilerServices_IsVolatile;
            else if (type == typeof(IDisposable))
                spt = SpecialType.System_Enum;
            else if (type == typeof(TypedReference))
                spt = SpecialType.System_TypedReference;
            //else if (type == typeof(System.ArgIterator))
            //    spt = SpecialType.System_ArgIterator;
            else if (type == typeof(RuntimeArgumentHandle))
                spt = SpecialType.System_RuntimeArgumentHandle;
            else if (type == typeof(RuntimeFieldHandle))
                spt = SpecialType.System_RuntimeFieldHandle;
            else if (type == typeof(RuntimeMethodHandle))
                spt = SpecialType.System_RuntimeMethodHandle;
            else if (type == typeof(RuntimeTypeHandle))
                spt = SpecialType.System_RuntimeTypeHandle;
            else if (type == typeof(IAsyncResult))
                spt = SpecialType.System_IAsyncResult;
            else if (type == typeof(AsyncCallback))
                spt = SpecialType.System_AsyncCallback;
            //else if (type == typeof(runtimefeature))
            //    spt = SpecialType.System_Runtime_CompilerServices_RuntimeFeature;
            //else if (type == typeof(perserve))
            //    spt = SpecialType.System_Runtime_CompilerServices_PreserveBaseOverridesAttribute;
            //else if (type == typeof(inlinearray))
            //    spt = SpecialType.System_Runtime_CompilerServices_InlineArrayAttribute;
            else
                return null;
            return compilation.GetSpecialType(spt);
        }

        private static ITypeSymbol? GetTypeSymbolByRuntimeTypeNonCache(Compilation compilation, Type type)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsArray) {
                var elementType = GetTypeSymbolByRuntimeType(compilation, typeInfo.GetElementType());
                if (elementType is null)
                    return null;
                int rank = typeInfo.GetArrayRank();
                return compilation.CreateArrayTypeSymbol(elementType, rank);
            }
            else if (typeInfo.IsPointer) {
                var elementType = GetTypeSymbolByRuntimeType(compilation, typeInfo.GetElementType());
                if (elementType is null)
                    return null;

                return compilation.CreatePointerTypeSymbol(elementType);
            }
            else if (typeInfo.DeclaringType != null) {
                Debug.Assert(!typeInfo.IsArray);

                // consolidated generic arguments (includes arguments of all declaring types):
                var genericArgs = typeInfo.GenericTypeArguments;

                var curTypeInfo = typeInfo.IsGenericType ? typeInfo.GetGenericTypeDefinition().GetTypeInfo() : typeInfo;
                var nestedTypes = TraEnumerable.EnumerateByNotNull(curTypeInfo,
                    ti =>
                    {
                        Debug.Assert(ti.IsGenericTypeDefinition || !curTypeInfo.IsGenericType);
                        return ti.DeclaringType == null ? null : ti.DeclaringType.GetTypeInfo();
                    })
                    .ToList();

                int i = nestedTypes.Count - 1;
                var symbol = GetTypeSymbolByRuntimeType(compilation, nestedTypes[i].AsType());
                if (symbol is null)
                    return null;

                while (--i >= 0) {
                    int forcedArity = nestedTypes[i].GenericTypeParameters.Length - nestedTypes[i + 1].GenericTypeParameters.Length;
                    symbol = symbol.GetTypeMembers(nestedTypes[i].Name, forcedArity).FirstOrDefault();
                    if (symbol is null)
                        return null;

                    symbol = ApplyGenericArgs((INamedTypeSymbol)symbol, genericArgs);
                    if (symbol is null)
                        return null;
                }
                return symbol;
            }
            else {
                AssemblyIdentity asmid = AssemblyIdentity.FromAssemblyDefinition(typeInfo.Assembly);
                var metadatasb = new StringBuilder();
                if (typeInfo.Namespace is { } ns) {
                    metadatasb.Append(ns).Append('.');
                }
                metadatasb.Append(typeInfo.Name);
                if (typeInfo.GenericTypeArguments.Length > 0) {
                    metadatasb.Append('`').Append(typeInfo.GenericTypeArguments.Length);
                }
                var metadataName = metadatasb.ToString();
                var symbol = compilation.GetTypeByMetadataName(metadataName);
                if (symbol is null)
                    return null;

                return ApplyGenericArgs(symbol, typeInfo.GenericTypeArguments);
            }

            ITypeSymbol? ApplyGenericArgs(INamedTypeSymbol symbol, Type[] genericArgs)
            {
                var typeArgs = new ITypeSymbol[genericArgs.Length];
                int typeArgIdx = 0;

                foreach (var arg in genericArgs) {
                    var sym = GetTypeSymbolByRuntimeType(compilation, type);
                    if (sym is null)
                        return null;
                    typeArgs[typeArgIdx++] = sym;
                }
                symbol = symbol.Construct(typeArgs);
                Debug.Assert(typeArgIdx == genericArgs.Length);
                return symbol;
            }
        }
    }

}

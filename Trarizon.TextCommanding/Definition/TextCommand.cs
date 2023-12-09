using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Trarizon.Library.Collections.Extensions;
using Trarizon.Library.Wrappers;
using Trarizon.TextCommanding.Attributes;
using Trarizon.TextCommanding.Exceptions;
using Trarizon.TextCommanding.Input;
using Trarizon.TextCommanding.Input.Parameters.CommandParameters;
using Trarizon.TextCommanding.Input.Parameters.ParameterMembers;
using Trarizon.TextCommanding.Utility;

namespace Trarizon.TextCommanding.Definition;
internal sealed class TextCommand : IOptionParameterInfoProvider
{
    private const BindingFlags AllInstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly ConcurrentDictionary<Type, TextCommand> _cache = new();

    public Type SourceType { get; }

    // TODO:Frozen
    public List<TextCommand>? SubCommands { get; private set; }
    public bool ForParseArgsOnly => SubCommands is null;

    private ConstructorInfo? _constructorInfo;
    private ConstructorParameterMember?[]? _constructorParameterInfos;
    // TODO: Frozen
    private readonly Dictionary<string, OptionParameter> _optionParamViaFullName;
    private readonly Dictionary<string, OptionParameter> _optionParamViaShortName;
    private readonly List<IValueParameter<TCValueAttribute>> _valueParams;

    IReadOnlyDictionary<string, OptionParameter> IOptionParameterInfoProvider.ViaFullName => _optionParamViaFullName;
    IReadOnlyDictionary<string, OptionParameter> IOptionParameterInfoProvider.ViaShortName => _optionParamViaShortName;

    private TextCommand(Type sourceType, TextCommandAttribute? preloadedAttribute = null)
    {
        SourceType = sourceType;

        InitializeSubCommands(preloadedAttribute);

        _optionParamViaFullName = new Dictionary<string, OptionParameter>();
        _optionParamViaShortName = new Dictionary<string, OptionParameter>();
        _valueParams = new List<IValueParameter<TCValueAttribute>>();

        InitializeConstructorInfo();
        InitializeParameters();
    }

    #region Initialization

    private void InitializeSubCommands(TextCommandAttribute? preloadedAttribute)
    {
        var attr = preloadedAttribute ?? SourceType.GetCustomAttribute<TextCommandAttribute>();
        if (attr == null)
            return; // Not decorated with [TextCommand], this type can only be used in ParseArgs()

        SubCommands = new(attr.ExplicitSubCommands.Length);

        // Only explicit types
        if (attr.SubCommandOption == SubCommandOption.OnlyExplicitTypes) {
            SubCommands.AddRange(attr.ExplicitSubCommands.Select(SelectOrThrow));
        }
        // Only nested types
        else if (attr.ExplicitSubCommands.Length == 0) {
            SubCommands.AddRange(SourceType.GetNestedTypes().WhereSelect(WhereSelector));
        }
        // Both
        else {
            SubCommands.AddRange(attr.ExplicitSubCommands.Select(SelectOrThrow)
                .Union(SourceType.GetNestedTypes().WhereSelect(WhereSelector)));
        }

        static TextCommand SelectOrThrow(Type t)
        {
            if (TryGetDecorated(t, out var cmd))
                return cmd;
            else {
                ThrowHelper.TextCommandInitializeFailed($"Explicit sub commands should be decorated with [{nameof(TextCommandAttribute)}]");
                return default!;
            }
        }

        static Optional<TextCommand> WhereSelector(Type t)
            => TryGetDecorated(t, out var cmd) ? Optional.Of(cmd) : default;
    }

    private void InitializeConstructorInfo()
    {
        var allCtors = SourceType.GetConstructors(AllInstanceMemberBindingFlags);
        switch (allCtors.Length) {
            case 0:
                ThrowHelper.TextCommandInitializeFailed($"No constructor defined in type <{SourceType.Name}>");
                break;
            case 1:
                _constructorInfo = allCtors[0];
                break;
            default:
                var ctors = allCtors.Where(c => c.GetCustomAttribute<TCConstructorAttribute>() != null);

                // If only one [TCCtor], use this ctor
                // If no [TCtor], set null and use Activator.CreateInstance() 
                if (!ctors.TrySingleOrNone(out _constructorInfo)) {
                    ThrowHelper.TextCommandInitializeFailed($"Only one constructor can be decoracted with {nameof(TCConstructorAttribute)}");
                }
                break;
        }
    }

    private void InitializeParameters()
    {
        IEnumerable<IParameterMember> fieldParamMembers = SourceType.GetFields(AllInstanceMemberBindingFlags)
            .Where(f => !f.IsInitOnly)
            .Select(f => new FieldParameterMember(f));
        IEnumerable<IParameterMember> propertyParamMembers = SourceType.GetProperties(AllInstanceMemberBindingFlags)
            .Where(p => p.CanWrite)
            .Select(p => new PropertyParameterMember(p));

        foreach (var prm in fieldParamMembers.Concat(propertyParamMembers)) {
            TryAddParameter(prm);
        }

        if (_constructorInfo != null) {
            ParameterInfo[] ctorPrmInfos = _constructorInfo.GetParameters();
            _constructorParameterInfos = new ConstructorParameterMember?[ctorPrmInfos.Length];

            for (int i = 0; i < ctorPrmInfos.Length; i++) {
                var cpm = new ConstructorParameterMember(ctorPrmInfos[i]);
                if (TryAddParameter(cpm))
                    _constructorParameterInfos[i] = cpm;
            }
        }

        _valueParams.Sort((l, r) => Comparer<int>.Default.Compare(l.Attribute.Order, r.Attribute.Order));
    }

    private bool TryAddParameter(IParameterMember prmMember)
    {
        if (!prmMember.GetParameterAttributes().TrySingleOrNone(out var attr))
            ThrowHelper.TextCommandInitializeFailed($"Field/Property <{prmMember.MemberName}> contains more than 1 {nameof(TCParameterAttribute)}.");

        switch (attr) {
            case null: return false;
            case TCOptionAttribute optAttr:
                ValidateParameterName(optAttr.FullName);
                if (optAttr.ShortName != null)
                    ValidateParameterName(optAttr.ShortName);

                var option = new OptionParameter(prmMember, optAttr);
                if (!_optionParamViaFullName.TryAdd(optAttr.FullName, option))
                    ThrowHelper.TextCommandInitializeFailed($"Argument full name <{optAttr.FullName}> repeated.");
                if (optAttr.ShortName != null && !_optionParamViaShortName.TryAdd(optAttr.ShortName, option))
                    ThrowHelper.TextCommandInitializeFailed($"Argument short name <{optAttr.ShortName}> repeated.");
                break;
            case TCValuesAttribute valsAttr:
                _valueParams.Add(new ValuesParameter(prmMember, valsAttr));
                break;
            case TCValueAttribute valAttr:
                _valueParams.Add(new ValueParameter(prmMember, valAttr));
                break;
            default:
                ThrowHelper.TextCommandInitializeFailed($"Unknown parameter type <{attr.GetType().Name}>");
                break;
        }
        return true;

        static void ValidateParameterName(string prmName)
        {
            if (prmName.StartsWith('-'))
                ThrowHelper.TextCommandInitializeFailed($"Parameter name <{prmName}> is invalid, parameter name cannot start with '-'.");
            foreach (char c in prmName) {
                if (char.IsWhiteSpace(c))
                    ThrowHelper.TextCommandInitializeFailed($"Parameter name <{prmName}> is invalid, parameter name cannot contains space.");
            }
        }
    }

    #endregion

    public static TextCommand Get(Type type) => _cache.GetOrAdd(type, t => new TextCommand(t));

    public static bool TryGetDecorated(Type type, [NotNullWhen(true)] out TextCommand? command)
    {
        if (_cache.TryGetValue(type, out command)) {
            if (command.ForParseArgsOnly) {
                command = null;
                return false;
            }
            return true;
        }
        else {
            var attr = type.GetCustomAttribute<TextCommandAttribute>();
            if (attr is null)
                return false;
            else {
                command = _cache.GetOrAdd(type, t => new TextCommand(t, attr));
                return true;
            }
        }


    }

    public static TextCommand Get<T>() => Get(typeof(T));

    public object Construct(RawArguments rawArguments)
    {
        ParsedArguments args = new(this, rawArguments);

        object obj;
        if (_constructorInfo is null) {
            obj = Activator.CreateInstance(SourceType)!;
            if (obj is null)
                ThrowHelper.InputParseFailed($"Cannot create object of type {SourceType.Name}");
        }
        else {
            // Set constrcutor parameters
            object?[] parameters = _constructorParameterInfos!.Length == 0
                ? Array.Empty<object>() : new object?[_constructorParameterInfos.Length];
            for (int i = 0; i < parameters.Length; i++) {
                IParameterMember? key = _constructorParameterInfos[i];
                if (key == null)
                    parameters[i] = null;
                else
                    parameters[i] = args.ConstructorArguments[key];
            }
            obj = _constructorInfo.Invoke(parameters);
        }

        // Set fields and properties
        foreach (var (prmMember, val) in args.Properties) {
            prmMember.SetValue(obj, val);
        }

        return obj;
    }

    private readonly ref struct ParsedArguments
    {
        public readonly Dictionary<IParameterMember, object?> ConstructorArguments = new();
        public readonly LinkedList<(IParameterProperty ParamInfo, object? Value)> Properties = new();

        public ParsedArguments(TextCommand command, RawArguments rawArguments)
        {
            foreach (var (name, prm) in command._optionParamViaFullName) {
                if (rawArguments.GetOptionValue(name, out var rawValue))
                    Add(prm.ParameterMember, prm.ParseValue(rawValue));
                else if (rawArguments.HasFlag(name))
                    Add(prm.ParameterMember, BoxUtil.True);
                else
                    Add(prm.ParameterMember, prm.Attribute.Default);
            }

            var values = rawArguments.GetValues();
            foreach (var prm in command._valueParams) {
                if (values.IsEnd)
                    Add(prm.ParameterMember, prm.Attribute.Default);
                else
                    Add(prm.ParameterMember, prm.ParseInput(ref values));
            }
        }

        public void Add(IParameterMember parameter, object? value)
        {
            if (parameter is IParameterProperty property)
                Properties.AddLast((property, value));
            else
                ConstructorArguments.Add(parameter, value);
        }
    }
}

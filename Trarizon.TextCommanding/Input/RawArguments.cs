using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Trarizon.TextCommanding.Definition;
using Trarizon.TextCommanding.Exceptions;
using Trarizon.TextCommanding.Input.Parameters.CommandParameters;

namespace Trarizon.TextCommanding.Input;
internal sealed class RawArguments
{
    internal readonly Dictionary<string, string> _options = new();
    private readonly HashSet<string> _flags = new();
    private readonly List<string> _values = new();

    public static RawArguments Parse<TInputArgs>(TInputArgs input, IOptionParameterInfoProvider prmProvider, ParseArgsSettings settings) where TInputArgs : IRawInput
    {
        RawArguments result = new();
        result.Initialize(input, prmProvider, settings);
        return result;
    }

    private void Initialize<TInputArgs>(TInputArgs input, IOptionParameterInfoProvider prmProvider, ParseArgsSettings settings) where TInputArgs : IRawInput
    {
        while (input.MoveNext()) {
            InputSplit split = input.Current;

            if (!HandleOptionParameter(split, settings.FullNamePrefix, prmProvider.ViaFullName)
                && !HandleOptionParameter(split, settings.ShortNamePrefix, prmProvider.ViaShortName))
                AddValue(split.Value);
        }


        bool HandleOptionParameter(in InputSplit split, string prefix, IReadOnlyDictionary<string, OptionParameter> parameters)
        {
            if (!split.Value.StartsWith(prefix))
                return false;

            string key = new(split.Value[prefix.Length..]);
            // As value
            if (!parameters.TryGetValue(key, out var prm)) {
                switch (settings.UnknownParameterNameHandling) {
                    case UnknownParameterNameHandling.AsValue:
                        AddValue(split.Value);
                        break;
                    case UnknownParameterNameHandling.ThrowException:
                        ThrowHelper.InputParseFailed($"Unknown parameter '{split.Value}'.");
                        break;
                    default:
                        ThrowHelper.ThrowInvalidOperation();
                        break;
                }
            }
            // As parameter
            else if (prm.ParameterMember.MemberType == typeof(bool))
                AddFlag(prm);
            else if (input.MoveNext())
                AddOption(prm, input.Current.Value);
            else
                ThrowHelper.InputParseFailed($"Input ended unexceptedly while reading argument <{prm.Attribute.DisplayName}>");

            return true;
        }
    }

    private void AddFlag(OptionParameter parameter)
    {
        if (_flags.Contains(parameter.Attribute.FullName))
            ThrowHelper.InputParameterRepeated(parameter);
        _flags.Add(parameter.Attribute.FullName);
    }

    private void AddOption(OptionParameter parameter, ReadOnlySpan<char> value)
    {
        if (!_options.TryAdd(parameter.Attribute.FullName, new string(value)))
            ThrowHelper.InputParameterRepeated(parameter);
    }

    private void AddValue(scoped in ReadOnlySpan<char> value)
    {
        _values.Add(new string(value));
    }

    public bool GetOptionValue(string key, [NotNullWhen(true)] out string? value)
        => _options.TryGetValue(key, out value);

    public bool HasFlag(string flag) => _flags.Contains(flag);

    public Values GetValues() => new(this);

    public ref struct Values(RawArguments rawArguments)
    {
        private readonly ReadOnlySpan<string> _values = CollectionsMarshal.AsSpan(rawArguments._values);
        private int _index = 0;

        public readonly bool IsEnd => _index >= _values.Length;

        public ReadOnlySpan<string> Read(int expectedCount)
        {
            if (IsEnd)
                return ReadOnlySpan<string>.Empty;

            int start = _index;
            int actualCount = int.Min(_values.Length - _index, expectedCount);
            _index += actualCount;
            return _values[start.._index];
        }

        public ReadOnlySpan<string> ReadToEnd()
        {
            var res = _values[_index..];
            _index = _values.Length;
            return res;
        }

        public bool Read([NotNullWhen(true)] out string? value)
        {
            if (IsEnd) {
                value = null;
                return false;
            }

            value = _values[_index++];
            return true;
        }
    }
}

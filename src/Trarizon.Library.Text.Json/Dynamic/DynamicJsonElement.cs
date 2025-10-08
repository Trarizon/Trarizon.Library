using System.Collections;
using System.Dynamic;
using System.Text.Json;

namespace Trarizon.Library.Text.Json.Dynamic;
public sealed class DynamicJsonElement(JsonElement jsonElement, bool suppressNull) : DynamicObject, IEnumerable<object?>
{
    private readonly JsonElement _element = jsonElement;
    private IEnumerable<string>? _memeberNames;
    private readonly bool _suppressNull = suppressNull;

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        if (_memeberNames is null) {
#if NET8_0
            var result = new List<string>();
            foreach (var item in _element.EnumerateObject()) {
                result.Add(item.Name);
            }
            _memeberNames = result;
#else
            var count = _element.GetPropertyCount();
            if (count == 0) {
                _memeberNames = [];
            }
            else {
                var result = new string[count];
                int i = 0;
                foreach (var item in _element.EnumerateObject()) {
                    result[i++] = item.Name;
                }
                _memeberNames = result;
            }
#endif
        }
        return _memeberNames;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (_element.ValueKind is JsonValueKind.Object && _element.TryGetProperty(binder.Name, out var property)) {
            result = GetObject(property);
            return true;
        }
        if (_suppressNull) {
            result = null;
            return true;
        }
        return base.TryGetMember(binder, out result);
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
    {
        if (indexes.Length != 1) {
            return base.TryGetIndex(binder, indexes, out result);
        }
        var idx = indexes[0];
        if (idx is int index) {
            if (_element.ValueKind is JsonValueKind.Array && index < _element.GetArrayLength()) {
                result = GetObject(_element[index]);
                return true;
            }
        }
        else if (idx is string key) {
            if (_element.ValueKind is JsonValueKind.Object && _element.TryGetProperty(key, out var property)) {
                result = GetObject(property);
                return true;
            }
        }

        if (_suppressNull) {
            result = null;
            return true;
        }
        return base.TryGetIndex(binder, indexes, out result);
    }

    public override bool TryConvert(ConvertBinder binder, out object? result)
    {
        var type = binder.Type;
        if (type == typeof(JsonElement)) {
            result = _element;
            return true;
        }

        if (type == typeof(string)) {
            if (_element.ValueKind is JsonValueKind.String) {
                result = _element.GetString();
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = null;
                return true;
            }
        }
        else if (type == typeof(byte)) {
            if (_element.TryGetByte(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(sbyte)) {
            if (_element.TryGetSByte(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(short)) {
            if (_element.TryGetInt16(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(ushort)) {
            if (_element.TryGetUInt16(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(int)) {
            if (_element.TryGetInt32(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(uint)) {
            if (_element.TryGetUInt32(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(long)) {
            if (_element.TryGetInt64(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(ulong)) {
            if (_element.TryGetUInt64(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(float)) {
            if (_element.TryGetSingle(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(double)) {
            if (_element.TryGetDouble(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(decimal)) {
            if (_element.TryGetDecimal(out var n)) {
                result = n;
                return true;
            }
        }
        else if (type == typeof(bool)) {
            if (_element.ValueKind is JsonValueKind.True) {
                result = true;
                return true;
            }
            else if (_element.ValueKind is JsonValueKind.False) {
                result = false;
                return true;
            }
        }
        else if (type == typeof(DateTime)) {
            if (_element.TryGetDateTime(out var dateTime)) {
                result = dateTime;
                return true;
            }
        }
        else if (type == typeof(DateTimeOffset)) {
            if (_element.TryGetDateTimeOffset(out var dateTimeOffset)) {
                result = dateTimeOffset;
                return true;
            }
        }
        else if (type == typeof(Guid)) {
            if (_element.TryGetGuid(out var guid)) {
                result = guid;
                return true;
            }
        }

        else if (type == typeof(byte?)) {
            if (_element.TryGetByte(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(byte?);
                return true;
            }
        }
        else if (type == typeof(sbyte?)) {
            if (_element.TryGetSByte(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(sbyte?);
                return true;
            }
        }
        else if (type == typeof(short?)) {
            if (_element.TryGetInt16(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(short?);
                return true;
            }
        }
        else if (type == typeof(ushort?)) {
            if (_element.TryGetUInt16(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(ushort?);
                return true;
            }
        }
        else if (type == typeof(int?)) {
            if (_element.TryGetInt32(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(int?);
                return true;
            }
        }
        else if (type == typeof(uint?)) {
            if (_element.TryGetUInt32(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(uint?);
                return true;
            }
        }
        else if (type == typeof(long?)) {
            if (_element.TryGetInt64(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(long?);
                return true;
            }
        }
        else if (type == typeof(ulong?)) {
            if (_element.TryGetUInt64(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(ulong?);
                return true;
            }
        }
        else if (type == typeof(float?)) {
            if (_element.TryGetSingle(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(float?);
                return true;
            }
        }
        else if (type == typeof(double?)) {
            if (_element.TryGetDouble(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(double?);
                return true;
            }
        }
        else if (type == typeof(decimal?)) {
            if (_element.TryGetDecimal(out var n)) {
                result = n;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(decimal?);
                return true;
            }
        }
        else if (type == typeof(bool?)) {
            if (_element.ValueKind is JsonValueKind.True) {
                result = true;
                return true;
            }
            else if (_element.ValueKind is JsonValueKind.False) {
                result = false;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(bool?);
                return true;
            }
        }
        else if (type == typeof(DateTime?)) {
            if (_element.TryGetDateTime(out var dateTime)) {
                result = dateTime;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(DateTime?);
                return true;
            }
        }
        else if (type == typeof(DateTimeOffset?)) {
            if (_element.TryGetDateTimeOffset(out var dateTimeOffset)) {
                result = dateTimeOffset;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(DateTimeOffset?);
                return true;
            }
        }
        else if (type == typeof(Guid?)) {
            if (_element.TryGetGuid(out var guid)) {
                result = guid;
                return true;
            }
            if (_element.ValueKind is JsonValueKind.Null) {
                result = default(Guid?);
                return true;
            }
        }

        return base.TryConvert(binder, out result!);
    }

    public override string ToString() => _element.ToString();

    private object? GetObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => new DynamicJsonElement(element, _suppressNull),
            JsonValueKind.Array => new DynamicJsonElement(element, _suppressNull),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => new DynamicJsonElement(element, _suppressNull)
        };
    }

    public IEnumerator GetEnumerator()
    {
        var len = _element.GetArrayLength();
        for (var i = 0; i < len; i++) {
            yield return GetObject(_element[i]);
        }
    }

    IEnumerator<object?> IEnumerable<object?>.GetEnumerator()
    {
        var len = _element.GetArrayLength();
        for (var i = 0; i < len; i++) {
            yield return GetObject(_element[i]);
        }
    }
}

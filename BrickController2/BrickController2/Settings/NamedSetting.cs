using Newtonsoft.Json;
using System;

namespace BrickController2.Settings;

public record NamedSetting
{
    /// <summary>Unique setting name</summary>
    [JsonProperty]
    public string Name { get; init; } = default!;

    /// <summary>Current setting value</summary>
    /// <remarks>Type should match <see cref="DefaultValue"/></remarks>
    [JsonProperty]
    public object Value { get; set; } = default!;

    /// <summary>Default setting value</summary>
    /// <remarks>This is not persisted, but kept in memory only</remarks>
    [JsonIgnore]
    public object DefaultValue { get; init; } = default!;

    /// <summary>Type of setting value</summary>
    [JsonIgnore]
    public Type Type => Value?.GetType() ?? typeof(void);

    /// <summary>Optional group name</summary>
    [JsonIgnore]
    public string Group { get; set; } = default!;

    [JsonIgnore]
    public bool IsBoolType => Type == typeof(bool);

    [JsonIgnore]
    public bool IsEnumType => Type.IsEnum;
}

namespace Crpg.Module.Common;

/// <summary>
/// A constant shared between the client and the server that can be easily changed with a chat command during development.
/// </summary>
internal class SharedConstant
{
    private static readonly Dictionary<int, SharedConstant> AllConstants = new();

    /// <param name="id">An id to uniquely identity the constant.</param>
    /// <param name="defaultValue">The default value of the constant.</param>
    public static SharedConstant Create(int id, float defaultValue)
    {
        if (AllConstants.ContainsKey(id))
        {
            throw new ArgumentException("A shared constant already exists with this id", nameof(id));
        }

        SharedConstant constant = new(defaultValue);
        AllConstants[id] = constant;
        return constant;
    }

    public static bool TryUpdate(int id, float newValue, out float oldValue)
    {
        if (AllConstants.TryGetValue(id, out var constant))
        {
            oldValue = constant.Value;
            constant.Value = newValue;
            return true;
        }

        oldValue = default;
        return false;
    }

    private SharedConstant(float defaultValue)
    {
        Value = defaultValue;
    }

    public float Value { get; private set; }
}

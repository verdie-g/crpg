namespace Crpg.Module.Common.HotConstants;

/// <summary>
/// A constant shared between the client and the server that can be easily changed with a chat command during development.
/// </summary>
internal class HotConstant
{
    private static readonly Dictionary<int, HotConstant> AllConstants = new();

    /// <param name="id">An id to uniquely identity the constant.</param>
    /// <param name="defaultValue">The default value of the constant.</param>
    public static HotConstant Create(int id, float defaultValue)
    {
        if (AllConstants.ContainsKey(id))
        {
            throw new ArgumentException("A hot constant already exists with this id", nameof(id));
        }

        HotConstant constant = new(defaultValue);
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

    private HotConstant(float defaultValue)
    {
        Value = defaultValue;
    }

    public float Value { get; private set; }
}

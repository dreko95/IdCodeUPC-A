using System.Collections.Generic;

namespace IdCode.Helpers;

public static class Constants {
    public static readonly byte[] H1 = [1, 0, 1];
    public static readonly byte[] H4 = [0, 1, 0, 1, 0];

    public static readonly Dictionary<char, byte[]> CodeSetADictionary = new() {
        { '0', [0, 0, 0, 1, 1, 0, 1] },
        { '1', [0, 0, 1, 1, 0, 0, 1] },
        { '2', [0, 0, 1, 0, 0, 1, 1] },
        { '3', [0, 1, 1, 1, 1, 0, 1] },
        { '4', [0, 1, 0, 0, 0, 1, 1] },
        { '5', [0, 1, 1, 0, 0, 0, 1] },
        { '6', [0, 1, 0, 1, 1, 1, 1] },
        { '7', [0, 1, 1, 1, 0, 1, 1] },
        { '8', [0, 1, 1, 0, 1, 1, 1] },
        { '9', [0, 0, 0, 1, 0, 1, 1] }
    };

    public static readonly Dictionary<char, byte[]> CodeSetBDictionary = new() {
        { '0', [0, 1, 0, 0, 1, 1, 1] },
        { '1', [0, 1, 1, 0, 0, 1, 1] },
        { '2', [0, 0, 1, 1, 0, 1, 1] },
        { '3', [0, 1, 0, 0, 0, 0, 1] },
        { '4', [0, 0, 1, 1, 1, 0, 1] },
        { '5', [0, 1, 1, 1, 0, 0, 1] },
        { '6', [0, 0, 0, 0, 1, 0, 1] },
        { '7', [0, 0, 1, 0, 0, 0, 1] },
        { '8', [0, 0, 0, 1, 0, 0, 1] },
        { '9', [0, 0, 1, 0, 1, 1, 1] }
    };
}
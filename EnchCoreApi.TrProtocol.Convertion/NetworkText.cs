using EnchCoreApi.TrProtocol.Attributes;
using Terraria.Localization;

namespace EnchCoreApi.TrProtocol.Convertion;
public class NetworkText {
    static NetworkText[] __covert_Array_NetworkTextModel_To_NetworkText(NetworkTextModel[] a) {
        var arr = new NetworkText[a.Length];
        for (int i = 0; i < a.Length; i++) {
            arr[i] = op_Implicit(a[i]);
        }
        return arr;
    }
    static NetworkTextModel[] __covert_Array_NetworkText_To_NetworkTextModel(NetworkText[] a) {
        var arr = new NetworkTextModel[a.Length];
        for (int i = 0; i < a.Length; i++) {
            arr[i] = op_Explicit(a[i]);
        }
        return arr;
    }
    [CastOperatorPlaceHolder(CastOperator.Implicit)]
    static extern NetworkText op_Implicit(NetworkTextModel a);
    [CastOperatorPlaceHolder(CastOperator.Explicit)]
    static extern NetworkTextModel op_Explicit(NetworkText a);
}

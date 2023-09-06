using EnchCoreApi.TrProtocol.Attributes;
using Terraria.Localization;

namespace Terraria.Localization; 
public class NetworkText {
    static NetworkText[] Covert_Array_NetworkTextModel_To_NetworkText(NetworkTextModel[] a) {
        var arr = new NetworkText[a.Length];
        for (int i = 0; i < a.Length; i++) {
            arr[i] = (NetworkText)a[i];
        }
        return arr;
    }
    static NetworkTextModel[] Covert_Array_NetworkText_To_NetworkTextModel(NetworkText[] a) {
        var arr = new NetworkTextModel[a.Length];
        for (int i = 0; i < a.Length; i++) {
            arr[i] = (NetworkTextModel)a[i];
        }
        return arr;
    }
    [CastOperatorPlaceHolder]
    public static implicit operator NetworkText(NetworkTextModel a) => null;
    [CastOperatorPlaceHolder]
    public static explicit operator NetworkTextModel(NetworkText a) => null;
}

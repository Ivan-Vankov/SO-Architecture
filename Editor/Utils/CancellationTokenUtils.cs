using System.Threading;

namespace Vaflov {
    public static class CancellationTokenUtils {
        public static void ResetCancellationTokenSource(ref CancellationTokenSource cts) {
            if (cts != null) {
                cts.Cancel();
                cts.Dispose();
            }
            cts = new CancellationTokenSource();
        }
    }
}

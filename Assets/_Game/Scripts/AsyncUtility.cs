using System.Threading;

namespace Moball {
  public static class AsyncUtility {
    public static void CancelAndDispose(ref CancellationTokenSource cts) {
      if (cts != null) {
        cts.Cancel();
        cts.Dispose();
        cts = null;
      }
    }
  }
}
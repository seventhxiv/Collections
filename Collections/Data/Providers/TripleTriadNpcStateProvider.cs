using System.Runtime.InteropServices;

namespace Collections;

public class TripleTriadNpcStateProvider
{
    private const int MinimumTriadNpcId = 0x230002;
    private const string IsNpcBeatenSignature = "40 53 48 83 ec 20 8d 82 fe ff dc ff";
    private const string TriadUiStateSignature = "48 8d 0d ?? ?? ?? ?? e8 ?? ?? ?? ?? 84 c0 74 0f 8b cb";

    public static TripleTriadNpcStateProvider Instance { get; } = new();

    private delegate byte IsNpcBeatenDelegate(IntPtr uiState, int triadNpcId);

    private readonly IntPtr isNpcBeatenPtr;
    private readonly IntPtr triadUiStatePtr;
    private readonly IsNpcBeatenDelegate? isNpcBeatenFunc;

    private TripleTriadNpcStateProvider()
    {
        try
        {
            // Services.UnlockState does not contain a method to retrieve IsNpcBeaten.
            // Signature address credit to https://github.com/MgAl2O4/FFTriadBuddyDalamud/blob/main/plugin/UnsafeReaderTriadCards.cs
            // This address remain const since launch of dawntrail as of may 2026
            isNpcBeatenPtr = Services.SigScanner.ScanText(IsNpcBeatenSignature);
            triadUiStatePtr = Services.SigScanner.GetStaticAddressFromSig(TriadUiStateSignature);

            if (isNpcBeatenPtr != IntPtr.Zero && triadUiStatePtr != IntPtr.Zero)
            {
                isNpcBeatenFunc = Marshal.GetDelegateForFunctionPointer<IsNpcBeatenDelegate>(isNpcBeatenPtr);
            }
        }
        catch (Exception ex)
        {
            Dev.Log($"Failed to initialize Triple Triad NPC state provider: {ex.Message}");
        }
    }

    public bool IsNpcBeaten(int npcId)
    {
        if (isNpcBeatenPtr == IntPtr.Zero || triadUiStatePtr == IntPtr.Zero || npcId < MinimumTriadNpcId)
        {
            return false;
        }

        return isNpcBeatenFunc != null && isNpcBeatenFunc(triadUiStatePtr, npcId) != 0;
    }
}

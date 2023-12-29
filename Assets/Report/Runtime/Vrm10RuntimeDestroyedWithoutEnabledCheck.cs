using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UniVRM10;
using VRMShaders;

public class Vrm10RuntimeDestroyedWithoutEnabledCheck : MonoBehaviour
{
    [SerializeField] private string vrmPath;
    [SerializeField] private bool accessRuntimeAfterDestroy;

    private RuntimeOnlyAwaitCaller awaitCaller;

    private byte[] bytes;

    private void Start()
    {
        bytes = File.ReadAllBytes(vrmPath);
        awaitCaller = new RuntimeOnlyAwaitCaller();

#pragma warning disable CS4014
        CheckAsync();
#pragma warning restore CS4014
    }

    private async Task CheckAsync()
    {
        var instance = await Vrm10.LoadBytesAsync(bytes);
        instance.gameObject.SetActive(false);

        await awaitCaller.NextFrame();
        DestroyImmediate(instance.gameObject);

        if (accessRuntimeAfterDestroy)
        {
            await awaitCaller.NextFrame();
            try
            {
                var _ = instance.Runtime;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}
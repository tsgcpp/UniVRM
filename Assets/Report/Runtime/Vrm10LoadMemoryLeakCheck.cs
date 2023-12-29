using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UniVRM10;
using VRMShaders;

public class Vrm10LoadMemoryLeakCheck : MonoBehaviour
{
    [SerializeField] private string vrmPath;
    [SerializeField] private int loadCount = 100;
    [SerializeField] private float firstDelaySeconds = 2.5f;
    [SerializeField] private bool useFirstPerson;
    [SerializeField] private bool destroyInstance;
    
    private byte[] bytes;
    private RuntimeOnlyAwaitCaller awaitCaller;
    
    private void Start()
    {
        bytes = File.ReadAllBytes(vrmPath);
        awaitCaller = new RuntimeOnlyAwaitCaller();

#pragma warning disable CS4014
        LoadLoopAsync();
#pragma warning restore CS4014
    }

    private async Task LoadLoopAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(firstDelaySeconds));
        
        for (int i = 0; i < loadCount; ++i)
        {
            await awaitCaller.NextFrame();
            if (this == null)
            {
                break;
            }
            
            var instance = await Vrm10.LoadBytesAsync(bytes);
            if (useFirstPerson)
            {
                await instance.Vrm.FirstPerson.SetupAsync(instance.gameObject, awaitCaller, isSelf: true);
            }

            if (destroyInstance)
            {
                DestroyImmediate(instance.gameObject);
            }
        }
        
        await awaitCaller.NextFrame();
        Debug.Log("Vrm10LoadMemoryLeakCheck.LoadLoopAsync finished");
    }
}

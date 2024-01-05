using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UniVRM10;
using VRMShaders;

public class Vrm10LoadWithFirstPerson : MonoBehaviour
{
    [SerializeField] private string vrmPath;

    private byte[] bytes;
    private RuntimeOnlyAwaitCaller awaitCaller;

    private void Start()
    {
        bytes = File.ReadAllBytes(vrmPath);
        awaitCaller = new RuntimeOnlyAwaitCaller();

#pragma warning disable CS4014
        LoadAsync();
#pragma warning restore CS4014
    }

    private async Task LoadAsync()
    {
        try
        {
            var instance = await Vrm10.LoadBytesAsync(bytes);
            await instance.Vrm.FirstPerson.SetupAsync(instance.gameObject, awaitCaller, isSelf: true);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }
}
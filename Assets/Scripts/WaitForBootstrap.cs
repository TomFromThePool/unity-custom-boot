using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HalliHax.Samples
{
    /// <summary>
    /// Simple script to wait for BootStrap initialisation in our initial scene
    /// </summary>
    public class WaitForBootstrap : MonoBehaviour
    {
        async void Awake()
        {
            while (!CustomBoot.CustomBoot.Initialised)
                await Task.Yield();

            SceneManager.LoadScene("Scenes/SampleScene/SampleScene");
        }

        
    }
}
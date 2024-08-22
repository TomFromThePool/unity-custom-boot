using System;
using UnityEngine;

namespace HalliHax.Samples
{
    /// <summary>
    /// Simple component to modify a material based on bootstrap init status
    /// </summary>
    public class BootStrapIndicator : MonoBehaviour
    {
        /// <summary>
        /// Green if boot-strapper is initialised on Awake
        /// </summary>
        Color InitialisedOnAwakeColour = Color.green;

        /// <summary>
        /// Green if boot-strapper was initialised after Awake
        /// </summary>
        Color InitialisedAfterAwakeColour = Color.yellow;

        /// <summary>
        /// Red if boot-strapper is not initialised
        /// </summary>
        Color UninitialisedColour = Color.red;

        public MeshRenderer MeshRenderer;

        private bool wasInitialisedOnAwake = false;
        private bool isInitialised = false;

        /// <summary>
        /// When we receive the Awake call, check the current bootstrapper status
        /// </summary>
        void Awake()
        {
            wasInitialisedOnAwake = CustomBoot.CustomBoot.Initialised;
            isInitialised = wasInitialisedOnAwake;

            if (wasInitialisedOnAwake)
            {
                SetIndicatorColour(InitialisedOnAwakeColour);
            }
            else
            {
                SetIndicatorColour(UninitialisedColour);
            }
        }

        private void Update()
        {
            if (!wasInitialisedOnAwake && !isInitialised && CustomBoot.CustomBoot.Initialised)
            {
                isInitialised = true;
                SetIndicatorColour(InitialisedAfterAwakeColour);
            }
            else if (isInitialised && !CustomBoot.CustomBoot.Initialised)
            {
                isInitialised = false;
                SetIndicatorColour(UninitialisedColour);
            }
        }
        
        void SetIndicatorColour(Color c)
        {
            if (MeshRenderer)
            {
                MeshRenderer.material.color = c;
            }
        }
    }
}
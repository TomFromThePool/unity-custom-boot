using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HalliHax.Samples
{
    public class PlayerController : MonoBehaviour
    {
        public float MoveSpeed = 1000f;
        public CharacterController Controller;
        public Transform CameraPivot;
        public InputActionReference Forward;
        public InputActionReference Backward;
        public InputActionReference Left;
        public InputActionReference Right;
        public InputActionReference Look;

        private void OnEnable()
        {
            Forward.action.Enable();
            Forward.action.started += ForwardStarted;
            Forward.action.canceled += ForwardCanceled;
            
            Backward.action.Enable();
            Backward.action.started += BackwardStarted;
            Backward.action.canceled += BackwardCanceled;
            
            Left.action.Enable();
            Left.action.started += LeftStarted;
            Left.action.canceled += LeftCanceled;
            
            Right.action.Enable();
            Right.action.started += RightStarted;
            Right.action.canceled += RightCanceled;
            Look.action.Enable();
            currentRotationEuler = CameraPivot.rotation.eulerAngles;
        }

        private void RightCanceled(InputAction.CallbackContext obj)
        {
            velocity -= Vector3.right;
        }

        private void RightStarted(InputAction.CallbackContext obj)
        {
            velocity += Vector3.right;
        }

        private void LeftCanceled(InputAction.CallbackContext obj)
        {
            velocity -= Vector3.left;
        }

        private void LeftStarted(InputAction.CallbackContext obj)
        {
            velocity += Vector3.left;
        }

        private void BackwardCanceled(InputAction.CallbackContext obj)
        {
            velocity -= Vector3.back;
        }

        private void BackwardStarted(InputAction.CallbackContext obj)
        {
            velocity += Vector3.back;
        }

        private Vector3 velocity = Vector3.zero;
        
        private void ForwardCanceled(InputAction.CallbackContext obj)
        {
            velocity -= Vector3.forward;
        }

        void ForwardStarted(InputAction.CallbackContext obj)
        {
            velocity += Vector3.forward;
        }

        private Quaternion currentRotation = Quaternion.identity;
        private Vector3 currentRotationEuler = Vector3.zero;
        
        private void Update()
        {
            Controller.Move(transform.TransformDirection(velocity + Physics.gravity) * (MoveSpeed * Time.deltaTime));
            var look = Look.action.ReadValue<Vector2>();

            currentRotationEuler += new Vector3(look.y, 0, 0);
            currentRotationEuler.x = Mathf.Clamp(currentRotationEuler.x, -45, 45);
            currentRotation.eulerAngles = currentRotationEuler;
            CameraPivot.localRotation = currentRotation;
            
            transform.rotation *= Quaternion.Euler(0, look.x, 0);

        }
    }
}
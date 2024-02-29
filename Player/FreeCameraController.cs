using UnityEngine;

namespace TravkinGames.Voxels
{
    public class FreeCameraController : MonoBehaviour
    {
        [Header("Input Keys")]
        [SerializeField] private KeyCode _forwardMovement = KeyCode.W;
        [SerializeField] private KeyCode _backwardMovement = KeyCode.S;
        [SerializeField] private KeyCode _leftMovement = KeyCode.A;
        [SerializeField] private KeyCode _rightMovement = KeyCode.D;
        [SerializeField] private KeyCode _upMovement = KeyCode.E;
        [SerializeField] private KeyCode _downMovement = KeyCode.Q;
        [SerializeField] private KeyCode _sprintMovement = KeyCode.LeftShift;
        
        [Header("Speeds")]
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _sprintMultiplier = 2.5f;
        
        private void Update()
        {
            var move = Vector3.zero;
            if (Input.GetKey(_forwardMovement))
                move += transform.forward;
            if (Input.GetKey(_backwardMovement))
                move -= transform.forward;
            if (Input.GetKey(_leftMovement))
                move -= transform.right;
            if (Input.GetKey(_rightMovement))
                move += transform.right;
            if (Input.GetKey(_upMovement))
                move += transform.up;
            if (Input.GetKey(_downMovement))
                move -= transform.up;
            
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            var speed = _movementSpeed;
            if (Input.GetKey(_sprintMovement))
                speed *= _sprintMultiplier;
            
            transform.position += move * (speed * Time.deltaTime);
            
            var rotation = Vector3.zero;
            rotation.y = Input.GetAxis("Mouse X");
            rotation.x = -Input.GetAxis("Mouse Y");
            transform.Rotate(rotation * _rotationSpeed);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
        }
    }
}
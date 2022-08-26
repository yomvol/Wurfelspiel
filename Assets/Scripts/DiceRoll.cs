using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiceRoll : MonoBehaviour
{
    public float impulseStrength; // module of the force vector
    public float angularVelocity;
    [HideInInspector]
    public DiceFace rollResult;
    public event EventHandler resultReadyEvent;

    private Tuple<DiceFace, Ray>[] _raysFromFaces;
    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;
    private MeshRenderer _renderer;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        _renderer = GetComponent<MeshRenderer>();
        _renderer.enabled = false;
        _raysFromFaces = new Tuple<DiceFace, Ray>[6];
        _rigidbody.useGravity = false;
        rollResult = DiceFace.One;
    }

    public void ThrowDice()
    {
        _renderer.enabled = true;
        Vector3 impulse = new Vector3();
        impulse.y = 0;
        float angleOfImpulse = Random.Range(0.01f, 2 * MathF.PI);
        impulse.x = MathF.Cos(angleOfImpulse) * impulseStrength;
        impulse.z = MathF.Sin(angleOfImpulse) * impulseStrength;
        Vector3 angularMomentum = Vector3.zero;
        int torqueDirection = Random.Range(0, 6);
        switch (torqueDirection)
        {
            case 0:
                angularMomentum = Vector3.up;
                break;
            case 1:
                angularMomentum = Vector3.down;
                break;
            case 2:
                angularMomentum = Vector3.left;
                break;
            case 3:
                angularMomentum = Vector3.right;
                break;
            case 4:
                angularMomentum = Vector3.forward;
                break;
            case 5:
                angularMomentum = Vector3.back;
                break;
            default:
                Debug.Log("Invalid torque direction randomized.");
                break;
        }

        angularMomentum *= angularVelocity * _rigidbody.mass * Mathf.Pow(_boxCollider.size.x, 2.0f);
        _rigidbody.useGravity = true;
        _rigidbody.AddForce(impulse, ForceMode.Impulse);
        _rigidbody.AddTorque(angularMomentum, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "TableTop")
        {
            Invoke("RaycastForResults", 1);
        }
    }

    private void RaycastForResults()
    {
        _raysFromFaces[0] = new Tuple<DiceFace, Ray>(DiceFace.One, new Ray(transform.position, transform.right));
        _raysFromFaces[1] = new Tuple<DiceFace, Ray>(DiceFace.Two, new Ray(transform.position, transform.up * -1.0f));
        _raysFromFaces[2] = new Tuple<DiceFace, Ray>(DiceFace.Three, new Ray(transform.position, transform.right * -1.0f)); // left
        _raysFromFaces[3] = new Tuple<DiceFace, Ray>(DiceFace.Four, new Ray(transform.position, transform.up));
        _raysFromFaces[4] = new Tuple<DiceFace, Ray>(DiceFace.Five, new Ray(transform.position, transform.forward * -1.0f));
        _raysFromFaces[5] = new Tuple<DiceFace, Ray>(DiceFace.Six, new Ray(transform.position, transform.forward));

        for (int i = 0; i < _raysFromFaces.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(_raysFromFaces[i].Item2, out hit, 10))
            {
                Debug.Log(hit.transform.name);
                if (hit.transform.name == "TableTop")
                {
                    Debug.Log($"You`ve got {_raysFromFaces[i].Item1}");
                    rollResult = _raysFromFaces[i].Item1;
                    if (resultReadyEvent != null) resultReadyEvent(this, null);
                }
            }
            _raysFromFaces[i] = null;
        }
    }
}

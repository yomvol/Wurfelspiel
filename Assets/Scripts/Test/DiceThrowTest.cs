using System;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

public class DiceThrowTest : MonoBehaviour
{
    [SerializeField] private float ImpulseStrength = 1; // module of the force vector
    [SerializeField] private float AngularVelocity = 0.1f;

    [Header("Tweening")]
    [SerializeField] private float AIShakeDuration = 1.5f;
    [SerializeField] private int NumberOfLoops = 4;
    [SerializeField] private float XRestraint = 0.05f; // +- 0.05
    [SerializeField] private float YRestraint = 0.05f;
    [SerializeField] private float ZRestraint = 0.04f;
    [SerializeField] private float _rotStrength = 60f;
    [SerializeField] private int _rotVibrato = 10;
    [SerializeField] private float _posStrength = 0.1f;
    [SerializeField] private int _posVibrato = 3;
    [SerializeField] private float _posRand = 20;



    public DiceFace RollResult { get; private set; }
    public event EventHandler ResultReadyEvent;

    private Tuple<DiceFace, Ray>[] _raysFromFaces;
    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;
    private MeshRenderer _renderer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        _renderer = GetComponent<MeshRenderer>();

        HideDice();
        _raysFromFaces = new Tuple<DiceFace, Ray>[6];
        RollResult = DiceFace.One;
    }

    public void HideDice()
    {
        //_renderer.enabled = false;
        _rigidbody.useGravity = false;
    }

    public void StartShaking()
    {
        // do some twerking
        transform.DOShakeRotation(duration: 100, _rotStrength, _rotVibrato, 90, false, ShakeRandomnessMode.Full);
        transform.DOShakePosition(duration: 100, _posStrength, _posVibrato, _posRand, false, false, ShakeRandomnessMode.Harmonic);
    }

    public void ThrowDiceShaken()
    {
        _renderer.enabled = true;

        // do some twerking
        float swingDuration = AIShakeDuration / NumberOfLoops;
        Vector3 endPos = new Vector3(transform.position.x + Random.Range(-XRestraint, XRestraint),
            transform.position.y + Random.Range(-YRestraint, YRestraint),
            transform.position.z + Random.Range(-ZRestraint, ZRestraint));

        transform.DOShakeRotation(AIShakeDuration, 60, 10, 90, false, ShakeRandomnessMode.Full);
        transform.DOMove(endPos, swingDuration, false).SetLoops(NumberOfLoops).SetEase(Ease.InOutQuad).OnComplete(ApplyForces);
    }

    public void ApplyForces()
    {
        transform.DOKill();

        Vector3 impulse = new Vector3();
        impulse.y = 0;
        float angleOfImpulse = Random.Range(0.01f, 2 * MathF.PI);
        impulse.x = MathF.Cos(angleOfImpulse) * ImpulseStrength;
        impulse.z = MathF.Sin(angleOfImpulse) * ImpulseStrength;
        Vector3 angularMomentum = Vector3.zero;
        int torqueDirection = Random.Range(0, 4);
        switch (torqueDirection)
        {
            case 0:
                angularMomentum = Vector3.left;
                break;
            case 1:
                angularMomentum = Vector3.right;
                break;
            case 2:
                angularMomentum = Vector3.forward;
                break;
            case 3:
                angularMomentum = Vector3.back;
                break;
            default:
                Debug.Log("Invalid torque direction randomized.");
                break;
        }

        angularMomentum *= AngularVelocity * _rigidbody.mass * Mathf.Pow(_boxCollider.size.x, 2.0f);
        //Debug.DrawLine(transform.position, impulse, Color.red);
        //Debug.DrawLine(transform.position, angularMomentum, Color.green);
        //Debug.Log(gameObject.name + " Impulse: " + impulse.ToString() + " Ang Momentum: " + angularMomentum.ToString());
        _rigidbody.useGravity = true;
        _rigidbody.AddForce(impulse, ForceMode.Impulse);
        _rigidbody.AddRelativeTorque(angularMomentum, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("TableTop"))
        {
            Invoke("RaycastForResults", 2);
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
                if (hit.transform.name == "TableTop")
                {
                    RollResult = _raysFromFaces[i].Item1;
                    if (ResultReadyEvent != null) ResultReadyEvent(this, null);
                    //Debug.Log(gameObject.name + ": " + RollResult.ToString());
                }
            }
        }
    }
}

using UnityEngine;
using System.Collections;

public class TornadoData : MonoBehaviour 
{
    public static TornadoData current;

    public float suctionForce;
    public float liftForce;
    public float rotationForce;

    public AnimationCurve forceCurve;
    public AnimationCurve forceLiftCurve;



    void Start () 
	{
        current = this;
	}
	
	

	void Update () 
	{
	
	}
}

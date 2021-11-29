using UnityEngine;
using System.Collections;

public class Debris : MonoBehaviour 
{
    //public AnimationCurve velocityCurve;

    Rigidbody debrisRB;

    //The tornado pieces
    Transform[] skeletonPiecesArray;

    Tornado tornadoScript;

    void Start () 
	{
        debrisRB = GetComponent<Rigidbody>();

        tornadoScript = GameObject.FindGameObjectWithTag("Tornado").GetComponent<Tornado>();
    }
	
	

	void Update () 
	{
        if (skeletonPiecesArray == null)
        {
            skeletonPiecesArray = tornadoScript.skeletonPiecesArray;
        }
	}




    void FixedUpdate()
    {
        if (skeletonPiecesArray != null)
        {
            float mass = debrisRB.mass;
            Vector3 position = transform.position;

            int closestIndex = FindIndex(position);

            //Now we can find the position we are going from
            Vector3 fromWp = skeletonPiecesArray[closestIndex].position;

            //If this position is the last position in the array of skeleton pieces then we go straight up
            Vector3 tornadoCenterPos = fromWp;

            //Else we have to find how far we have travelled between the waypoints
            if (closestIndex != skeletonPiecesArray.Length - 1)
            {
                Vector3 toWp = skeletonPiecesArray[closestIndex + 1].position;

                float progress = TornadoMath.CalculateProgress(fromWp, toWp, position);

                tornadoCenterPos = progress * (toWp - fromWp) + fromWp;
            }


            //The distance to the tornado
            float distance = (position - tornadoCenterPos).magnitude;

            float maxDistance = 100f;

            float propDistance = TornadoData.current.forceCurve.Evaluate(distance / maxDistance);
            float propHeight = TornadoData.current.forceLiftCurve.Evaluate(position.y / 100f);

            //propDistance *= propHeight;

            float velocity = 0f;

            if (distance < maxDistance)
            {
                velocity = propDistance * tornadoScript.tornadoSpinSpeed;
            }

            //Centrifugal force
            float centrifugalForce = CalculateCentrifulgalForce(velocity, mass, tornadoCenterPos, position);

            //In which direction to apply centrifugal force
            Vector3 dir = (position - tornadoCenterPos).normalized;

            //Add the force
            //The force is larger the higher up we are so debris dont get stuck at the top
            debrisRB.AddForce(centrifugalForce * dir * propHeight);


            //Suction force
            if (distance > 1f && distance < maxDistance)
            {
                debrisRB.AddForce(TornadoData.current.suctionForce * -dir * propDistance);
            }

            //Lift force
            if (distance < maxDistance && position.y < 100f)
            {

                if (closestIndex == skeletonPiecesArray.Length - 1)
                {
                    debrisRB.AddForce(Vector3.up * TornadoData.current.liftForce * propDistance);
                }
                //Should be in the direction of the tornado and not just up
                else
                {
                    Vector3 liftDir = (skeletonPiecesArray[closestIndex + 1].position - skeletonPiecesArray[closestIndex].position).normalized;

                    debrisRB.AddForce(liftDir * TornadoData.current.liftForce * propDistance);
                }
            }

            //RotationForce
            if (distance < maxDistance)
            {
                //the direction perpendicular
                Vector3 v = tornadoCenterPos - position;

                Vector3 perpendicular = new Vector3(-v.z, 0f, v.x) / Mathf.Sqrt((v.x * v.x) + (v.z * v.z));

                //perpendicular.y = transform.position.y;

                //perpendicular = perpendicular.normalized;

                debrisRB.AddForce(perpendicular * TornadoData.current.rotationForce * -1f * propDistance);

                //Debug.DrawLine(position, position + perpendicular * 20f, Color.blue);
            }
        }
    }




    float CalculateCentrifulgalForce(float velocity, float mass, Vector3 centerPos, Vector3 pos)
    {
        float xMinusXCenterSqr = (pos.x - centerPos.x) * (pos.x - centerPos.x);
        float zMinusZCenterSqr = (pos.z - centerPos.z) * (pos.z - centerPos.z);

        float centrifugalForce = (velocity * velocity) / Mathf.Sqrt(xMinusXCenterSqr + zMinusZCenterSqr);

        return centrifugalForce;
    }




    //Find the index of the closest skeleton piece
    public int FindIndex(Vector3 particlePos)
    {
        int index = 0;

        float bestDist = Mathf.Infinity;

        for (int i = 0; i < skeletonPiecesArray.Length; i++)
        {
            Vector3 skeletonPos = skeletonPiecesArray[i].position;

            //skeletonPos.x = particlePos.x;
            //skeletonPos.z = particlePos.z;

            float dist = Mathf.Abs(skeletonPos.y - particlePos.y);

            if (dist < bestDist)
            {
                bestDist = dist;

                index = i;
            }
        }

        //But we dont want the closest, but the position we are going from
        if (particlePos.y < skeletonPiecesArray[index].position.y)
        {
            index -= 1;
        }

        index = Mathf.Clamp(index, 0, skeletonPiecesArray.Length - 1);

        return index;
    }
}

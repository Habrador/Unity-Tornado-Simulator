using UnityEngine;
using System.Collections;

public class TornadoDust : MonoBehaviour 
{
    public GameObject groundPSObj;
    public GameObject debugCylinder;
    public GameObject debugSphere;

    //Tornado data
    //The rotation speed of the particles
    public float rotationSpeed = 10f;
    //The radius of the circle the particles are rotating around
    public float radius = 15f;
    public float tornadoHeight;
    public float topRadius;

    //The pieces the tornado consists of 
    [System.NonSerialized]
    public Transform[] skeletonPiecesArray;

    private Vector3[] lastSkeletonPiecesPosArray;

    [System.NonSerialized]
    public AnimationCurve tornadoShape;

    [System.NonSerialized]
    public Transform tornadoTrans;

    //The particle system
    ParticleSystem groundPS;
    //All particles in the ps
    ParticleSystem.Particle[] m_Particles;

    float lastRotY;


    //Should the particles grow as higher up they are, like the clouds do?
    public bool isJunk = false;


    void Start()
    {
        //Get the particle system
        groundPS = groundPSObj.GetComponent<ParticleSystem>();

        //If we do this once here then wa cant change the number of particles dynamically
        //m_Particles = new ParticleSystem.Particle[groundPS.maxParticles];
        //lastRotY = tornadoTrans.eulerAngles.y;

        //lastSkeletonPiecesPosArray = new Vector3[6];
    }


    //Important to update the particles in Late Update or some of them will get strange position when they spawn
    void LateUpdate()
    {
        if (lastSkeletonPiecesPosArray == null)
        {
            lastSkeletonPiecesPosArray = new Vector3[skeletonPiecesArray.Length];

            //Add init values
            for (int i = 0; i < skeletonPiecesArray.Length; i++)
            {
                lastSkeletonPiecesPosArray[i] = skeletonPiecesArray[i].position;
            }
        }


        //Move the ps to the first position
        Vector3 groundPos = skeletonPiecesArray[skeletonPiecesArray.Length - 1].position;

        groundPos.y = 0f;

        transform.position = groundPos;

        //Debug.Log(lastSkeletonPiecesPosArray[0]);

        debugCylinder.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);

        //If we do this here then we can change the number of particles dynamically
        m_Particles = new ParticleSystem.Particle[groundPS.maxParticles];

        RotateParticles();

        //Update the last pos array
        for (int i = 0; i < skeletonPiecesArray.Length; i++)
        {
            lastSkeletonPiecesPosArray[i] = skeletonPiecesArray[i].position;
        }
    }



    //http://docs.unity3d.com/ScriptReference/ParticleSystem.GetParticles.html
    void RotateParticles()
    {
        //Get the number of particles that are alive
        int numParticlesAlive = groundPS.GetParticles(m_Particles);

        //Debug.Log(m_Particles.Length + " " + numParticlesAlive);

        //What the particles are rotating around
        //Is not constant because the tornado is not a straight line up
        //Vector3 centerPos = transform.position;

        //Change only the particles that are alive
        for (int i = 0; i < numParticlesAlive; i++)
        {
            //Calculate the particle's current angle
            //If the ps is using local coordinates, then the position of the particle in the array is also local
            //Vector3 particlePos = groundPSObj.transform.TransformPoint(m_Particles[i].position);
            Vector3 particlePos = m_Particles[i].position;

            //Save the y coordinate because it will always be the same
            float particleY = particlePos.y;

            //if (particleY < 0.1f)
            //{
            //    particleY = 0.1f;
            //}

            //Kill the particle if it is too high up
            if (particleY > tornadoHeight)
            {
                m_Particles[i].remainingLifetime = -1f;

                continue;
            }



            //
            //Find the position of what the particle is rotating around at this height
            //
            
            //Find the index of the skeleton piece we are moving away from
            int closestIndex = FindIndex(particlePos);

            //Now we can find the position we are going from
            Vector3 fromWp = skeletonPiecesArray[closestIndex].position;

            //If this position is the last position in the array of skeleton pieces then we go straight up
            Vector3 centerPos = fromWp;

            //Else we have to find how far we have travelled between the waypoints
            if (closestIndex != skeletonPiecesArray.Length - 1)
            {
                Vector3 toWp = skeletonPiecesArray[closestIndex + 1].position;

                float progress = TornadoMath.CalculateProgress(fromWp, toWp, particlePos);

                centerPos = progress * (toWp - fromWp) + fromWp;
            }


            //Move the particle to the new pos from the last pos
            Vector3 oldFromWp = lastSkeletonPiecesPosArray[closestIndex];

            Vector3 oldCenterPos = oldFromWp;

            //Else we have to find how far we have travelled between the waypoints
            if (closestIndex != skeletonPiecesArray.Length - 1)
            {
                Vector3 oldToWp = lastSkeletonPiecesPosArray[closestIndex + 1];

                float progress = TornadoMath.CalculateProgress(oldFromWp, oldToWp, particlePos);

                oldCenterPos = progress * (oldToWp - oldFromWp) + oldFromWp;
            }


            //Move the particle to the new position - will prevent the particle from moving if we move the object
            Vector3 posChange = centerPos - oldCenterPos;

            particlePos += posChange;



            //
            //Rotate the particle
            //
            
            //Get the radius at this height
            float wantedRadius = tornadoShape.Evaluate(particleY / tornadoHeight) * topRadius + 0f;

            //The junk have larger radius
            if (isJunk)
            {
                //wantedRadius += (i / numParticlesAlive) * 10f;
                wantedRadius += 12f;
            }

            //Change the radius to the correct size
            particlePos = TornadoMath.CheckRadius(particlePos, centerPos, wantedRadius);

            //To calculate the particle's angle we have to transform the coordinate as if the centerpos was origo
            Vector3 anglePos = particlePos - centerPos;

            float currentAngle = TornadoMath.GetAngle(anglePos.x, anglePos.z);

            //Get the updated position
            particlePos = TornadoMath.GetParticlePos(centerPos, currentAngle, wantedRadius, rotationSpeed);



            //Dont change the y position
            particlePos.y = particleY;

            //Assign the new position to the particle system
            //newParticlePos = groundPSObj.transform.InverseTransformPoint(newParticlePos);

            m_Particles[i].position = particlePos;

            //Debug.Log(particlePos);
            //Debug.Log(currentAngle);
            //Debug.Log(newParticlePos);
            //Debug.Log(groundPSObj.transform.InverseTransformPoint(newParticlePos));
            //Debug.Log("");

            //Resize the particle
            if (!isJunk)
            {
                m_Particles[i].startSize = 10f + (35f * tornadoShape.Evaluate(particleY / tornadoHeight));

                //if (particleY < 1f)
                //{
                //    m_Particles[i].startSize = 0f;
                //}
            }
        }

        //Apply the particle changes to the particle system
        groundPS.SetParticles(m_Particles, numParticlesAlive);
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

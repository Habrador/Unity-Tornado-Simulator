using UnityEngine;
using System.Collections;

public class GroundDust : MonoBehaviour 
{
    public GameObject groundPSObj;
    public GameObject debugCylinder;

    //Dust data
    //The rotation speed of the particles
    public float rotationSpeed = 10f;
    //The radius of the circle the particles are rotating around
    public float bottomRadius = 15f;
    //The height of the dust
    public float height = 20f;

    //The shape of the dust
    public AnimationCurve dustShape;

    //The particle system
    ParticleSystem groundPS;
    //All particles in the ps
    ParticleSystem.Particle[] m_Particles;

    //To move the particle to the new center position
    Vector3 oldCenterPos;
    


    void Start () 
	{
        //Get the particle system
        groundPS = groundPSObj.GetComponent<ParticleSystem>();

        //If we do this once here then wa cant change the number of particles dynamically
        m_Particles = new ParticleSystem.Particle[groundPS.maxParticles];

        oldCenterPos = transform.position;
    }
	
	

	void LateUpdate() 
	{
        debugCylinder.transform.localScale = new Vector3(bottomRadius * 2f, 1f, bottomRadius * 2f);

        //Move the ps to the first position
        //Vector3 groundPos = skeletonPiecesArray[skeletonPiecesArray.Length - 1].position;

        //groundPos.y = 0f;

        //transform.position = groundPos;

        RotateParticles();

        oldCenterPos = transform.position;
    }



    //http://docs.unity3d.com/ScriptReference/ParticleSystem.GetParticles.html
    void RotateParticles()
    {
        //Get the number of particles that are alive
        int numParticlesAlive = groundPS.GetParticles(m_Particles);

        //Debug.Log(m_Particles.Length + " " + numParticlesAlive);

        //Change only the particles that are alive
        for (int i = 0; i < numParticlesAlive; i++)
        {
            //Get the position of the particle
            //If the ps is using local coordinates, then the position of the particle in the array is also local
            //Vector3 particlePos = groundPSObj.transform.TransformPoint(m_Particles[i].position);
            Vector3 particlePos = m_Particles[i].position;

            //Save the y coordinate because it will always be the same
            float particleY = particlePos.y;

            //Deactivate the particle if it's above a certain height
            if (particleY > height)
            {
                m_Particles[i].remainingLifetime = -1f;

                continue;
            }


            //What the particle is rotating around
            Vector3 centerPos = transform.position;


            //Move the particle to the new position - will prevent the particle from moving if we move the object
            Vector3 posChange = centerPos - oldCenterPos;

            particlePos += posChange;



            //Rotate the particle
            //Get the radius at this height
            float wantedRadius = dustShape.Evaluate(particleY / height) * bottomRadius + 0.5f;

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
            //newParticlePos = groundPSObj.transform.InverseTransformPoint(particlePos);

            m_Particles[i].position = particlePos;

            //Debug.Log(particlePos);
            //Debug.Log(currentAngle);
            //Debug.Log(newParticlePos);
            //Debug.Log(groundPSObj.transform.InverseTransformPoint(newParticlePos));
            //Debug.Log("");
        }

        //Apply the particle changes to the particle system
        groundPS.SetParticles(m_Particles, numParticlesAlive);
    }
}

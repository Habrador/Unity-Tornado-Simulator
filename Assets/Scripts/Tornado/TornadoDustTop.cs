using UnityEngine;
using System.Collections;

//This is the top of the tornado where the particles will slow down as they spread out 
//and sort of morph into the surrounding clouds
public class TornadoDustTop : MonoBehaviour 
{
    public GameObject groundPSObj;
    public GameObject debugCylinder;

    //Dust data
    //The rotation speed of the particles
    public float rotationSpeed = 10f;
    //The radius of the circle the particles are rotating around
    public float bottomRadius = 15f;
    //The top radius
    public float topRadius = 50f;
    //The height of the dust
    public float height = 40f;
    //The height of the tornado;
    public float tornadoHeight;

    //The shape of the dust
    public AnimationCurve dustShape;
    //The speed of the dust
    //public AnimationCurve dustSpeedChange;

    //The particle system
    ParticleSystem groundPS;
    //All particles in the ps
    ParticleSystem.Particle[] m_Particles;

    //To move the particle to the new center position
    Vector3 oldCenterPos;



    void Start()
    {
        //Get the particle system
        groundPS = groundPSObj.GetComponent<ParticleSystem>();

        //If we do this once here then wa cant change the number of particles dynamically
        m_Particles = new ParticleSystem.Particle[groundPS.maxParticles];

        oldCenterPos = transform.position;
    }



    void LateUpdate()
    {
        //Change the shape of the ps
        var x = groundPS.shape;
        x.radius = bottomRadius;

        debugCylinder.transform.localScale = new Vector3(bottomRadius * 2f, 1f, bottomRadius * 2f);

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
            if (particleY > height + tornadoHeight + 20f)
            {
                m_Particles[i].remainingLifetime = -1f;

                continue;
            }

            //To make the top cloud particles fade out
            if (particleY > height + tornadoHeight)
            {
                Color32 currentColor = m_Particles[i].startColor;

                currentColor.a = 150;

                m_Particles[i].startColor = currentColor;
            }


            //What the particle is rotating around
            Vector3 centerPos = transform.position;


            //Move the particle to the new position - will prevent the particle from moving if we move the object
            Vector3 posChange = centerPos - oldCenterPos;

            particlePos += posChange;



            //Rotate the particle
            //Get the radius at this height
            float wantedRadius = (dustShape.Evaluate((particleY - tornadoHeight) / height) * (topRadius - bottomRadius)) + bottomRadius;

            //Change the radius to the correct size
            particlePos = TornadoMath.CheckRadius(particlePos, centerPos, wantedRadius);

            //To calculate the particle's angle we have to transform the coordinate as if the centerpos was origo
            Vector3 anglePos = particlePos - centerPos;

            float currentAngle = TornadoMath.GetAngle(anglePos.x, anglePos.z);

            //Calculate the speed at this height
            float wantedSpeed = (1f - dustShape.Evaluate((particleY - tornadoHeight) / height)) * rotationSpeed;


            //Get the updated position
            particlePos = TornadoMath.GetParticlePos(centerPos, currentAngle, wantedRadius, wantedSpeed);


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

            //Change size
            //m_Particles[i].startSize = 10f + (35f * tornadoShape.Evaluate(particleY / tornadoHeight));
            m_Particles[i].startSize = (150f * dustShape.Evaluate((particleY - tornadoHeight) / height)) + 45f;
        }

        //Apply the particle changes to the particle system
        groundPS.SetParticles(m_Particles, numParticlesAlive);
    }
}

using UnityEngine;
using System.Collections;

public static class TornadoMath 
{
    //Get the angle from two positions (like unity's y angle but inverted, so begins up from x axis)
    public static float GetAngle(float x, float y)
    {
        //http://answers.unity3d.com/questions/1032673/how-to-get-0-360-degree-from-two-points.html

        float angle = (Mathf.Atan2(y, x) / Mathf.PI) * 180;

        if (angle < 0f)
        {
            angle += 360f;
        }

        //Debug.Log(angle);

        //Unity's angles are flipped, so use this if needed
        //angle = 360f - angle;

        return angle;
    }



    //Get the particle's new postion with a certain rotation speed
    public static Vector3 GetParticlePos(Vector3 centerPos, float angle, float radius, float rotationSpeed)
    {
        //Increase the angle with the rotationSpeed
        angle += rotationSpeed * Time.deltaTime;
        //angle += 0f;

        float angleInRad = angle * Mathf.Deg2Rad;

        //Calculate the new positions
        float newX = centerPos.x + radius * Mathf.Cos(angleInRad);
        //float newY = currentPos.y + speedY * Time.deltaTime;
        float newZ = centerPos.z + radius * Mathf.Sin(angleInRad);

        Vector3 newPos = new Vector3(newX, 0f, newZ);

        return newPos;
    }



    public static Vector3 GetParticleVel(float angle, float radius, float rotationSpeed)
    {
        //Increase the angle with the rotationSpeed
        angle += rotationSpeed * Time.deltaTime;
        //angle += 0f;

        float angleInRad = angle * Mathf.Deg2Rad;

        //Calculate the new positions
        float newX = Mathf.Cos(angleInRad) - radius * Mathf.Sin(angleInRad);
        //float newY = currentPos.y + speedY * Time.deltaTime;
        float newZ = Mathf.Sin(angleInRad) + radius * Mathf.Cos(angleInRad);

        Vector3 newVel = new Vector3(newX, 0f, newZ);

        return newVel;
    }



    /// <summary>
    /// Calculate how far we have progressed on the segment from
    /// the waypoint to the waypoint we are heading towards
    /// </summary>
    /// <param name="a">The coordinate we are going from</param>
    /// <param name="b">The cooridnate we are going to</param>
    /// <param name="c">The current position of the object we are tracking</param>
    /// <returns></returns>
    public static float CalculateProgress(Vector3 from, Vector3 to, Vector3 current)
    {
        //http://forum.unity3d.com/threads/calculate-a-percentage-of-a-distance-between-two-points.217683/
        //https://www.udacity.com/course/viewer#!/c-cs373/l-48696626/e-48403941/m-48716166

        //float Rx = currentPos.x - P1.x;
        //float Rz = currentPos.z - P1.z;

        //float deltaX = P2.x - P1.x;
        //float deltaZ = P2.z - P1.z;

        //If this is > 1 then we have passed the point we are going towards
        //float progress = ((Rx * deltaX) + (Rz * deltaZ)) / ((deltaX * deltaX) + (deltaZ * deltaZ));

        Vector3 a = from;
        Vector3 b = to;
        Vector3 c = current;


        Vector3 ab = b - a;
        Vector3 ac = c - a;

        float progress = Vector3.Dot(ac, ab) / ab.sqrMagnitude;

        //To get the coordinate of this progress point, we do:
        //Vector3 progressCoordinate = progress * (b - a) + a;

        return progress;
    }



    //To make sure the radius is correct at this height
    public static Vector3 CheckRadius(Vector3 particlePos, Vector3 centerPos, float wantedRadius)
    {
        //Make sure y is always 0
        particlePos.y = 0f;
        centerPos.y = 0f;

        //Get the current radius
        float currentRadius = (particlePos - centerPos).magnitude;

        //To change the radius we need the direction
        Vector3 dir = (particlePos - centerPos).normalized;

        //Make sure the radius is always increasing
        if (currentRadius < wantedRadius)
        {
            particlePos += dir * (wantedRadius - currentRadius);
        }

        //if (i == 0)
        //{
        //    //Debug.Log(wantedRadius + " " + (particlePos - centerPos).magnitude);
        //}

        return particlePos;
    }
}

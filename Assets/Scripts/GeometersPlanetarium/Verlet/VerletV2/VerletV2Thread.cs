﻿using UnityEngine;

public class VerletV2Thread : AbstractThread
{
    public Vector3d a;
    public float m;
    public float[] m2;
    public Vector3d outputPosition;
    public Vector3d p;
    public Vector3d[] p2;
    public Vector3d pp; //The previous position of the body
    public float pts; //The previous timestep for time corrected verlet
    public int thisIndex;
    public float ts; //The length of the timestep in seconds

    protected override void ThreadedFunction()
    {
        verletIntegration();
    }

    protected override Vector3d OnFinished()
    {
        return p;
    }

    public void verletIntegration()
    {
        calculateAcceleration(); //Calculates acceleration
        var tp = p; //Saves the position for next calculation BELOW:The Verlet Calculation
        p = p + (p - pp) * (ts / pts) + a * ts * ts;
        pts = ts; //Saves the timestep for next calculation (ABOVE) The actual time adjusted verlet algorithm
        pp = tp; //Saves the position for next calculation
    }

    public void calculateAcceleration()
    {
        var ForceVector = Vector3d.zero;
        double d = 0;
        for (var i = 0; i < m2.Length; i++)
            //For the rest of the objects...
            if (thisIndex != i)
            {
                //If the object isn't this object
                var mO = p2[i];
                var m1 = m2[i];
                d = Mathd.Sqrt(Mathd.Pow(p.x - mO.x, 2) + Mathd.Pow(p.y - mO.y, 2) +
                               Mathd.Pow(p.z - mO.z, 2));
                var Force = 1.9934976 * Mathd.Pow(10, -44) * (m * m1) *
                            (1 / Mathd.Pow(d,
                                 2)); //This and ABOVE calculate force of gravity and distance between bodies respectively

                var
                    thisVector =
                        (mO - p).normalized *
                        Force; //Updates the velocity vector based on each body, thus calculating all relationships
                ForceVector = ForceVector + thisVector;
            }

        a = ForceVector * (1 / m); //Calculates the actual accleration
    }
}
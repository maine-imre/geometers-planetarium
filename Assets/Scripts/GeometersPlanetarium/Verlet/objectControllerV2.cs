﻿using UnityEngine;

namespace IMRE.HandWaver.Space.BigBertha
{
    /// <summary>
    ///     This script does ___.
    ///     The main contributor(s) to this script is TB
    ///     Status: WORKING
    /// </summary>
    public class objectControllerV2 : MonoBehaviour
    {
        public double counter;
        public float dt = 0.001f;
        public GravationalObjectV2[] objects;
        public double scale = 1;

        public double stepsPerFrame = 28800;

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            for (var i = 0; i < stepsPerFrame; i++)
            {
                counter += dt;
                updatePositions();
            }
        }

        private void updatePositions()
        {
            if (objects.Length == 0)
            {
            }
            else
            {
                for (var a = 0; a < objects.Length - 1; a++)
                for (var b = a + 1; b < objects.Length; b++)
                    fGrav(objects[a], objects[b]);
                for (var i = 0; i < objects.Length; i++)
                {
                    objects[i].scale = scale;
                    objects[i].x += objects[i].VVec.x;
                    objects[i].y += objects[i].VVec.y;
                    objects[i].z += objects[i].VVec.z;
                }
            }
        }

        private void fGrav(GravationalObjectV2 a, GravationalObjectV2 b)
        {
            var distance = Mathd.Sqrt(Mathd.Pow(b.x - a.x, 2) +
                                      Mathd.Pow(b.y - a.y, 2) +
                                      Mathd.Pow(b.z - a.z, 2));
            var F = (float) (6.67408 * Mathd.Pow(10, -11)) *
                    (float) (a.mass * b.mass / Mathd.Pow(distance, 2)); //meters, kg, seconds
            //Debug.Log(F);

            //Calculate new vector for a
            var veca = new Vector3d(b.x, b.y, b.z) -
                       new Vector3d(a.x, a.y, a.z); //direction vector
            veca = veca.normalized * (F * dt / a.mass); //force vector
            //veca = veca*(dt/a.mass);
            a.VVec = a.VVec + veca;

            //Calculate new vector for b
            var vecb = new Vector3d(a.x, a.y, a.z) -
                       new Vector3d(b.x, b.y, b.z); //direction vector
            vecb = vecb.normalized * (F * dt / b.mass); //force vector
            //vecb = (vecb*dt)/b.mass;
            b.VVec = b.VVec + vecb;
        }
    }
}
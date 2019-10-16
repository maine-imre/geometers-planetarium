﻿/*
 * V4.0 Released 08.16.18 By Timothy Bruce
 * It pretty much works I guess. This version only works to the PREVIOUS HOUR. Seconds have not been implemented.
 * Example of calling this script:
 * generateData(DateTime.Now);			//Will get the data for the current time.
 * To get the outputted data, look in the "planets" when planetsHaveValues is true.
 *
 * V4.1 Released 08.28.18 By Timothy Bruce
 * Added to the Minute, Second, and Millisecond accuracy. 
 * Also, I think this version has been edited by IMRE for HandWaver.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace IMRE.HandWaver.Space
{
    /// <summary>
    /// </summary>
    public class planetData
    {
        public int id;
        public string name; //Name of the object

        //Storage system for the data related to the object
        public Vector3 position; //The position of the object
        public string rawData; //The complete data from horizons for the object.
        public string time; //Timestamp of the data
        public Vector3 velocity; //Velocity of the object in case you want it

        public override string ToString()
        {
            //Handy tostring for hoomans to know that the code is working.
            return string.Format("Name:		{0}\nPosition:		{1}\nVelocity:		{2}\nTimestamp:	{3}"
                , name
                , position.ToString()
                , velocity.ToString()
                , time);
        }
    }

    /// <summary>
    /// </summary>
    public class Horizons : MonoBehaviour
    {
        public static Horizons ins;

        public static int[] ids = {10, 399, 301}; //This is the list of objects that the function will calling.

        public static Action planetsDataUpdated;

        private static readonly List<planetData> planets =
            new List<planetData>(); //This is where each planet object is stored.

        public static bool planetsHaveValues; //Is true when planets should be populated with nonzero values.

        private static WWW www; //The www object for the horizons database.

        /// <summary>
        ///     Output that dynamically updates?
        ///     MAYBE NULL
        /// </summary>
        public static List<planetData> Planets =>
            planetsHaveValues ? planets : new List<planetData>();

        private void Start()
        {
            ins = this;
        }

        public void generateData(DateTime time)
        {
            //Call this script to generate the planetData objects for every value in ids
            if (planets.Count < ids.Length)
                foreach (var id in ids) //For all ids
                {
                    var body = new planetData(); //Create an object
                    body.id = id;
                    planets.Add(body); //Add the object to the list
                }

            //keep this from adding more bodies to the script.
            planets.ForEach(p => StartCoroutine(getData(time, p.id, p)));
            StartCoroutine(
                updateData()); //When all values are recived from the interwebz, populate the fields in the object
        }

        private IEnumerator getData(DateTime time, int bodyID, planetData body)
        {
            //Gets the data from the horizons server and populates the raw data field of the appropriate object
            var www =
                UnityWebRequest.Get(generateURL(DateTime.Now, bodyID));
            yield return www.SendWebRequest(); //Wait for return fromrequest

            if (www.isNetworkError || www.isHttpError) //If error, output error
            {
                Debug.Log(www.error);
            }
            else //Else populate rawData
            {
                body.rawData = www.downloadHandler.text;
                body.id = bodyID;
            }
        }

        private IEnumerator updateData()
        {
            //Populates the fields other than raw data once rawData has been populated
            var flag = false;
            while (!flag)
            {
                //Loops while the fields have not been populated
                var count = 0; //This bit checks if rawData has been populated for everything
                foreach (var body in planets)
                    if (body.rawData != null)
                        count += 1;

                if (count == planets.Count)
                {
                    flag = true;
                    foreach (var body in planets) //When the rawData is populated, for every object:
                    {
                        //(below) Split by lines
                        var lines = body.rawData.Split('\r', '\n');
                        var name = Regex.Replace(lines[1], @"\s+", " ")
                            .Split(null)[5];
                        //(above) get the name
                        var counter = 0;
                        foreach (var line in lines)
                        {
                            if (line == "$$SOE") //When start of data is encountered,
                                readData(lines[counter + 1], name, body);
                            counter++;
                        }

                        if (RSDESManager.verboseLogging) Debug.Log(body.ToString());
                        planetsHaveValues = true; //Mark that the planets have values.
                        if (planetsDataUpdated != null && planetsDataUpdated.Method != null)
                            planetsDataUpdated.Invoke();
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        private static void readData(string input, string name, planetData body)
        {
            var data = input.Split(','); //Populates the body object with the position data.
            body.time = data[1];
            body.position = new Vector3(float.Parse(data[2]), float.Parse(data[4]), float.Parse(data[3]));
            body.velocity = new Vector3(float.Parse(data[5]), float.Parse(data[7]), float.Parse(data[6]));
            body.name = name;
        }

        private static string generateURL(DateTime time, int bodyID)
        {
            //Generates a url to access using the inputted time and bodyID number.
            var url = string.Format(
                "https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1%20&COMMAND=%27{0}%27%20&TABLE_TYPE=%27VECTORS%27%20&CENTER=%27399%27%20&START_TIME=%27{1}STOP_TIME=%27{2}STEP_SIZE=%2760%20min%27%20&OUT_UNITS%20%20=%20%27KM-D%27%20&VEC_TABLE%20=%20%273%27%20&CSV_FORMAT=%27YES%27"
                , bodyID
                , ins.generateDateInput(time)
                , ins.generateDateInput(time.AddHours(1)));
            return url;
        }

        private string generateDateInput(DateTime time)
        {
            //Generates a date field in the format that the horizons database likes.
            var targetDateTime = time.Year + "-";

            if (time.Month < 10) targetDateTime += "0";
            targetDateTime += time.Month + "-";

            if (time.Day < 10) targetDateTime += "0";
            targetDateTime += time.Day + "%20";
            if (time.Hour < 10) targetDateTime += "0";
            targetDateTime += time.Hour + ":";
            if (time.Minute < 10) targetDateTime += "0";
            targetDateTime += time.Minute + ":";
            if (time.Second < 10) targetDateTime += "0";
            targetDateTime += time.Second + ".";
            if (time.Millisecond < 10)
                targetDateTime += "000";
            else if (time.Millisecond < 100)
                targetDateTime += "00";
            else if (time.Millisecond < 1000) targetDateTime += "0";
            targetDateTime += time.Millisecond + "%27%20&";
            return targetDateTime;
        }
    }
}
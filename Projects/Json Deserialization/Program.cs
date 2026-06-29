using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

using GHIElectronics.TinyCLR.Data.Json;
using GHIElectronics.TinyCLR.Native;

namespace JsonDeserialization
{
    internal class Program
    {
        // This text is a result of a call to : https://api.openweathermap.org/data/2.5/weather?lat=42.49&lon=83.11&appid={your api key here}
        // Documented here : https://openweathermap.org/current
        const string JsonText = "{\"coord\":{\"lon\":-83.11,\"lat\":42.49},\"weather\":[{\"id\":800,\"main\":\"Clear\",\"description\":\"clear sky\",\"icon\":\"01d\"}],\"base\":\"stations\",\"main\":{\"temp\":266.74,\"feels_like\":266.74,\"temp_min\":265.88,\"temp_max\":267.99,\"pressure\":1041,\"humidity\":62,\"sea_level\":1041,\"grnd_level\":1014},\"visibility\":10000,\"wind\":{\"speed\":1.03,\"deg\":0},\"clouds\":{\"all\":0},\"dt\":1734112973,\"sys\":{\"type\":2,\"id\":2043784,\"country\":\"US\",\"sunrise\":1734094446,\"sunset\":1734127192},\"timezone\":-18000,\"id\":5000500,\"name\":\"Madison Heights\",\"cod\":200}";

#pragma warning disable IDE1006 // Naming Styles - properties that start with lower case letter

        internal class OwxCoordinates {
            public float lon { get; set; }
            public float lat { get; set; }
        }

        internal class OwxWeather {
            public uint id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }

        internal class OwxMain {
            public float temp { get; set; }
            public float feels_like { get; set; }
            public float temp_min { get; set; }
            public float temp_max { get; set; }
            public uint pressure { get; set; }
            public uint humidity { get; set; }
            public uint sea_level { get; set; }
            public uint grnd_level { get; set; }
        }

        internal class OwxWind {
            public float speed { get; set; }
            public int deg { get; set; }
        }

        internal class OwxSys {
            public int type { get; set; }
            public uint id { get; set; }
            public string country { get; set; }
            public uint sunrise { get; set; }
            public uint sunset { get; set; }
        }

        internal class OpenWeatherReport {
            public OwxCoordinates coord { get; set; }
            public OwxWeather[] weather { get; set; }
            public string @base { get; set; }
            public OwxMain main { get; set; }
            public uint visibility { get; set; }
            public OwxWind wind { get; set; }
            public ulong dt { get; set; }
            public OwxSys sys { get; set; }
            public int timezone { get; set; }
            public uint id { get; set; }
            public string name { get; set; }
            public uint cod { get; set; }
        }
#pragma warning restore IDE1006 //  Naming Styles - properties that start with lower case letter

        /// <summary>
        /// Because TinyCLR doesn't give us enough type information about arrays, we have to have a
        /// helper function that will instantiate the array and each member in the array. We will get
        /// called for every type being instantiated, but we only have to return instances for arrays
        /// and members in arrays, though you could also use this to return child types for polymorphic
        /// members of classes.
        /// </summary>
        /// <param name="path">The full json path to the member being created.</param>
        /// <param name="root">The root token for the json currently being deserialized. You could inspect this to pull
        /// out dynamic information (e.g., structures not known at compile time)
        /// <param name="baseType">The type of the object we are deserializing into. This will be null when instantiating
        /// the individual elements of an array, so you must use the path to decide what type of element to instantiate.</param>
        /// <param name="name">not used</param>
        /// <param name="length">When instantiating arrays, this will be the number of elements we need to allocate.</param>
        /// <returns></returns>
        private static object WxCreateInstance(string path, JToken root, Type baseType, string name, int length) {
            // A little debug output to help you with decoding your own types...
            if (baseType != null) {
                Debug.Write("Base type = " + baseType.FullName + "  ");
            }
            Debug.WriteLine("Path = " + path + "  Name = " + name + "  Length = " + length);

            // Allocate arrays and members of arrays (we can ignore all other cases and return null)
            switch (path) {
                case "/":
                    switch (name) {
                        case "weather":
                            // The 'weather' element at the root of the json is an array. Allocate the
                            // array space. Length will be >= 0
                            return new OwxWeather[length];
                    }
                    break;
                case "//weather":
                    // This is asking us to allocate a single element to store in the 'weather' array
                    // at the root of the json tree. Length will be -1 and the baseType will be null.
                    // If TinyCLR would tell us the baseType, we wouldn't need this CreateInstance routine
                    // at all, but this issue dates back to the origin of netMF.
                    return new OwxWeather();
            }
            return null;
        }

        static void Main() {
            var weather = (OpenWeatherReport)JsonConverter.DeserializeObject(JsonText, typeof(OpenWeatherReport), WxCreateInstance);
            Debug.WriteLine("Current Temperature (K) is " + weather.main.temp + " but it feels like " + weather.main.feels_like);
        }
    }
}

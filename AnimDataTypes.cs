
namespace AnimDataNS
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class AnimDataTypes
    {
        public enum DataType
        {
            LocalRotation,
            LocalPosition,
            Other
        }

        [System.Serializable]
        public class DataCurve
        {
            public string path;
            public string pathRaw;
            public string propertyName;
            public int count;
            public List<AnimationCurve> curves;
            public DataType dataType = DataType.Other;

            public virtual float[] Evaluate(float time)
            {
                float[] output = new float[curves.Count];
                for (var i = 0; i < curves.Count; ++i)
                    output[i] = curves[i].Evaluate(time);
                return output;
            }

            public float TotalTime() => EvalTime( count - 1 );

            public virtual float EvalTime(int index) => curves[0].keys[index].time;
            public virtual float[] EvalAt(int index) => Evaluate(EvalTime(index));


            public Vector3 ToPosition(float[] input) => new Vector3(input[0], input[1], input[2]);
            public Quaternion ToRotation(float[] input) => new Quaternion(input[0], input[1], input[2], input[3]);

            public Vector3 ToEulerAngles( Quaternion q )
            {
                var e = q.eulerAngles;
                while (e.x > 180) e.x -= 360;
                while (e.y > 180) e.y -= 360;
                while (e.z > 180) e.z -= 360;
                while (e.x < -180) e.x += 360;
                while (e.y < -180) e.y += 360;
                while (e.z < -180) e.z += 360;
                return e;
            }
        }
    }
}

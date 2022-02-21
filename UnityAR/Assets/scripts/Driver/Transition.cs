using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition
{
    public const string LINEAR = "LINEAR";
    public const string EXPONENTIAL = "EXPONENTIAL";
    public const string LOGARITHMIC = "LOGARITHMIC";

    public static float Exponential(float coeff)
	{
        float minExpPower = -2.0f;
        float minExpValue = Mathf.Exp(minExpPower);
        float maxExpPower = 2.0f;
        float maxExpValue = Mathf.Exp(maxExpPower);
        float expPower = Mathf.Lerp(minExpPower, maxExpPower, coeff);
        float expValue = Mathf.Exp(expPower);
        float res = (expValue - minExpValue) / (maxExpValue - minExpValue);
        //Debug.Log("Exponential() " + coeff + " => " +  res);
        return res;
    }
    public static float Logarithmic(float coeff)
    {
        float minLogPower = -0.1f;
        float minLogValue = Mathf.Log(minLogPower);
        float maxLogPower = 8.0f;
        float maxLogValue = Mathf.Log(maxLogPower);
        float logPower = Mathf.Lerp(minLogPower, maxLogPower, coeff);
        float logValue = Mathf.Log(logPower);
        return (logValue - minLogValue) / (maxLogValue - minLogValue);
    }
}

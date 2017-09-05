using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Simple implementation of the Kalman Filter for 1D data, without any dependencies
 * Originally written in JavaScript by Wouter Bulten
 * 
 * Now rewritten into Java
 * 2017
 * 
 * @license GNU LESSER GENERAL PUBLIC LICENSE v3
 *
 * @author Sifan Ye
 * 
 * @see https://github.com/wouterbulten/kalmanjs
 *
 */
public class KalmanFilter
{
    public static KalmanFilter _instance;

    private float A = 1;
    private float B = 0;
    private float C = 1;

    private float R;
    private float Q;

    private float cov = float.NaN;
    private float x = float.NaN;

    /**
	 * Constructor
	 * 
	 * @param R Process noise
	 * @param Q Measurement noise
	 */
    public KalmanFilter(float R, float Q)
    {
        this.R = R;
        this.Q = Q;
    }

    /**
	 * Filters a measurement
	 * 
	 * @param measurement The measurement value to be filtered
	 * @return The filtered value
	 */
    public float Filter(float measurement)
    {
        float u = 0;
        if (float.IsNaN(this.x))
        {
            this.x = (1 / this.C) * measurement;
            this.cov = (1 / this.C) * this.Q * (1 / this.C);
        }
        else
        {
            float predX = (this.A * this.x) + (this.B * u);
            float predCov = ((this.A * this.cov) * this.A) + this.R;

            // Kalman gain
            float K = predCov * this.C * (1 / ((this.C * predCov * this.C) + this.Q));

            // Correction
            this.x = predX + K * (measurement - (this.C * predX));
            this.cov = predCov - (K * this.C * predCov);
        }
        return this.x;
    }

    /**
	 * 
	 * @return The last measurement fed into the filter
	 */
    public float LastMeasurement()
    {
        return this.x;
    }

    /**
	 * Sets measurement noise
	 *
	 * @param noise The new measurement noise
	 */
    public void SetMeasurementNoise(float noise)
    {
        this.Q = noise;
    }

    /**
	 * Sets process noise
	 *
	 * @param noise The new process noise
	 */
    public void SetProcessNoise(float noise)
    {
        this.R = noise;
    }
}

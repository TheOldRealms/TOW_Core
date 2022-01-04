﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace TOW_Core.Battle.Artillery
{
    public class BallisticSolver
    {
        // SolveQuadric, SolveCubic, and SolveQuartic were ported from C as written for Graphics Gems I
        // Original Author: Jochen Schwarze (schwarze@isa.de)
        // https://github.com/erich666/GraphicsGems/blob/240a34f2ad3fa577ef57be74920db6c4b00605e4/gems/Roots3And4.c

        // Utility function used by SolveQuadratic, SolveCubic, and SolveQuartic
        private static bool IsZero(double d)
        {
            const double eps = 1e-9;
            return d > -eps && d < eps;
        }

        private static double GetCubicRoot(double value)
        {
            if (value > 0.0)
            {
                return System.Math.Pow(value, 1.0 / 3.0);
            }
            else if (value < 0)
            {
                return -System.Math.Pow(-value, 1.0 / 3.0);
            }
            else
            {
                return 0.0;
            }
        }

        // Solve quadratic equation: c0*x^2 + c1*x + c2. 
        // Returns number of solutions.
        public static int SolveQuadric(double c0, double c1, double c2, out double s0, out double s1)
        {
            s0 = double.NaN;
            s1 = double.NaN;

            double p, q, D;

            /* normal form: x^2 + px + q = 0 */
            p = c1 / (2 * c0);
            q = c2 / c0;

            D = p * p - q;

            if (IsZero(D))
            {
                s0 = -p;
                return 1;
            }
            else if (D < 0)
            {
                return 0;
            }
            else /* if (D > 0) */
            {
                double sqrt_D = System.Math.Sqrt(D);

                s0 = sqrt_D - p;
                s1 = -sqrt_D - p;
                return 2;
            }
        }

        // Solve cubic equation: c0*x^3 + c1*x^2 + c2*x + c3. 
        // Returns number of solutions.
        public static int SolveCubic(double c0, double c1, double c2, double c3, out double s0, out double s1, out double s2)
        {
            s0 = double.NaN;
            s1 = double.NaN;
            s2 = double.NaN;

            int num;
            double sub;
            double A, B, C;
            double sq_A, p, q;
            double cb_p, D;

            /* normal form: x^3 + Ax^2 + Bx + C = 0 */
            A = c1 / c0;
            B = c2 / c0;
            C = c3 / c0;

            /*  substitute x = y - A/3 to eliminate quadric term:  x^3 +px + q = 0 */
            sq_A = A * A;
            p = 1.0 / 3 * (-1.0 / 3 * sq_A + B);
            q = 1.0 / 2 * (2.0 / 27 * A * sq_A - 1.0 / 3 * A * B + C);

            /* use Cardano's formula */
            cb_p = p * p * p;
            D = q * q + cb_p;

            if (IsZero(D))
            {
                if (IsZero(q)) /* one triple solution */
                {
                    s0 = 0;
                    num = 1;
                }
                else /* one single and one double solution */
                {
                    double u = GetCubicRoot(-q);
                    s0 = 2 * u;
                    s1 = -u;
                    num = 2;
                }
            }
            else if (D < 0) /* Casus irreducibilis: three real solutions */
            {
                double phi = 1.0 / 3 * System.Math.Acos(-q / System.Math.Sqrt(-cb_p));
                double t = 2 * System.Math.Sqrt(-p);

                s0 = t * System.Math.Cos(phi);
                s1 = -t * System.Math.Cos(phi + System.Math.PI / 3);
                s2 = -t * System.Math.Cos(phi - System.Math.PI / 3);
                num = 3;
            }
            else /* one real solution */
            {
                double sqrt_D = System.Math.Sqrt(D);
                double u = GetCubicRoot(sqrt_D - q);
                double v = -GetCubicRoot(sqrt_D + q);

                s0 = u + v;
                num = 1;
            }

            /* resubstitute */
            sub = 1.0 / 3 * A;

            if (num > 0) s0 -= sub;
            if (num > 1) s1 -= sub;
            if (num > 2) s2 -= sub;

            return num;
        }

        // Solve quartic function: c0*x^4 + c1*x^3 + c2*x^2 + c3*x + c4. 
        // Returns number of solutions.
        public static int SolveQuartic(double c0, double c1, double c2, double c3, double c4, out double s0, out double s1, out double s2, out double s3)
        {
            s0 = double.NaN;
            s1 = double.NaN;
            s2 = double.NaN;
            s3 = double.NaN;

            double[] coeffs = new double[4];
            double z, u, v, sub;
            double A, B, C, D;
            double sq_A, p, q, r;
            int num;

            /* normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0 */
            A = c1 / c0;
            B = c2 / c0;
            C = c3 / c0;
            D = c4 / c0;

            /*  substitute x = y - A/4 to eliminate cubic term: x^4 + px^2 + qx + r = 0 */
            sq_A = A * A;
            p = -3.0 / 8 * sq_A + B;
            q = 1.0 / 8 * sq_A * A - 1.0 / 2 * A * B + C;
            r = -3.0 / 256 * sq_A * sq_A + 1.0 / 16 * sq_A * B - 1.0 / 4 * A * C + D;

            if (IsZero(r))
            {
                /* no absolute term: y(y^3 + py + q) = 0 */

                coeffs[3] = q;
                coeffs[2] = p;
                coeffs[1] = 0;
                coeffs[0] = 1;

                num = SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);
            }
            else
            {
                /* solve the resolvent cubic ... */
                coeffs[3] = 1.0 / 2 * r * p - 1.0 / 8 * q * q;
                coeffs[2] = -r;
                coeffs[1] = -1.0 / 2 * p;
                coeffs[0] = 1;

                SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);

                /* ... and take the one real solution ... */
                z = s0;

                /* ... to build two quadric equations */
                u = z * z - r;
                v = 2 * z - p;

                if (IsZero(u))
                    u = 0;
                else if (u > 0)
                    u = System.Math.Sqrt(u);
                else
                    return 0;

                if (IsZero(v))
                    v = 0;
                else if (v > 0)
                    v = System.Math.Sqrt(v);
                else
                    return 0;

                coeffs[2] = z - u;
                coeffs[1] = q < 0 ? -v : v;
                coeffs[0] = 1;

                num = SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);

                coeffs[2] = z + u;
                coeffs[1] = q < 0 ? v : -v;
                coeffs[0] = 1;

                if (num == 0) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);
                else if (num == 1) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s1, out s2);
                else if (num == 2) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s2, out s3);
            }

            /* resubstitute */
            sub = 1.0 / 4 * A;

            if (num > 0) s0 -= sub;
            if (num > 1) s1 -= sub;
            if (num > 2) s2 -= sub;
            if (num > 3) s3 -= sub;

            return num;
        }


        // Calculate the maximum range that a ballistic projectile can be fired on given speed and gravity.
        //
        // speed (float): projectile velocity
        // gravity (float): force of gravity, positive is down
        // initial_height (float): distance above flat terrain
        //
        // return (float): maximum range
        public static float GetMaxRange(float speed, float gravity, float height_difference)
        {
            // Derivation
            //   (1) y = speed * time * cos O
            //   (2) z = initial_height + (speed * time * sin O) - (.5 * gravity*time*time)
            //   (3) via quadratic: t = (speed*sin O)/gravity + sqrt(speed*speed*sin O + 2*gravity*initial_height)/gravity    [ignore smaller root]
            //   (4) solution: range = x = (speed*cos O)/gravity * sqrt(speed*speed*sin O + 2*gravity*initial_height)    [plug t back into x=speed*time*cos O]
            float angle = 45 * MathF.DegToRad; // no air resistence, so 45 degrees provides maximum range
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            float range = (speed * cos / gravity) * (speed * sin + MathF.Sqrt(speed * speed * sin * sin + 2 * gravity * height_difference));
            return range;
        }


        // Solve firing angles for a ballistic projectile with speed and gravity to hit a fixed position.
        //
        // proj_pos (Vec3): point projectile will fire from
        // proj_speed (float): scalar speed of projectile
        // target (Vec3): point projectile is trying to hit
        // gravity (float): force of gravity, positive down
        //
        // s0 (out Vec3): firing solution (low angle) 
        // s1 (out Vec3): firing solution (high angle)
        //
        // return (int): number of unique solutions found: 0, 1, or 2.
        public static int SolveBallisticArc(Vec3 proj_pos, float proj_speed, Vec3 target, float gravity, out Vec3 lowAngle, out Vec3 highAngle)
        {
            // C# requires out variables be set
            lowAngle = Vec3.Zero;
            highAngle = Vec3.Zero;


            // Derivation
            //   (1) x = v*t*cos O
            //   (2) y = v*t*sin O - .5*g*t^2
            // 
            //   (3) t = x/(cos O*v)                                        [solve t from (1)]
            //   (4) y = v*x*sin O/(cos O * v) - .5*g*x^2/(cos^2 O*v^2)     [plug t into y=...]
            //   (5) y = x*tan O - g*x^2/(2*v^2*cos^2 O)                    [reduce; cos/sin = tan]
            //   (6) y = x*tan O - (g*x^2/(2*v^2))*(1+tan^2 O)              [reduce; 1+tan O = 1/cos^2 O]
            //   (7) 0 = ((-g*x^2)/(2*v^2))*tan^2 O + x*tan O - (g*x^2)/(2*v^2) - y    [re-arrange]
            //   Quadratic! a*p^2 + b*p + c where p = tan O
            //
            //   (8) let gxv = -g*x*x/(2*v*v)
            //   (9) p = (-x +- sqrt(x*x - 4gxv*(gxv - y)))/2*gxv           [quadratic formula]
            //   (10) p = (v^2 +- sqrt(v^4 - g(g*x^2 + 2*y*v^2)))/gx        [multiply top/bottom by -2*v*v/x; move 4*v^4/x^2 into root]
            //   (11) O = atan(p)

            Vec3 diff = target - proj_pos;
            float groundDist = diff.AsVec2.Length;

            float speed2 = proj_speed * proj_speed;
            float speed4 = proj_speed * proj_speed * proj_speed * proj_speed;
            float y = diff.z;
            float x = groundDist;
            float gx = gravity * x;

            float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

            // No solution
            if (root < 0)
                return 0;

            root = MathF.Sqrt(root);

            float lowAng = (float)Math.Atan2(speed2 - root, gx);
            float highAng = (float)Math.Atan2(speed2 + root, gx);
            int numSolutions = lowAng != highAng ? 2 : 1;

            Vec3 groundDir = new Vec3(diff.AsVec2.Normalized(),0);
            lowAngle = groundDir * MathF.Cos(lowAng) * proj_speed + Vec3.Up * MathF.Sin(lowAng) * proj_speed;
            if (numSolutions > 1)
                highAngle = groundDir * MathF.Cos(highAng) * proj_speed + Vec3.Up * MathF.Sin(highAng) * proj_speed;

            return numSolutions;
        }
        /*
        // Solve firing angles for a ballistic projectile with speed and gravity to hit a target moving with constant, linear velocity.
        //
        // proj_pos (Vec3): point projectile will fire from
        // proj_speed (float): scalar speed of projectile
        // target (Vec3): point projectile is trying to hit
        // target_velocity (Vec3): velocity of target
        // gravity (float): force of gravity, positive down
        //
        // s0 (out Vec3): firing solution (fastest time impact) 
        // s1 (out Vec3): firing solution (next impact)
        // s2 (out Vec3): firing solution (next impact)
        // s3 (out Vec3): firing solution (next impact)
        //
        // return (int): number of unique solutions found: 0, 1, 2, 3, or 4.
        public static int solve_ballistic_arc(Vec3 proj_pos, float proj_speed, Vec3 target_pos, Vec3 target_velocity, float gravity, out Vec3 s0, out Vec3 s1)
        {

            // Initialize output parameters
            s0 = Vec3.zero;
            s1 = Vec3.zero;

            // Derivation 
            //
            //  For full derivation see: blog.forrestthewoods.com
            //  Here is an abbreviated version.
            //
            //  Four equations, four unknowns (solution.x, solution.y, solution.z, time):
            //
            //  (1) proj_pos.x + solution.x*time = target_pos.x + target_vel.x*time
            //  (2) proj_pos.y + solution.y*time + .5*G*t = target_pos.y + target_vel.y*time
            //  (3) proj_pos.z + solution.z*time = target_pos.z + target_vel.z*time
            //  (4) proj_speed^2 = solution.x^2 + solution.y^2 + solution.z^2
            //
            //  (5) Solve for solution.x and solution.z in equations (1) and (3)
            //  (6) Square solution.x and solution.z from (5)
            //  (7) Solve solution.y^2 by plugging (6) into (4)
            //  (8) Solve solution.y by rearranging (2)
            //  (9) Square (8)
            //  (10) Set (8) = (7). All solution.xyz terms should be gone. Only time remains.
            //  (11) Rearrange 10. It will be of the form a*^4 + b*t^3 + c*t^2 + d*t * e. This is a quartic.
            //  (12) Solve the quartic using SolveQuartic.
            //  (13) If there are no positive, real roots there is no solution.
            //  (14) Each positive, real root is one valid solution
            //  (15) Plug each time value into (1) (2) and (3) to calculate solution.xyz
            //  (16) The end.

            double G = gravity;

            double A = proj_pos.x;
            double B = proj_pos.y;
            double C = proj_pos.z;
            double M = target_pos.x;
            double N = target_pos.y;
            double O = target_pos.z;
            double P = target_velocity.x;
            double Q = target_velocity.y;
            double R = target_velocity.z;
            double S = proj_speed;

            double H = M - A;
            double J = O - C;
            double K = N - B;
            double L = -.5f * G;

            // Quartic Coeffecients
            double c0 = L * L;
            double c1 = -2 * Q * L;
            double c2 = Q * Q - 2 * K * L - S * S + P * P + R * R;
            double c3 = 2 * K * Q + 2 * H * P + 2 * J * R;
            double c4 = K * K + H * H + J * J;

            // Solve quartic
            double[] times = new double[4];
            int numTimes = SolveQuartic(c0, c1, c2, c3, c4, out times[0], out times[1], out times[2], out times[3]);

            // Sort so faster collision is found first
            System.Array.Sort(times);

            // Plug quartic solutions into base equations
            // There should never be more than 2 positive, real roots.
            Vec3[] solutions = new Vec3[2];
            int numSolutions = 0;

            for (int i = 0; i < times.Length && numSolutions < 2; ++i)
            {
                double t = times[i];
                if (t <= 0 || double.IsNaN(t))
                    continue;

                solutions[numSolutions].x = (float)((H + P * t) / t);
                solutions[numSolutions].y = (float)((K + Q * t - L * t * t) / t);
                solutions[numSolutions].z = (float)((J + R * t) / t);
                ++numSolutions;
            }

            // Write out solutions
            if (numSolutions > 0) s0 = solutions[0];
            if (numSolutions > 1) s1 = solutions[1];

            return numSolutions;
        }



        // Solve the firing arc with a fixed lateral speed. Vertical speed and gravity varies. 
        // This enables a visually pleasing arc.
        //
        // proj_pos (Vec3): point projectile will fire from
        // lateral_speed (float): scalar speed of projectile along XZ plane
        // target_pos (Vec3): point projectile is trying to hit
        // max_height (float): height above Max(proj_pos, impact_pos) for projectile to peak at
        //
        // fire_velocity (out Vec3): firing velocity
        // gravity (out float): gravity necessary to projectile to hit precisely max_height
        //
        // return (bool): true if a valid solution was found
        public static bool solve_ballistic_arc_lateral(Vec3 proj_pos, float lateral_speed, Vec3 target_pos, float max_height, out Vec3 fire_velocity, out float gravity)
        {

            // Handling these cases is up to your project's coding standards
            Debug.Assert(proj_pos != target_pos && lateral_speed > 0 && max_height > proj_pos.y, "fts.solve_ballistic_arc called with invalid data");

            fire_velocity = Vec3.zero;
            gravity = float.NaN;

            Vec3 diff = target_pos - proj_pos;
            Vec3 diffXZ = new Vec3(diff.x, 0f, diff.z);
            float lateralDist = diffXZ.magnitude;

            if (lateralDist == 0)
                return false;

            float time = lateralDist / lateral_speed;

            fire_velocity = diffXZ.normalized * lateral_speed;

            // System of equations. Hit max_height at t=.5*time. Hit target at t=time.
            //
            // peak = y0 + vertical_speed*halfTime + .5*gravity*halfTime^2
            // end = y0 + vertical_speed*time + .5*gravity*time^s
            // Wolfram Alpha: solve b = a + .5*v*t + .5*g*(.5*t)^2, c = a + vt + .5*g*t^2 for g, v
            float a = proj_pos.y;       // initial
            float b = max_height;       // peak
            float c = target_pos.y;     // final

            gravity = -4 * (a - 2 * b + c) / (time * time);
            fire_velocity.y = -(3 * a - 4 * b + c) / time;

            return true;
        }

        // Solve the firing arc with a fixed lateral speed. Vertical speed and gravity varies. 
        // This enables a visually pleasing arc.
        //
        // proj_pos (Vec3): point projectile will fire from
        // lateral_speed (float): scalar speed of projectile along XZ plane
        // target_pos (Vec3): point projectile is trying to hit
        // max_height (float): height above Max(proj_pos, impact_pos) for projectile to peak at
        //
        // fire_velocity (out Vec3): firing velocity
        // gravity (out float): gravity necessary to projectile to hit precisely max_height
        // impact_point (out Vec3): point where moving target will be hit
        //
        // return (bool): true if a valid solution was found
        public static bool solve_ballistic_arc_lateral(Vec3 proj_pos, float lateral_speed, Vec3 target, Vec3 target_velocity, float max_height_offset, out Vec3 fire_velocity, out float gravity, out Vec3 impact_point)
        {

            // Handling these cases is up to your project's coding standards
            Debug.Assert(proj_pos != target && lateral_speed > 0, "fts.solve_ballistic_arc_lateral called with invalid data");

            // Initialize output variables
            fire_velocity = Vec3.zero;
            gravity = 0f;
            impact_point = Vec3.zero;

            // Ground plane terms
            Vec3 targetVelXZ = new Vec3(target_velocity.x, 0f, target_velocity.z);
            Vec3 diffXZ = target - proj_pos;
            diffXZ.y = 0;

            // Derivation
            //   (1) Base formula: |P + V*t| = S*t
            //   (2) Substitute variables: |diffXZ + targetVelXZ*t| = S*t
            //   (3) Square both sides: Dot(diffXZ,diffXZ) + 2*Dot(diffXZ, targetVelXZ)*t + Dot(targetVelXZ, targetVelXZ)*t^2 = S^2 * t^2
            //   (4) Quadratic: (Dot(targetVelXZ,targetVelXZ) - S^2)t^2 + (2*Dot(diffXZ, targetVelXZ))*t + Dot(diffXZ, diffXZ) = 0
            float c0 = Vec3.Dot(targetVelXZ, targetVelXZ) - lateral_speed * lateral_speed;
            float c1 = 2f * Vec3.Dot(diffXZ, targetVelXZ);
            float c2 = Vec3.Dot(diffXZ, diffXZ);
            double t0, t1;
            int n = fts.SolveQuadric(c0, c1, c2, out t0, out t1);

            // pick smallest, positive time
            bool valid0 = n > 0 && t0 > 0;
            bool valid1 = n > 1 && t1 > 0;

            float t;
            if (!valid0 && !valid1)
                return false;
            else if (valid0 && valid1)
                t = MathF.Min((float)t0, (float)t1);
            else
                t = valid0 ? (float)t0 : (float)t1;

            // Calculate impact point
            impact_point = target + (target_velocity * t);

            // Calculate fire velocity along XZ plane
            Vec3 dir = impact_point - proj_pos;
            fire_velocity = new Vec3(dir.x, 0f, dir.z).normalized * lateral_speed;

            // Solve system of equations. Hit max_height at t=.5*time. Hit target at t=time.
            //
            // peak = y0 + vertical_speed*halfTime + .5*gravity*halfTime^2
            // end = y0 + vertical_speed*time + .5*gravity*time^s
            // Wolfram Alpha: solve b = a + .5*v*t + .5*g*(.5*t)^2, c = a + vt + .5*g*t^2 for g, v
            float a = proj_pos.y;       // initial
            float b = MathF.Max(proj_pos.y, impact_point.y) + max_height_offset;  // peak
            float c = impact_point.y;   // final

            gravity = -4 * (a - 2 * b + c) / (t * t);
            fire_velocity.y = -(3 * a - 4 * b + c) / t;

            return true;
        }
        */
    }
}

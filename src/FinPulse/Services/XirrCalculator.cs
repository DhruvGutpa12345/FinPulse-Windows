using FinPulse.Models;

namespace FinPulse.Services;

/// <summary>
/// High-precision, industry-standard XIRR engine solving:
/// Sum( C_i / (1 + r)^((d_i - d_0) / 365) ) = 0
/// 
/// Employs Newton-Raphson with analytical first-order derivatives, paired with an
/// automatic Brent's Method / Bisection fallback for complex, oscillating, or high-yield curves.
/// </summary>
public class XirrCalculator : IXirrCalculator
{
    private const double Tolerance = 1e-8;
    private const int MaxNewtonIterations = 60;
    private const int MaxBrentIterations = 150;
    private const double MinRateBound = -0.999999; // -99.9999%
    private const double MaxRateBound = 100.0;    // +10,000%

    private record CleanedPoint(double Amount, double YearsFromBase);

    public XirrResult Calculate(IEnumerable<CashFlowItem> items, double? guess = null)
    {
        if (items == null)
        {
            return XirrResult.Failure("Cash flow list cannot be null.");
        }

        // 1. Filter out zero amounts and sort strictly chronologically by date
        var sorted = items
            .Where(x => x != null && x.Amount != 0m)
            .OrderBy(x => x.Date)
            .ToList();

        // 2. Edge Case: Less than two distinct cash flows
        if (sorted.Count < 2)
        {
            return XirrResult.Failure("At least two distinct non-zero transactions are required to calculate XIRR.");
        }

        // 3. Edge Case: Must contain at least one outflow (< 0) and at least one inflow (> 0)
        bool hasNegative = sorted.Any(x => x.Amount < 0m);
        bool hasPositive = sorted.Any(x => x.Amount > 0m);

        if (!hasNegative || !hasPositive)
        {
            return XirrResult.Failure("Cash flows must include at least one negative outflow (investment) and one positive inflow (payout/maturity).");
        }

        // 4. Merge cash flows sharing the exact same calendar date to avoid collinearity/numerical noise
        var baseDate = sorted.First().Date.Date;
        var aggregated = sorted
            .GroupBy(x => x.Date.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalAmount = (double)g.Sum(x => x.Amount),
                Years = (g.Key - baseDate).TotalDays / 365.0
            })
            .Where(x => Math.Abs(x.TotalAmount) > 1e-9)
            .OrderBy(x => x.Years)
            .Select(x => new CleanedPoint(x.TotalAmount, x.Years))
            .ToList();

        if (aggregated.Count < 2)
        {
            return XirrResult.Failure("Transactions must span across distinct dates or have opposing net cash flows.");
        }

        // 5. Special check: If total sum of cash flows is zero, rate is exactly 0.0
        double totalSum = aggregated.Sum(x => x.Amount);
        if (Math.Abs(totalSum) < 1e-9)
        {
            double npvAtZero = EvaluateNpv(aggregated, 0.0);
            if (Math.Abs(npvAtZero) < Tolerance)
            {
                return XirrResult.Success(0.0, "Exact Zero Sum", 1, npvAtZero);
            }
        }

        // 6. Determine starting guess
        double r0;
        if (guess.HasValue && !double.IsNaN(guess.Value) && guess.Value > MinRateBound)
        {
            r0 = guess.Value;
        }
        else
        {
            r0 = CalculateSmartInitialGuess(aggregated);
        }

        // 7. Phase 1: Newton-Raphson Method
        var (newtonSuccess, newtonRate, newtonIterations, newtonResidual) = TryNewtonRaphson(aggregated, r0);
        if (newtonSuccess)
        {
            return XirrResult.Success(newtonRate, "Newton-Raphson", newtonIterations, newtonResidual);
        }

        // 8. Phase 2: Brent's Method Fallback
        var (brentSuccess, brentRate, brentIterations, brentResidual) = TryBrentMethod(aggregated);
        if (brentSuccess)
        {
            return XirrResult.Success(brentRate, "Brent-Dekker Fallback", brentIterations, brentResidual);
        }

        // 9. Phase 3: Robust Bisection Fallback
        var (bisectionSuccess, bisectionRate, bisectionIterations, bisectionResidual) = TryBisectionMethod(aggregated);
        if (bisectionSuccess)
        {
            return XirrResult.Success(bisectionRate, "Bisection Fallback", bisectionIterations, bisectionResidual);
        }

        return XirrResult.Failure("XIRR computation failed to converge within reasonable bounds [-99.9% to +10,000%]. Check for erratic or non-solvable cash flows.");
    }

    public CalculationSummary CalculateSummary(IEnumerable<CashFlowItem> items)
    {
        if (items == null) return CalculationSummary.Empty;

        var list = items.Where(x => x != null && x.Amount != 0m).ToList();
        if (list.Count == 0) return CalculationSummary.Empty;

        decimal totalInvested = Math.Abs(list.Where(x => x.Amount < 0m).Sum(x => x.Amount));
        decimal totalRealized = list.Where(x => x.Amount > 0m).Sum(x => x.Amount);
        decimal netProfit = totalRealized - totalInvested;

        double absoluteReturn = totalInvested > 0m
            ? (double)((netProfit / totalInvested) * 100m)
            : 0.0;

        var xirrRes = Calculate(list);

        return new CalculationSummary
        {
            TotalInvested = totalInvested,
            TotalRealized = totalRealized,
            NetProfit = netProfit,
            AbsoluteReturnPercentage = absoluteReturn,
            XirrPercentage = xirrRes.IsSuccess ? xirrRes.Percentage : null,
            IsConverged = xirrRes.IsSuccess,
            ConvergenceMethod = xirrRes.MethodUsed,
            ErrorMessage = xirrRes.ErrorMessage
        };
    }

    #region Mathematical Evaluation: NPV & Derivative

    /// <summary>
    /// NPV(r) = Sum( C_i / (1 + r)^t_i )
    /// </summary>
    private static double EvaluateNpv(IReadOnlyList<CleanedPoint> points, double r)
    {
        if (r <= -1.0)
        {
            return double.PositiveInfinity;
        }

        double sum = 0.0;
        double baseFactor = 1.0 + r;

        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            if (p.YearsFromBase == 0.0)
            {
                sum += p.Amount;
            }
            else
            {
                sum += p.Amount / Math.Pow(baseFactor, p.YearsFromBase);
            }
        }

        return sum;
    }

    /// <summary>
    /// NPV'(r) = - 1/(1+r) * Sum( t_i * C_i / (1 + r)^t_i )
    /// </summary>
    private static double EvaluateNpvDerivative(IReadOnlyList<CleanedPoint> points, double r)
    {
        if (r <= -1.0)
        {
            return double.NaN;
        }

        double sum = 0.0;
        double baseFactor = 1.0 + r;

        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            if (p.YearsFromBase > 0.0)
            {
                sum += p.YearsFromBase * p.Amount / Math.Pow(baseFactor, p.YearsFromBase);
            }
        }

        return -sum / baseFactor;
    }

    #endregion

    #region Numerical Solvers

    private static (bool Success, double Rate, int Iterations, double Residual) TryNewtonRaphson(
        IReadOnlyList<CleanedPoint> points,
        double r0)
    {
        double r = r0;

        for (int iter = 0; iter < MaxNewtonIterations; iter++)
        {
            double npv = EvaluateNpv(points, r);
            if (Math.Abs(npv) < Tolerance)
            {
                return (true, r, iter + 1, npv);
            }

            double dNpv = EvaluateNpvDerivative(points, r);

            // Derivative near zero: cannot divide, break to fallback
            if (double.IsNaN(dNpv) || double.IsInfinity(dNpv) || Math.Abs(dNpv) < 1e-12)
            {
                break;
            }

            double step = npv / dNpv;
            double nextR = r - step;

            // Prevent shooting past -100% or astronomical bounds
            if (nextR <= MinRateBound || nextR > MaxRateBound || double.IsNaN(nextR) || double.IsInfinity(nextR))
            {
                // Damped step trial
                nextR = r - (step * 0.5);
                if (nextR <= MinRateBound || nextR > MaxRateBound)
                {
                    break;
                }
            }

            if (Math.Abs(nextR - r) < Tolerance)
            {
                double finalNpv = EvaluateNpv(points, nextR);
                if (Math.Abs(finalNpv) < 1e-5)
                {
                    return (true, nextR, iter + 1, finalNpv);
                }
            }

            r = nextR;
        }

        return (false, r, MaxNewtonIterations, EvaluateNpv(points, r));
    }

    /// <summary>
    /// Brent-Dekker algorithm guaranteeing convergence for continuous functions within an active bracket [a, b].
    /// Combines linear interpolation, inverse quadratic interpolation, and bisection.
    /// </summary>
    private static (bool Success, double Rate, int Iterations, double Residual) TryBrentMethod(
        IReadOnlyList<CleanedPoint> points)
    {
        var bracket = FindInitialBracket(points);
        if (!bracket.HasValue)
        {
            return (false, double.NaN, 0, double.NaN);
        }

        double a = bracket.Value.Lower;
        double b = bracket.Value.Upper;
        double fa = EvaluateNpv(points, a);
        double fb = EvaluateNpv(points, b);

        if (Math.Abs(fa) < Tolerance) return (true, a, 1, fa);
        if (Math.Abs(fb) < Tolerance) return (true, b, 1, fb);

        if (fa * fb > 0)
        {
            return (false, double.NaN, 0, double.NaN);
        }

        if (Math.Abs(fa) < Math.Abs(fb))
        {
            // Swap so |f(b)| <= |f(a)|
            (a, b) = (b, a);
            (fa, fb) = (fb, fa);
        }

        double c = a;
        double fc = fa;
        bool mflag = true;
        double s = b;
        double fs = fb;
        double d = 0;

        for (int iter = 0; iter < MaxBrentIterations; iter++)
        {
            if (Math.Abs(fb) < Tolerance)
            {
                return (true, b, iter + 1, fb);
            }

            if (Math.Abs(b - a) < Tolerance)
            {
                return (true, b, iter + 1, fb);
            }

            if (!PrecisionEquals(fa, fc) && !PrecisionEquals(fb, fc))
            {
                // Inverse quadratic interpolation
                s = (a * fb * fc) / ((fa - fb) * (fa - fc))
                  + (b * fa * fc) / ((fb - fa) * (fb - fc))
                  + (c * fa * fb) / ((fc - fa) * (fc - fb));
            }
            else
            {
                // Secant method
                s = b - fb * (b - a) / (fb - fa);
            }

            // Conditions under which to use bisection instead
            double delta = Math.Abs(2 * 1e-14 * b);
            bool cond1 = (s < (3 * a + b) / 4 && s < b) || (s > (3 * a + b) / 4 && s > b);
            bool cond2 = mflag && (Math.Abs(s - b) >= Math.Abs(b - c) / 2);
            bool cond3 = !mflag && (Math.Abs(s - b) >= Math.Abs(c - d) / 2);
            bool cond4 = mflag && (Math.Abs(b - c) < delta);
            bool cond5 = !mflag && (Math.Abs(c - d) < delta);

            if (cond1 || cond2 || cond3 || cond4 || cond5)
            {
                s = (a + b) / 2;
                mflag = true;
            }
            else
            {
                mflag = false;
            }

            fs = EvaluateNpv(points, s);
            d = c;
            c = b;
            fc = fb;

            if (fa * fs < 0)
            {
                b = s;
                fb = fs;
            }
            else
            {
                a = s;
                fa = fs;
            }

            if (Math.Abs(fa) < Math.Abs(fb))
            {
                (a, b) = (b, a);
                (fa, fb) = (fb, fa);
            }
        }

        return (false, b, MaxBrentIterations, fb);
    }

    private static (bool Success, double Rate, int Iterations, double Residual) TryBisectionMethod(
        IReadOnlyList<CleanedPoint> points)
    {
        var bracket = FindInitialBracket(points);
        if (!bracket.HasValue)
        {
            return (false, double.NaN, 0, double.NaN);
        }

        double low = bracket.Value.Lower;
        double high = bracket.Value.Upper;
        double fLow = EvaluateNpv(points, low);
        double fHigh = EvaluateNpv(points, high);

        if (fLow * fHigh > 0)
        {
            return (false, double.NaN, 0, double.NaN);
        }

        double mid = low;
        double fMid = fLow;

        for (int iter = 0; iter < 100; iter++)
        {
            mid = (low + high) / 2.0;
            fMid = EvaluateNpv(points, mid);

            if (Math.Abs(fMid) < Tolerance || Math.Abs(high - low) < Tolerance)
            {
                return (true, mid, iter + 1, fMid);
            }

            if (fLow * fMid < 0)
            {
                high = mid;
                fHigh = fMid;
            }
            else
            {
                low = mid;
                fLow = fMid;
            }
        }

        return (true, mid, 100, fMid);
    }

    #endregion

    #region Bracket & Guess Utilities

    private static (double Lower, double Upper)? FindInitialBracket(IReadOnlyList<CleanedPoint> points)
    {
        // Test common financial intervals first
        double[] searchPoints = { -0.99, -0.90, -0.50, -0.20, -0.05, 0.0, 0.05, 0.10, 0.20, 0.50, 1.0, 2.0, 5.0, 10.0, 25.0, 50.0, 100.0 };

        for (int i = 0; i < searchPoints.Length - 1; i++)
        {
            double r1 = searchPoints[i];
            double r2 = searchPoints[i + 1];

            double f1 = EvaluateNpv(points, r1);
            double f2 = EvaluateNpv(points, r2);

            if (f1 * f2 <= 0.0)
            {
                return (r1, r2);
            }
        }

        // Expanded dense scan if standard points didn't catch sign change
        double step = 0.05;
        double cur = -0.95;
        double prevF = EvaluateNpv(points, cur);

        while (cur < 20.0)
        {
            double next = cur + step;
            double nextF = EvaluateNpv(points, next);

            if (prevF * nextF <= 0.0)
            {
                return (cur, next);
            }

            cur = next;
            prevF = nextF;
        }

        return null;
    }

    private static double CalculateSmartInitialGuess(IReadOnlyList<CleanedPoint> points)
    {
        double totalOutflows = Math.Abs(points.Where(p => p.Amount < 0).Sum(p => p.Amount));
        double totalInflows = points.Where(p => p.Amount > 0).Sum(p => p.Amount);
        double maxYears = points.Max(p => p.YearsFromBase);

        if (totalOutflows > 0 && maxYears > 0.05)
        {
            double ratio = totalInflows / totalOutflows;
            if (ratio > 0)
            {
                // Compound annual growth rate approx
                double approxCagr = Math.Pow(ratio, 1.0 / maxYears) - 1.0;
                if (!double.IsNaN(approxCagr) && !double.IsInfinity(approxCagr))
                {
                    return Math.Clamp(approxCagr, -0.50, 0.75);
                }
            }
        }

        return 0.10; // Default standard 10%
    }

    private static bool PrecisionEquals(double a, double b) => Math.Abs(a - b) < 1e-12;

    #endregion
}

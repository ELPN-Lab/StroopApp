"""Independent reference implementation of the StroopApp QUEST+ calibration.

Written from the formulas of the calibration specification, without reading the C# code.
Prints the values hard-coded in CalibrationReferenceTests.cs.
Run: python quest_reference.py
"""

import math

GRID_STEP = 0.005
TIE_TOLERANCE = 1e-12


def default_parameters():
    return {
        "lower": 140, "upper": 5000, "target": 0.85, "gamma": 0.25,
        "delta": 0.01, "beta": 2.0, "prior_width": 2.0,
    }


def threshold_factor(p, accuracy):
    fraction = (accuracy - p["gamma"]) / (1 - p["gamma"] - p["delta"])
    return (-math.log(1 - fraction)) ** (1 / p["beta"])


def probability(p, time, threshold):
    alpha = threshold / threshold_factor(p, p["target"])
    return p["gamma"] + (1 - p["gamma"] - p["delta"]) * (1 - math.exp(-((time / alpha) ** p["beta"])))


def grid(p):
    low, high = math.log10(p["lower"]), math.log10(p["upper"])
    n = math.ceil((high - low) / GRID_STEP)
    points = [10 ** (low + i * (high - low) / n) for i in range(n + 1)]
    points[0], points[-1] = float(p["lower"]), float(p["upper"])
    return points


def normalize(log_weights):
    top = max(log_weights)
    log_sum = top + math.log(sum(math.exp(w - top) for w in log_weights))
    return [w - log_sum for w in log_weights]


def prior(p, points, start):
    sigma = math.log10(p["prior_width"])
    return normalize([-0.5 * ((math.log10(t) - math.log10(start)) / sigma) ** 2 for t in points])


def update(p, log_weights, points, time, correct):
    updated = []
    for w, t in zip(log_weights, points):
        prob = probability(p, time, t)
        updated.append(w + math.log(prob if correct else 1 - prob))
    return normalize(updated)


def entropy(values, total):
    return -sum((v / total) * math.log(v / total) for v in values if v > 0)


def select(p, log_weights, points):
    weights = [math.exp(w) for w in log_weights]
    candidates = sorted(set(round(t) for t in points))
    best_time, best_entropy = 0, float("inf")
    for s in candidates:
        probs = [probability(p, s, t) for t in points]
        good = [w * q for w, q in zip(weights, probs)]
        bad = [w * (1 - q) for w, q in zip(weights, probs)]
        pc, pe = sum(good), sum(bad)
        expected = pc * entropy(good, pc) + pe * entropy(bad, pe)
        if expected < best_entropy - TIE_TOLERANCE:
            best_time, best_entropy = s, expected
    return best_time


def quantile(log_weights, points, q):
    cumulative, previous_middle, previous_log = 0.0, 0.0, 0.0
    for i, (w, t) in enumerate(zip(log_weights, points)):
        weight = math.exp(w)
        middle = cumulative + weight / 2
        cumulative += weight
        log_time = math.log10(t)
        if middle >= q:
            if i == 0:
                return points[0]
            fraction = (q - previous_middle) / (middle - previous_middle)
            return 10 ** (previous_log + fraction * (log_time - previous_log))
        previous_middle, previous_log = middle, log_time
    return points[-1]


def run(name, p, start, answers):
    points = grid(p)
    log_weights = prior(p, points, start)
    times = []
    for correct in answers:
        time = select(p, log_weights, points)
        times.append(time)
        log_weights = update(p, log_weights, points, time, correct)
    t5, t50, t95 = (quantile(log_weights, points, q) for q in (0.05, 0.5, 0.95))
    print(name)
    print("  times:", ", ".join(str(t) for t in times))
    print(f"  t5={t5!r} t50={t50!r} t95={t95!r} r={(t95 - t5) / t50!r}")


if __name__ == "__main__":
    C, E = True, False
    run("A", default_parameters(), 800, [C, C, E, C, C, C, E, C, C, C])
    run("B", default_parameters(), 800, [E] * 8)
    custom = default_parameters() | {"lower": 200, "upper": 3000, "target": 0.80, "beta": 1.85, "prior_width": 3.0}
    run("C", custom, 1200, [C, E] * 6)

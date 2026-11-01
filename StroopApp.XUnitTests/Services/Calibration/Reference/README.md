# QUEST+ reference implementation

This folder contains `quest_reference.py`, a second implementation of the calibration math used by `CalibrationCalculator`. The values it prints are hard-coded in `CalibrationReferenceTests.cs`, which checks that the C# code produces the same results.

## Why a second implementation

The simple formulas of the calibration (Weibull function, starting point, R, stop rule) are tested against values computed by hand. A full QUEST+ sequence cannot be computed by hand: each trial evaluates several hundred thousand probabilities. If the expected values came from the C# code itself, an error in a formula would appear on both sides and the tests would still pass.

The Python script was written from the formulas of the calibration specification, without reading the C# code. When two independent implementations agree, the C# code is very likely to follow the formulas.

## Why Python

- Python is a different language from the application, which prevents copying code, and therefore errors, from the C# implementation.
- Python is the language most researchers read, so the script can be reviewed without knowing C#.
- The script uses only the standard library and has about 120 lines.

## What is compared

The script and the C# tests run three fixed answer sequences:

| Sequence | Parameters | Starting point | Answers |
|---|---|---|---|
| A | Defaults | 800 ms | Mostly correct |
| B | Defaults | 800 ms | Errors only |
| C | 200 to 3000 ms, 80 % target, beta 1.85, prior width 3 | 1200 ms | Alternating |

For each sequence, the tests compare:

- the presentation time chosen by QUEST+ at every trial, to the millisecond;
- the final t5, t50, t95 and R, within 1e-9.

## Regenerating the values

The C# tests do not run Python. Python is only needed after a change to the formulas:

```
python quest_reference.py
```

The printed values replace the constants in `CalibrationReferenceTests.cs`.

## Note from the maintainer

The statistics behind QUEST+ are outside my field of expertise. I relied on the specification agreed with the research team and on this independent check to make the implementation as reliable as I could. The script is also meant to let people who know these methods review the math without reading the application code. I will ask the research team to review it.

Mylan Beghin

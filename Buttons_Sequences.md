### Button types
- `Normal`: pressed - move, released - stop
- `SimpleToggle`: press - start, press - stop
- `Alternating`: press - move one direction, press - move another direction
- `Circular`: press - move one direction, press - move another direction, press - stop
- `PingPong`: press - move one direction, press - stop, press - move another direction, press - stop
- `Stop`: stop
- `Accelerator`: each press increases speed
- `Sequence`: press - start sequence, press while sequence working - stop

### Sequence settings
- `Max output` in button settings isn't counted, output and direction are set in sequence steps (`Value`).
- `Loop` loops sequence. If not checked stop after last step.
- `Interpolate`: smoothly change value from one step to next one.
- Min `Duration` is 300ms, max 10000ms.


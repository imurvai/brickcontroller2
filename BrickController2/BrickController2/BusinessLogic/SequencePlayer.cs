using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;

namespace BrickController2.BusinessLogic
{
    public class SequencePlayer : ISequencePlayer
    {
        private const int _playerIntervalMs = 50;

        private readonly IDeviceManager _deviceManager;

        private readonly IDictionary<(string DeviceId, int Channel), (Sequence Sequence, bool Invert, int? StartTimeMs)> _sequences = new Dictionary<(string, int), (Sequence, bool, int?)>();
        private readonly object _lock = new object();

        private Timer _playerTimer;
        private int _timeSinceStartMs;

        public SequencePlayer(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public void StartPlayer()
        {
            lock (_lock)
            {
                StopPlayer();

                lock (_sequences)
                {
                    _sequences.Clear();
                }

                _timeSinceStartMs = 0;
                _playerTimer = new Timer((_) =>
                {
                    lock (_lock)
                    {
                        ProcessSequences();
                        _timeSinceStartMs += _playerIntervalMs;
                    }
                },
                null,
                0,
                _playerIntervalMs);
            }
        }

        public void StopPlayer()
        {
            lock (_lock)
            {
                _playerTimer?.Dispose();
                _playerTimer = null;

                lock (_sequences)
                {
                    _sequences.Clear();
                }
            }
        }

        public void ToggleSequence(string deviceId, int channel, bool invert, Sequence sequence)
        {
            lock (_sequences)
            {
                if (_sequences.ContainsKey((deviceId, channel)))
                {
                    _sequences.Remove((deviceId, channel));

                    var device = _deviceManager.GetDeviceById(deviceId);
                    device?.SetOutput(channel, 0);
                }
                else
                {
                    _sequences[(deviceId, channel)] = (sequence, invert, null);
                }
            }
        }

        private void ProcessSequences()
        {
            try
            {
                lock (_sequences)
                {
                    IList<(string DeviceId, int Channel)> sequencesToRemove = new List<(string, int)>();

                    foreach (var kvp in _sequences)
                    {
                        // Start the sequence "now" if it hasn't been started yet
                        if (!kvp.Value.StartTimeMs.HasValue)
                        {
                            _sequences[kvp.Key] = (kvp.Value.Sequence, kvp.Value.Invert, _timeSinceStartMs);
                        }

                        if (!ProcessSequence(kvp.Key.DeviceId, kvp.Key.Channel, kvp.Value.Sequence, kvp.Value.Invert, kvp.Value.StartTimeMs.Value, _timeSinceStartMs))
                        {
                            sequencesToRemove.Add((kvp.Key.DeviceId, kvp.Key.Channel));
                        }
                    }

                    foreach (var key in sequencesToRemove)
                    {
                        _sequences.Remove(key);
                    }
                }
            }
            catch
            {
            }
        }

        private bool ProcessSequence(string deviceId, int channel, Sequence sequence, bool invert, int startTimeMs, int nowMs)
        {
            if (sequence.ControlPoints.Count == 0)
            {
                return false;
            }

            var sequenceDurationMs = sequence.TotalDurationMs;
            var elapsedTimeMs = nowMs - startTimeMs;

            if ((!sequence.Loop && (sequenceDurationMs <= elapsedTimeMs)) || sequenceDurationMs == 0)
            {
                // Sequence is not looping and has finished
                return false;
            }

            var sequenceTimeMs = elapsedTimeMs - ((elapsedTimeMs / sequenceDurationMs) * sequenceDurationMs);

            SequenceControlPoint controlPoint1 = sequence.ControlPoints[0];
            SequenceControlPoint controlPoint2 = controlPoint1;
            var controlPoint1StartTimeMs = 0;
            var controlPoint1DurationMs = controlPoint1.DurationMs;

            for (int i = 1; i <= sequence.ControlPoints.Count; i++)
            {
                controlPoint2 = i < sequence.ControlPoints.Count ?
                    sequence.ControlPoints[i] :
                    sequence.Loop ?
                        sequence.ControlPoints[0] :
                        controlPoint1;

                var controlPoint2StartTimeMs = controlPoint1StartTimeMs + controlPoint1DurationMs;

                if (controlPoint1StartTimeMs <= sequenceTimeMs && sequenceTimeMs < controlPoint2StartTimeMs)
                {
                    // Found the 2 control points where the player is at the moment
                    break;
                }
                else
                {
                    controlPoint1 = controlPoint2;
                    controlPoint1StartTimeMs = controlPoint2StartTimeMs;
                    controlPoint1DurationMs = controlPoint2.DurationMs;
                    controlPoint2 = null;
                }
            }

            var value = controlPoint1.Value;

            if (sequence.Interpolate)
            {
                var value1 = controlPoint1.Value;
                var value2 = controlPoint2.Value;

                var relativeSequenceTime = (float)(sequenceTimeMs - controlPoint1StartTimeMs);

                if (relativeSequenceTime != 0 && controlPoint1DurationMs != 0)
                {
                    value = value1 + ((relativeSequenceTime / controlPoint1DurationMs) * (value2 - value1));
                }
            }

            var device = _deviceManager.GetDeviceById(deviceId);
            value = invert ? -1 * value : value;
            device?.SetOutput(channel, value);

            return true;
        }
    }
}

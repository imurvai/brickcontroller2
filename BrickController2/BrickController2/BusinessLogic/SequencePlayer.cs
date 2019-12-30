using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.BusinessLogic
{
    public class SequencePlayer : ISequencePlayer
    {
        private static readonly TimeSpan _playerInterval = TimeSpan.FromMilliseconds(20);

        private readonly IDictionary<(string DeviceId, int Channel), (Sequence Sequence, DateTime? StartTime)> _sequences = new Dictionary<(string, int), (Sequence, DateTime?)>();
        private readonly AsyncLock _lock = new AsyncLock();

        private readonly IDeviceManager _deviceManager;

        private Task _playerTask;
        private CancellationTokenSource _playerTaskTokenSource;

        public SequencePlayer(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public async Task StartPlayerAsync()
        {
            using (await _lock.LockAsync())
            {
                await StopPlayerInternalAsync();

                lock (_sequences)
                {
                    _sequences.Clear();
                }

                StartPlayerInternal();
            }
        }

        public async Task StopPlayerAsync()
        {
            using (await _lock.LockAsync())
            {
                await StopPlayerInternalAsync();

                lock (_sequences)
                {
                    _sequences.Clear();
                }
            }
        }

        public void ToggleSequence(string deviceId, int channel, Sequence sequence)
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
                    _sequences[(deviceId, channel)] = (sequence, null);
                }
            }
        }

        private void StartPlayerInternal()
        {
            _playerTaskTokenSource = new CancellationTokenSource();
            var token = _playerTaskTokenSource.Token;

            _playerTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var sequenceProcessingStartTime = DateTime.Now;

                        ProcessSequences();

                        var waitInterval = sequenceProcessingStartTime + _playerInterval - DateTime.Now;
                        await Task.Delay(waitInterval, token);
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }

        private async Task StopPlayerInternalAsync()
        {
            if (_playerTaskTokenSource == null)
            {
                return;
            }

            try
            {
                _playerTaskTokenSource.Cancel();
                await _playerTask;
            }
            catch (Exception)
            {
            }
            finally
            {
                _playerTask = null;
                _playerTaskTokenSource.Dispose();
                _playerTaskTokenSource = null;
            }
        }

        private void ProcessSequences()
        {
            lock (_sequences)
            {
                var now = DateTime.Now;
                IList<(string DeviceId, int Channel)> sequencesToRemove = new List<(string, int)>();

                foreach (var kvp in _sequences)
                {
                    // Start the sequence "now" if it hasn't been started yet
                    if (!kvp.Value.StartTime.HasValue)
                    {
                        _sequences[kvp.Key] = (kvp.Value.Sequence, now);
                    }

                    if (!ProcessSequence(kvp.Key.DeviceId, kvp.Key.Channel, kvp.Value.Sequence, kvp.Value.StartTime.Value, now))
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

        private bool ProcessSequence(string deviceId, int channel, Sequence sequence, DateTime startTime, DateTime now)
        {
            if (sequence.ControlPoints.Count == 0)
            {
                return false;
            }

            var totalDurationMs = sequence.TotalDurationMs;
            var elapsedTimeMs = (now - startTime).TotalMilliseconds;

            if ((!sequence.Loop && (totalDurationMs < elapsedTimeMs)) || totalDurationMs == 0)
            {
                // Sequence is not looping and has expired
                return false;
            }

            var sequenceTimeMs = elapsedTimeMs - ((int)(elapsedTimeMs / totalDurationMs) * totalDurationMs);

            SequenceControlPoint controlPoint1 = sequence.ControlPoints[0];
            SequenceControlPoint controlPoint2 = controlPoint1;
            var controlPoint1StartTimeMs = 0D;
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
                    // Found the 2 control points where the player is currently
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

                var relativeSequenceTime = sequenceTimeMs - controlPoint1StartTimeMs;

                if (relativeSequenceTime != 0)
                {
                    value = (float)(value1 + (relativeSequenceTime / controlPoint1DurationMs) * (value2 - value1));
                }
            }

            var device = _deviceManager.GetDeviceById(deviceId);
            device?.SetOutput(channel, value);

            return true;
        }
    }
}

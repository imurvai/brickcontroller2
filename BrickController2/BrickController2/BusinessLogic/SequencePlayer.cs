using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.BusinessLogic
{
    public class SequencePlayer : ISequencePlayer
    {
        private static readonly TimeSpan _playerInterval = TimeSpan.FromMilliseconds(20);

        private readonly IDictionary<(string DeviceId, int Channel), (Sequence Sequence, DateTime StartTime)> _sequences = new Dictionary<(string, int), (Sequence, DateTime)>();
        private readonly AsyncLock _lock = new AsyncLock();

        private readonly IDeviceManager _deviceManager;

        private Task _playerTask;
        private CancellationTokenSource _playerTaskTokenSource;

        public SequencePlayer(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public async Task StopAllSequencesAsync()
        {
            using (await _lock.LockAsync())
            {
                _sequences.Clear();

                await StopPlayerAsync();
            }
        }

        public async Task ToggleSequenceAsync(string deviceId, int channel, Sequence sequence)
        {
            using (await _lock.LockAsync())
            {
                if (_sequences.ContainsKey((deviceId, channel)))
                {
                    _sequences.Remove((deviceId, channel));

                    if (!_sequences.Keys.Any())
                    {
                        await StopPlayerAsync();
                    }
                }
                else
                {
                    _sequences[(deviceId, channel)] = (sequence, DateTime.Now);

                    if (_sequences.Keys.Count == 1)
                    {
                        await StartPlayerAsync();
                    }
                }
            }
        }

        private async Task StartPlayerAsync()
        {
            await StopPlayerAsync();

            _playerTaskTokenSource = new CancellationTokenSource();
            var token = _playerTaskTokenSource.Token;

            _playerTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var sequenceProcessingStartTime = DateTime.Now;

                        await ProcessSequencesAsync();

                        var waitInterval = sequenceProcessingStartTime + _playerInterval - DateTime.Now;
                        await Task.Delay(waitInterval, token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            });
        }

        private async Task StopPlayerAsync()
        {
            if (_playerTaskTokenSource == null)
            {
                return;
            }

            _playerTaskTokenSource.Cancel();
            await _playerTask;

            _playerTask = null;
            _playerTaskTokenSource.Dispose();
            _playerTaskTokenSource = null;
        }

        private async Task ProcessSequencesAsync()
        {
            IEnumerable<(string DeviceId, int Channel, Sequence Sequence, DateTime StartTime)> sequencesToPlay;
            IList<(string DeviceId, int Channel)> sequencesToRemove = new List<(string, int)>();

            using (await _lock.LockAsync())
            {
                sequencesToPlay = _sequences.Select(kvp => (kvp.Key.DeviceId, kvp.Key.Channel, kvp.Value.Sequence, kvp.Value.StartTime)).ToArray();
            }

            var now = DateTime.Now;
            foreach (var item in sequencesToPlay)
            {
                if (!ProcessSequence(item.DeviceId, item.Channel, item.Sequence, item.StartTime, now))
                {
                    sequencesToRemove.Add((item.DeviceId, item.Channel));
                }
            }

            using (await _lock.LockAsync())
            {
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

            var device = _deviceManager.GetDeviceById(deviceId);

            var totalDurationMs = sequence.TotalDuration.TotalMilliseconds;
            var elapsedTimeMs = (now - startTime).TotalMilliseconds;

            if ((!sequence.Loop && (totalDurationMs <= elapsedTimeMs)) || totalDurationMs == 0)
            {
                return false;
            }

            var sequenceTimeMs = elapsedTimeMs - ((int)(elapsedTimeMs / totalDurationMs) * totalDurationMs);

            var controlPoint1StartTimeMs = 0D;
            var controlPoint1 = sequence.ControlPoints[0];

            if (sequence.ControlPoints.Count < 2)
            {
                device.SetOutput(channel, controlPoint1.Value);
                return true;
            }

            //var controlPoint2 = ;
            return true;
        }
    }
}

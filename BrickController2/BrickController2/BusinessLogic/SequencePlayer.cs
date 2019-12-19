using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.BusinessLogic
{
    public class SequencePlayer : ISequencePlayer
    {
        private readonly IDictionary<(Device Device, int Channel), (Sequence Sequence, DateTime StartTime)> _sequences = new Dictionary<(Device, int), (Sequence, DateTime)>();
        private readonly object _lockObject = new object();

        public SequencePlayer()
        {
        }

        public void StopAllSequences()
        {
            lock (_lockObject)
            {
                _sequences.Clear();
                StopPlayer();
            }
        }

        public void ToggleSequence(Device device, int channel, Sequence sequence)
        {
            if (_sequences.ContainsKey((device, channel)))
            {
                lock (_lockObject)
                {
                    _sequences.Remove((device, channel));
                    
                    if (!_sequences.Keys.Any())
                    {
                        StopPlayer();
                    }
                }
            }
            else
            {
                lock (_lockObject)
                {
                    _sequences[(device, channel)] = (sequence, DateTime.Now);

                    if (_sequences.Keys.Count == 1)
                    {
                        StartPlayer();
                    }
                }
            }
        }

        private void StartPlayer()
        {
        }

        private void StopPlayer()
        {

        }

        private void ProcessSequences()
        {
            IEnumerable<(Device Device, int Channel, Sequence Sequence, DateTime StartTime)> sequencesToPlay;

            lock (_lockObject)
            {
                sequencesToPlay = _sequences.Select(kvp => (kvp.Key.Device, kvp.Key.Channel, kvp.Value.Sequence, kvp.Value.StartTime)).ToArray();
            }

            var now = DateTime.Now;
            foreach (var item in sequencesToPlay)
            {
                ProcessSequence(item.Device, item.Channel, item.Sequence, item.StartTime, now);
            }
        }

        private void ProcessSequence(Device device, int channel, Sequence sequence, DateTime startTime, DateTime now)
        {

        }
    }
}

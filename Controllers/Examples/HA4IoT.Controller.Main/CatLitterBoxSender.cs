﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.ExternalServices.Twitter;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Services.System;

namespace HA4IoT.Controller.Main
{
    internal class CatLitterBoxTwitterSender
    {
        private readonly ITwitterClientService _twitterClientService;
        private const string Suffix = "\r\nTime in litter box: {0}s\r\nNr. this day: {1}\r\n@chkratky";

        private readonly Timeout _timeout;
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);
        private readonly Stopwatch _timeInLitterBox = new Stopwatch();
        private readonly ILogger _log;

        private TimeSpan _effectiveTimeInLitterBox;
        private int _count = 1;
        private DateTime? _lastTweetTimestamp;
        private string _previousMessage = string.Empty;
        
        // Twitter will not accept the same tweet twice.
        private readonly string[] _messages =
        {
                "I was just using my #litter box...",
                "#Meow... that was just in time :-)",
                "Used my #litter box...",
                "Got some #work for you.",
                "Hey! Clean up my #litter box.",
                "I just left my #litter box.",
                "#OMG! I left a big thing for you.",
                "May te #poo be with you.",
                "#WOW, I think this is my #best one.",
                "Hey, this one looks like you :-)"         
            };

        public CatLitterBoxTwitterSender(ITimerService timerService, ITwitterClientService twitterClientService, ILogService logService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (twitterClientService == null) throw new ArgumentNullException(nameof(twitterClientService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            _twitterClientService = twitterClientService;

            _log = logService.CreatePublisher(nameof(logService));

            _timeout = new Timeout(timerService, TimeSpan.FromSeconds(30));
            _timeout.Elapsed += (s, e) =>
            {
                _timeInLitterBox.Stop();
                Task.Run(() => Tweet(_timeInLitterBox.Elapsed));
            };
        }

        public CatLitterBoxTwitterSender WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.StateChanged += RestartTimer;
            return this;
        }

        private void RestartTimer(object sender, EventArgs eventArgs)
        {
            if (!_timeInLitterBox.IsRunning)
            {
                _timeInLitterBox.Restart();
            }
            
            _timeout.Restart();
        }

        private async Task Tweet(TimeSpan timeInLitterBox)
        {
            if (IsTweetingTooFrequently())
            {
                return;
            }

            if (DurationIsTooShort(timeInLitterBox))
            {
                return;
            }

            UpdateCounter();

            string message = GenerateMessage();
            _log.Verbose("Trying to tweet '" + message + "'.");

            try
            {
                
                await _twitterClientService.Tweet(message);

                _lastTweetTimestamp = DateTime.Now;
                _log.Info("Successfully tweeted: " + message);
            }
            catch (Exception exception)
            {
                _log.Warning("Failed to tweet. " + exception.Message);
            }
        }

        private string GenerateMessage()
        {
            string message;
            do
            {
                message = _messages[_random.Next(_messages.Length - 1)];
            } while (message == _previousMessage);

            _previousMessage = message;

            return message + string.Format(Suffix, (int)_effectiveTimeInLitterBox.TotalSeconds, _count);
        }

        private bool IsTweetingTooFrequently()
        {
            return _lastTweetTimestamp.HasValue && (DateTime.Now - _lastTweetTimestamp) < TimeSpan.FromMinutes(5);
        }

        private bool DurationIsTooShort(TimeSpan timeInLitterBox)
        {
            _effectiveTimeInLitterBox = timeInLitterBox - _timeout.Duration;
            return _effectiveTimeInLitterBox < TimeSpan.FromSeconds(10);
        }

        private void UpdateCounter()
        {
            if (!_lastTweetTimestamp.HasValue)
            {
                return;
            }

            if (_lastTweetTimestamp.Value.Date.Equals(DateTime.Now.Date))
            {
                _count++;
            }
            else
            {
                _count = 1;
            }
        }
    }
}

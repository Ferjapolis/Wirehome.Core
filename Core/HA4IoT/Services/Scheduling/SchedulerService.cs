﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Services.System;
using Newtonsoft.Json.Linq;
using Timeout = System.Threading.Timeout;

namespace HA4IoT.Services.Scheduling
{
    [ApiServiceClass(typeof(ISchedulerService))]
    public class SchedulerService : ServiceBase, ISchedulerService
    {
        private readonly object _syncRoot = new object();
        private readonly List<Schedule> _schedules = new List<Schedule>();
        private readonly Timer _timer;
        private readonly ITimerService _timerService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger _log;

        public SchedulerService(ITimerService timerService, IDateTimeService dateTimeService, ILogService logService)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (logService == null) throw new ArgumentNullException(nameof(logService));

            _timerService = timerService;
            _dateTimeService = dateTimeService;

            _log = logService.CreatePublisher(nameof(SchedulerService));

            _timer = new Timer(e => ExecuteSchedules(), null, -1, Timeout.Infinite);
        }

        public override void Startup()
        {
            _timer.Change(100, 0);
        }

        public IDelayedAction In(TimeSpan delay, Action action)
        {
            return new DelayedAction(delay, action, _timerService);
        }

        [ApiMethod]
        public void GetSchedules(IApiContext apiContext)
        {
            lock (_syncRoot)
            {
                apiContext.Result = JObject.FromObject(_schedules);
            }
        }

        public void RegisterSchedule(string name, TimeSpan interval, Action action)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (action == null) throw new ArgumentNullException(nameof(action));

            lock (_syncRoot)
            {
                if (_schedules.Any(s => s.Name.Equals(name)))
                {
                    throw new InvalidOperationException($"Schedule with name '{name}' is already registered.");
                }

                var schedule = new Schedule(name, interval, action) { NextExecution = _dateTimeService.Now };
                _schedules.Add(schedule);

                _log.Info($"Registerd schedule '{name}' with interval of {interval}.");
            }
        }

        private void ExecuteSchedules()
        {
            lock (_syncRoot)
            {
                var deletedSchedules = new List<Schedule>();

                var now = _dateTimeService.Now;
                foreach (var schedule in _schedules)
                {
                    if (schedule.Status == ScheduleStatus.Running || now < schedule.NextExecution)
                    {
                        continue;
                    }

                    if (schedule.IsOneTimeSchedule)
                    {
                        deletedSchedules.Add(schedule);
                    }

                    schedule.Status = ScheduleStatus.Running;
                    Task.Run(() => ExecuteSchedule(schedule));
                }

                foreach (var deletedSchedule in deletedSchedules)
                {
                    _schedules.Remove(deletedSchedule);
                }

                _timer.Change(100, 0);
            }
        }

        private void ExecuteSchedule(Schedule schedule)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _log.Verbose($"Executing schedule '{schedule.Name}'.");

                schedule.Action();
                schedule.LastErrorMessage = null;
                schedule.Status = ScheduleStatus.Idle;
            }
            catch (Exception exception)
            {
                _log.Error(exception, $"Error while executing schedule '{schedule.Name}'.");

                schedule.Status = ScheduleStatus.Faulted;
                schedule.LastErrorMessage = exception.Message;
            }
            finally
            {
                schedule.LastExecutionDuration = stopwatch.Elapsed;
                schedule.LastExecution = _dateTimeService.Now;
                schedule.NextExecution = _dateTimeService.Now + schedule.Interval;
            }
        }
    }
}

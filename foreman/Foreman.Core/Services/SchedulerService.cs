/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foreman.Core.Infrastructure.Extensions;
using Foreman.Core.Jobs;
using Foreman.Core.Models;
using Foreman.Core.ViewModels;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace Foreman.Core.Services
{
    public interface ISchedulerService
    {
        int Count(CancellationToken ct);
        Task<IEnumerable<ScheduledJob>> List(CancellationToken ct);
        Task<ScheduledJob> GetByName(Guid name, string groupName, CancellationToken ct);
        Task<IReadOnlyCollection<IJobExecutionContext>> GetCurrent(CancellationToken ct);
        Task<JobKey> Create(WorkOrder workOrder, WebHook webHook, CancellationToken ct);
        void Delete(string key, string group, CancellationToken ct);
        void Clear(CancellationToken ct);
    }

    public class SchedulerService : ISchedulerService
    {
        private IScheduler _scheduler { get; set; }

        public SchedulerService()
        {
            // Grab the _scheduler instance from the Factory
            var props = new NameValueCollection
            {
                {"quartz.serializer.type", "binary"}
            };
            var factory = new StdSchedulerFactory(props);
            this._scheduler = factory.GetScheduler().Result;
            if(!this._scheduler.IsStarted)
                this._scheduler.Start();
        }

        public void Clear(CancellationToken ct)
        {
            this._scheduler.Clear(ct);
        }

        public int Count(CancellationToken ct)
        {
            return this._scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), ct).Result.Count;
        }

        public async Task<IEnumerable<ScheduledJob>> List(CancellationToken ct)
        {
            var list = new List<ScheduledJob>();

            foreach (var groupName in await _scheduler.GetJobGroupNames(ct))
            {
                foreach (var jobKey in await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName), ct))
                {
                    var jobName = jobKey.Name;
                    //var jobGroup = jobKey.Group;

                    //get job's triggers
                    var triggers = (List<ITrigger>) await this._scheduler.GetTriggersOfJob(jobKey, ct);
                    var nextFireTime = triggers[0].GetNextFireTimeUtc();
                    var triggerKey = !string.IsNullOrEmpty(groupName) ? new TriggerKey(jobName, groupName) : new TriggerKey(jobName);
                    var triggerDetails = await this._scheduler.GetTrigger(triggerKey, ct);
                    var detail = await this._scheduler.GetJobDetail(jobKey, ct);

                    var s = new ScheduledJob();

                    s.Triggers = triggers;
                    s.Detail = detail;
                    s.JobName = jobName;
                    s.GroupName = groupName;
                    if (nextFireTime.HasValue)
                        s.NextFireTimeUtc = nextFireTime.Value.UtcDateTime;
                    s.IsCompleted = triggerDetails.GetMayFireAgain();
                    s.TriggerKey = triggerDetails.Key.Name;
                    s.JobKey = triggerDetails.JobKey.Name;

                    list.Add(s);

                    //Console.WriteLine($"[job]: {jobName} [group]: {jobGroup} [next]: {nextFireTime}");
                }
            }

            return list;
        }

        public async Task<IReadOnlyCollection<IJobExecutionContext>> GetCurrent(CancellationToken ct)
        {
            return await this._scheduler.GetCurrentlyExecutingJobs(ct);
        }

        public async Task<ScheduledJob> GetByName(Guid name, string groupName, CancellationToken ct)
        {
            foreach (var gName in await _scheduler.GetJobGroupNames(ct))
            {
                foreach (var jobKey in await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(gName), ct))
                {
                    var jobName = jobKey.Name;

                    if (jobName == name.ToString())
                    {
                        //get job's triggers
                        var triggers = (List<ITrigger>) await this._scheduler.GetTriggersOfJob(jobKey, ct);
                        var nextFireTime = triggers[0].GetNextFireTimeUtc();
                        var triggerKey = !string.IsNullOrEmpty(groupName) ? new TriggerKey(jobName, groupName) : new TriggerKey(jobName);
                        var triggerDetails = this._scheduler.GetTrigger(triggerKey, ct);
                        var detail = await this._scheduler.GetJobDetail(jobKey, ct);

                        var s = new ScheduledJob();

                        s.Triggers = triggers;
                        s.Detail = detail;
                        s.JobName = jobName;
                        s.GroupName = groupName;
                        if (nextFireTime.HasValue)
                            s.NextFireTimeUtc = nextFireTime.Value.UtcDateTime;
                        s.IsCompleted = triggerDetails.IsCompleted;
                        s.TriggerKey = triggerDetails.Result.Key.Name;
                        s.JobKey = triggerDetails.Result.JobKey.Name;

                        return s;
                    }
                }
            }

            return new ScheduledJob();
        }

        public async Task<JobKey> Create(WorkOrder workOrder, WebHook webHook, CancellationToken ct)
        {
            if (workOrder.Id == Guid.Empty)
            {
                workOrder.Id = Guid.NewGuid();
            }

            return await Schedule(workOrder, webHook, ct);
        }

        public void Delete(string key, string group, CancellationToken ct)
        {
            this._scheduler.DeleteJob(new JobKey(key, group), ct);
        }

        private async Task<JobKey> Schedule(WorkOrder workOrder, WebHook webHook, CancellationToken ct)
        {
            try
            {
                return await SetupSchedule(workOrder, webHook, ct);
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
                throw;
            }
        }

        private async Task<JobKey> SetupSchedule(WorkOrder workOrder, WebHook webHook, CancellationToken ct)
        {
            try
            {
                IJobDetail job;
                switch (workOrder.Job)
                {
                    case JobType.Test:
                        job = JobBuilder.Create<TestJob>()
                            .WithIdentity(workOrder.Id.ToString(), workOrder.GroupName)
                            .Build();
                        break;
                    case JobType.WebHook:
                        if (webHook == null)
                            throw new ArgumentException("WebHook Id not found");
                        var parameters = JsonConvert.SerializeObject(workOrder.Params);
                        job = JobBuilder.Create<WebHookJob>()
                            .WithIdentity(workOrder.Id.ToString(), workOrder.GroupName)
                            .UsingJobData("url", webHook.PostbackUrl)
                            .UsingJobData("method", webHook.PostbackMethod.ToString())
                            .UsingJobData("mustAuthenticate", webHook.MustAuthenticate)
                            .UsingJobData("payload", webHook.Payload)
                            .UsingJobData("parameters", parameters)
                            .Build();
                        break;
                    default:
                        throw new NotImplementedException("Job Type not supported");
                }
                
                foreach (var t in workOrder.Triggers)
                {
                    var trigger = GetTrigger(workOrder, t, job.Key);

                    Console.WriteLine($"Job setup with: Interval: {t.Interval}s | StartUTC: {trigger.StartTimeUtc} - EndUTC: {trigger.EndTimeUtc} LastFire: {trigger.FinalFireTimeUtc}");

                    await StartScheduledJob(job, trigger, ct);
                }

                return job.Key;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Tell quartz to schedule the job using our trigger
        /// </summary>
        private async Task StartScheduledJob(IJobDetail job, ITrigger trigger, CancellationToken ct)
        {
            await this._scheduler.ScheduleJob(job, trigger, ct);
        }

        /// <summary>
        /// Build the appropriate trigger for fire-once vs repeat jobs 
        /// </summary>
        private static ITrigger GetTrigger(WorkOrder workOrder, WorkOrder.WorkOrderTrigger t, JobKey job)
        {
            var trigger = TriggerBuilder.Create()
                .WithIdentity(workOrder.Id.ToString(), workOrder.GroupName)
                .ForJob(job);
                
            if (workOrder.Start < DateTime.UtcNow)
            {
                trigger
                    .StartNow();
            }
            else
            {
                trigger
                    .StartAt(workOrder.Start.GetQuartzDateTimeOffset());
                
                // only need EndAt for interval (non-single-fire) jobs 
                if (t.Interval > 0)
                {
                    // Trigger the job to run now, and then repeat every n
                    trigger
                        .EndAt(workOrder.End.GetQuartzDateTimeOffset())
                        .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(t.Interval)
                            .RepeatForever());
                }
            }
                
            return trigger.Build();
        }
    }
}

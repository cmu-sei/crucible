/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using Steamfitter.Api.Data.Models;

namespace Steamfitter.Api.Data
{
    public class Seed
    {
        public static void Run(SteamfitterContext context)
        {
            // sketch users
            var uAdministrator = new UserEntity { Id = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), Name = "admin user" };
            context.Users.Add(uAdministrator);

            // scenarios
            var scenario1 = new ScenarioEntity { Name = "First Scenario", Description = "My first scenario", CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow, DurationHours = 1};
            context.Scenarios.Add(scenario1);
            var scenario2 = new ScenarioEntity { Name = "Second Scenario", Description = "My second scenario", CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow, DurationHours = 2};
            context.Scenarios.Add(scenario2);
            var scenario3 = new ScenarioEntity { Name = "Third Scenario", Description = "My third scenario", CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow, DurationHours = 3};
            context.Scenarios.Add(scenario3);
            context.SaveChanges();
            // sessions
            var session1 = new SessionEntity { Name = "Session001", Description = "Session #1", ScenarioId = scenario1.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), DateCreated = DateTime.UtcNow, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), ExerciseId = Guid.Parse("453d394e-bf18-499b-9786-149b0f8d69ec"), Exercise="RCC -E EM 2018", Status = SessionStatus.ready, OnDemand = false };
            context.Sessions.Add(session1);
            var session2 = new SessionEntity { Name = "Session002", Description = "Session #2", ScenarioId = scenario1.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(2), DateCreated = DateTime.UtcNow, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), ExerciseId = Guid.Parse("453d394e-bf18-499b-9786-149b0f8d69ec"), Exercise="RCC -E EM 2018", Status = SessionStatus.ready, OnDemand = false };
            context.Sessions.Add(session2);
            context.SaveChanges();

            // //sessionVms  (or 5209fff8-8098-f1de-40c3-ac0eb1c8d515?)
            // var vm1 = new SessionVmEntity {SessionId =session1.Id, Vm = Guid.Parse("0539a863-ece6-4ca7-ae8d-f46a909526e9")};
            // context.SessionVms.Add(vm1);
            // context.SaveChanges();
            // VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9")

            // scenario #1 dispatchTasks
            var dispatchTask2 = new DispatchTaskEntity { Name = "Get host name", Description = "For scenario #1 get the host name from s3-vm", ScenarioId = scenario1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_process_run, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"CommandText\": \"CMD.EXE\", \"CommandArgs\": \"/c hostname\"}", ExpectedOutput = "DESKTOP-OM1GOK7", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask2);
            var dispatchTask2b = new DispatchTaskEntity { Name = "Run netstat -an", Description = "For sceanrion #1 run netstat -an against all of the vm's", ScenarioId = scenario1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_process_run, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"CommandText\": \"CMD.EXE\", \"CommandArgs\": \"/c netstat -an\"}", ExpectedOutput = "This will never work!", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask2b);
            // sceanrio #2 dispatchTasks
            var dispatchTaskC1 = new DispatchTaskEntity { Name = "Parent Task #1", Description = "Read the test file from s3-vm", ScenarioId = scenario2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testGet.txt\"}", ExpectedOutput = "This is the text from the TEST file.", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTaskC1);
            var dispatchTaskC1b = new DispatchTaskEntity { Name = "Success Task", Description = "Read the success file from s3-vm", ScenarioId = scenario2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testSuccess.txt\"}", ExpectedOutput = "This is the text from the SUCCESS file.", TriggerTask=dispatchTaskC1, TriggerCondition = TaskTrigger.Success, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTaskC1b);
            var dispatchTaskC3 = new DispatchTaskEntity { Name = "Failure Task", Description = "Read the failure file from s3-vm", ScenarioId = scenario2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testFailure.txt\"}", ExpectedOutput = "This is the text from the FAILURE file.", TriggerTask=dispatchTaskC1, TriggerCondition = TaskTrigger.Failure, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTaskC3);
            var dispatchTaskC4 = new DispatchTaskEntity { Name = "Completion Task", Description = "Read the completion file from s3-vm", ScenarioId = scenario2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testCompletion.txt\"}", ExpectedOutput = "This is the text from the COMPLETION file.", TriggerTask=dispatchTaskC1, TriggerCondition = TaskTrigger.Completion, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTaskC4);
            var dispatchTaskC5 = new DispatchTaskEntity { Name = "Expiration Task", Description = "Read the expiration file from s3-vm", ScenarioId = scenario2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testExpiration.txt\"}", ExpectedOutput = "This is the text from the EXPIRATION file.", TriggerTask=dispatchTaskC1, TriggerCondition = TaskTrigger.Expiration, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTaskC5);
            var dispatchTaskC6 = new DispatchTaskEntity { Name = "Delayed Task", Description = "Read the delay file from s3-vm", ScenarioId = scenario2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testDelay.txt\"}", ExpectedOutput = "This is the text from the DELAY file.", TriggerTask=dispatchTaskC4, TriggerCondition = TaskTrigger.Completion, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, DelaySeconds = 10, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTaskC6);
            // session #1 dispatchTasks
            var dispatchTask1 = new DispatchTaskEntity { Name = "Parent Task", Description = "Parent Task #1a", SessionId = session1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testGet.txt\"}", ExpectedOutput = "This is the text from the TEST file.", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask1);
            var dispatchTask1b = new DispatchTaskEntity { Name = "Success Task", Description = "Success Task #1b", SessionId = session1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testSuccess.txt\"}", ExpectedOutput = "This is the text from the SUCCESS file.", TriggerTask=dispatchTask1, TriggerCondition = TaskTrigger.Success, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask1b);
            context.DispatchTasks.Add(dispatchTask2b);
            var dispatchTask3 = new DispatchTaskEntity { Name = "Failure Task", Description = "Failure Task #3", SessionId = session1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testFailure.txt\"}", ExpectedOutput = "This is the text from the FAILURE file.", TriggerTask=dispatchTask1, TriggerCondition = TaskTrigger.Failure, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask3);
            var dispatchTask4 = new DispatchTaskEntity { Name = "Completion Task", Description = "Completion Task #4", SessionId = session1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testCompletion.txt\"}", ExpectedOutput = "This is the text from the COMPLETION file.", TriggerTask=dispatchTask1, TriggerCondition = TaskTrigger.Completion, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask4);
            var dispatchTask5 = new DispatchTaskEntity { Name = "Expiration Task", Description = "Expiration Task #5", SessionId = session1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testExpiration.txt\"}", ExpectedOutput = "This is the text from the EXPIRATION file.", TriggerTask=dispatchTask1, TriggerCondition = TaskTrigger.Expiration, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask5);
            var dispatchTask6 = new DispatchTaskEntity { Name = "Delayed Task", Description = "Delayed Task #6", SessionId = session1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testDelay.txt\"}", ExpectedOutput = "This is the text from the DELAY file.", TriggerTask=dispatchTask4, TriggerCondition = TaskTrigger.Completion, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, DelaySeconds = 10, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask6);
            // independent task
            var dispatchTask1c = new DispatchTaskEntity { Name = "Independent Task", Description = "Independent Task #1c", ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testGet.txt\"}", ExpectedOutput = "This is the text from the TEST file.", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), UserId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.DispatchTasks.Add(dispatchTask1c);
            context.SaveChanges();
            
            // // DispatchTaskResults
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask1.Id, VmId = Guid.Parse("0539a863-ece6-4ca7-ae8d-f46a909526e9"), VmName = "S3-VM2", Status = TaskStatus.queued, InputString = "copy test1.txt success.txt", ExpectedOutput = "Copied", ExpirationSeconds = 61, Iterations = 1, IntervalSeconds = 0, DateCreated = DateTime.Now.AddMinutes(-33) });
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.queued, InputString = "copy test2.txt success.txt", ExpectedOutput = "Oh boy!", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-43) });
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.succeeded, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ActualOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-53)});
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.failed, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ActualOutput = "Failure", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-55)});
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.succeeded, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ActualOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-153)});
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.sent, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-155)});
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.error, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-156)});
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.queued, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-157)});
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.expired, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-158)});
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.pending, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-159)});
            context.DispatchTaskResults.Add(new DispatchTaskResultEntity { DispatchTaskId = dispatchTask2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.cancelled, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-175)});
            context.SaveChanges();
            
            Console.WriteLine("Seed data completed");            
        }
    }
}


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

            // scenarioTemplates
            var scenarioTemplate1 = new ScenarioTemplateEntity { Name = "First ScenarioTemplate", Description = "My first scenarioTemplate", CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow, DurationHours = 1};
            context.ScenarioTemplates.Add(scenarioTemplate1);
            var scenarioTemplate2 = new ScenarioTemplateEntity { Name = "Second ScenarioTemplate", Description = "My second scenarioTemplate", CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow, DurationHours = 2};
            context.ScenarioTemplates.Add(scenarioTemplate2);
            var scenarioTemplate3 = new ScenarioTemplateEntity { Name = "Third ScenarioTemplate", Description = "My third scenarioTemplate", CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow, DurationHours = 3};
            context.ScenarioTemplates.Add(scenarioTemplate3);
            context.SaveChanges();
            // scenarios
            var scenario1 = new ScenarioEntity { Name = "Scenario001", Description = "Scenario #1", ScenarioTemplateId = scenarioTemplate1.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1), DateCreated = DateTime.UtcNow, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), ViewId = Guid.Parse("453d394e-bf18-499b-9786-149b0f8d69ec"), View="RCC -E EM 2018", Status = ScenarioStatus.ready, OnDemand = false };
            context.Scenarios.Add(scenario1);
            var scenario2 = new ScenarioEntity { Name = "Scenario002", Description = "Scenario #2", ScenarioTemplateId = scenarioTemplate1.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(2), DateCreated = DateTime.UtcNow, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), ViewId = Guid.Parse("453d394e-bf18-499b-9786-149b0f8d69ec"), View="RCC -E EM 2018", Status = ScenarioStatus.ready, OnDemand = false };
            context.Scenarios.Add(scenario2);
            context.SaveChanges();

            // //scenarioVms  (or 5209fff8-8098-f1de-40c3-ac0eb1c8d515?)
            // var vm1 = new ScenarioVmEntity {ScenarioId =scenario1.Id, Vm = Guid.Parse("0539a863-ece6-4ca7-ae8d-f46a909526e9")};
            // context.ScenarioVms.Add(vm1);
            // context.SaveChanges();
            // VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9")

            // scenarioTemplate #1 tasks
            var task2 = new TaskEntity { Name = "Get host name", Description = "For scenarioTemplate #1 get the host name from s3-vm", ScenarioTemplateId = scenarioTemplate1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_process_run, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"CommandText\": \"CMD.EXE\", \"CommandArgs\": \"/c hostname\"}", ExpectedOutput = "DESKTOP-OM1GOK7", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task2);
            var task2b = new TaskEntity { Name = "Run netstat -an", Description = "For sceanrion #1 run netstat -an against all of the vm's", ScenarioTemplateId = scenarioTemplate1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_process_run, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"CommandText\": \"CMD.EXE\", \"CommandArgs\": \"/c netstat -an\"}", ExpectedOutput = "This will never work!", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task2b);
            // sceanrio #2 tasks
            var taskC1 = new TaskEntity { Name = "Parent Task #1", Description = "Read the test file from s3-vm", ScenarioTemplateId = scenarioTemplate2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testGet.txt\"}", ExpectedOutput = "This is the text from the TEST file.", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(taskC1);
            var taskC1b = new TaskEntity { Name = "Success Task", Description = "Read the success file from s3-vm", ScenarioTemplateId = scenarioTemplate2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testSuccess.txt\"}", ExpectedOutput = "This is the text from the SUCCESS file.", TriggerTask=taskC1, TriggerCondition = TaskTrigger.Success, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(taskC1b);
            var taskC3 = new TaskEntity { Name = "Failure Task", Description = "Read the failure file from s3-vm", ScenarioTemplateId = scenarioTemplate2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testFailure.txt\"}", ExpectedOutput = "This is the text from the FAILURE file.", TriggerTask=taskC1, TriggerCondition = TaskTrigger.Failure, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(taskC3);
            var taskC4 = new TaskEntity { Name = "Completion Task", Description = "Read the completion file from s3-vm", ScenarioTemplateId = scenarioTemplate2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testCompletion.txt\"}", ExpectedOutput = "This is the text from the COMPLETION file.", TriggerTask=taskC1, TriggerCondition = TaskTrigger.Completion, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(taskC4);
            var taskC5 = new TaskEntity { Name = "Expiration Task", Description = "Read the expiration file from s3-vm", ScenarioTemplateId = scenarioTemplate2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testExpiration.txt\"}", ExpectedOutput = "This is the text from the EXPIRATION file.", TriggerTask=taskC1, TriggerCondition = TaskTrigger.Expiration, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(taskC5);
            var taskC6 = new TaskEntity { Name = "Delayed Task", Description = "Read the delay file from s3-vm", ScenarioTemplateId = scenarioTemplate2.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testDelay.txt\"}", ExpectedOutput = "This is the text from the DELAY file.", TriggerTask=taskC4, TriggerCondition = TaskTrigger.Completion, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, DelaySeconds = 10, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(taskC6);
            // scenario #1 tasks
            var task1 = new TaskEntity { Name = "Parent Task", Description = "Parent Task #1a", ScenarioId = scenario1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testGet.txt\"}", ExpectedOutput = "This is the text from the TEST file.", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task1);
            var task1b = new TaskEntity { Name = "Success Task", Description = "Success Task #1b", ScenarioId = scenario1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testSuccess.txt\"}", ExpectedOutput = "This is the text from the SUCCESS file.", TriggerTask=task1, TriggerCondition = TaskTrigger.Success, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task1b);
            context.Tasks.Add(task2b);
            var task3 = new TaskEntity { Name = "Failure Task", Description = "Failure Task #3", ScenarioId = scenario1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testFailure.txt\"}", ExpectedOutput = "This is the text from the FAILURE file.", TriggerTask=task1, TriggerCondition = TaskTrigger.Failure, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task3);
            var task4 = new TaskEntity { Name = "Completion Task", Description = "Completion Task #4", ScenarioId = scenario1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testCompletion.txt\"}", ExpectedOutput = "This is the text from the COMPLETION file.", TriggerTask=task1, TriggerCondition = TaskTrigger.Completion, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task4);
            var task5 = new TaskEntity { Name = "Expiration Task", Description = "Expiration Task #5", ScenarioId = scenario1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testExpiration.txt\"}", ExpectedOutput = "This is the text from the EXPIRATION file.", TriggerTask=task1, TriggerCondition = TaskTrigger.Expiration, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task5);
            var task6 = new TaskEntity { Name = "Delayed Task", Description = "Delayed Task #6", ScenarioId = scenario1.Id, ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testDelay.txt\"}", ExpectedOutput = "This is the text from the DELAY file.", TriggerTask=task4, TriggerCondition = TaskTrigger.Completion, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, DelaySeconds = 10, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task6);
            // independent task
            var task1c = new TaskEntity { Name = "Independent Task", Description = "Independent Task #1c", ApiUrl = "stackstorm", Action = TaskAction.guest_file_read, InputString = "{\"VmGuid\": \"42313053-c2e6-42af-cf2a-6db9f791794a\", \"Username\": \"Developer\", \"Password\": \"develop@1\", \"GuestFilePath\": \"C:\\\\Users\\\\Developer\\\\testGet.txt\"}", ExpectedOutput = "This is the text from the TEST file.", TriggerCondition = TaskTrigger.Manual, ExpirationSeconds = 0, IntervalSeconds = 0, Iterations = 0, CreatedBy = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), UserId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), DateCreated = DateTime.UtcNow };
            context.Tasks.Add(task1c);
            context.SaveChanges();
            
            // // Results
            context.Results.Add(new ResultEntity { TaskId = task1.Id, VmId = Guid.Parse("0539a863-ece6-4ca7-ae8d-f46a909526e9"), VmName = "S3-VM2", Status = TaskStatus.queued, InputString = "copy test1.txt success.txt", ExpectedOutput = "Copied", ExpirationSeconds = 61, Iterations = 1, IntervalSeconds = 0, DateCreated = DateTime.Now.AddMinutes(-33) });
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.queued, InputString = "copy test2.txt success.txt", ExpectedOutput = "Oh boy!", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-43) });
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.succeeded, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ActualOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-53)});
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.failed, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ActualOutput = "Failure", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-55)});
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.succeeded, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ActualOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-153)});
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.sent, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-155)});
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.error, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-156)});
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.queued, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-157)});
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.expired, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-158)});
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.pending, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-159)});
            context.Results.Add(new ResultEntity { TaskId = task2.Id, VmId = Guid.Parse("9fd3c38e-58b0-4af1-80d1-1895af91f1f9"), VmName = "S3-VM3", Status = TaskStatus.cancelled, InputString = "copy test.txt success.txt", ExpectedOutput = "Completed", ExpirationSeconds = 62, Iterations = 2, IntervalSeconds = 120, DateCreated = DateTime.Now.AddMinutes(-175)});
            context.SaveChanges();
            
            Console.WriteLine("Seed data completed");            
        }
    }
}


/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

namespace Steamfitter.Api.Data
{
    public enum TaskAction
    {
        guest_process_run = 100,
        guest_file_read = 101,
        guest_file_write = 102,
        vm_hw_power_off = 103,
        vm_hw_power_on = 104,
        vm_create_from_template = 105,
        vm_hw_remove = 106
    }

    public enum TaskStatus
    {
        pending = 10,
        queued = 20,
        sent = 30,
        cancelled = 40,
        expired = 50,
        failed = 60,
        succeeded = 70,
        error = 80
    }

    public enum TaskTrigger
    {
        Time = 1,
        Success =2 ,
        Failure = 3,
        Completion = 4,
        Expiration = 5,
        Manual = 6
    }

    public enum ScenarioStatus
    {
        ready = 1,
        active = 2,
        paused = 3,
        ended = 4,
        archived = 5,
        error = 6
    }

}


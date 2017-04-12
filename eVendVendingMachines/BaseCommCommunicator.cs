using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVendVendingMachines {

    public interface ICommunicateViaComms {
        CommsNextAction Start();
        CommsNextAction WorkOnResponse(List<CommCommand> sentCommands, List<CommCommand> recievedCommands);
        CommsNextAction Kick(List<CommCommand> sentCommands, List<CommCommand> recievedCommands);
    }
    public class CommsNextAction {
        public bool ShouldReply { get; set; }
        public string Reply { get; set; }
        public VendingMilestone? Milestone { get; set; }
    }
    public enum VendingMilestone {
        DoorOpened,
        DoorClosed,
    }
    public class Robo500Communicator : ICommunicateViaComms {
        public CommsNextAction Kick(List<CommCommand> sentCommands, List<CommCommand> recievedCommands) {
            return new CommsNextAction() { ShouldReply = true, Reply = "RDS" };
        }

        public CommsNextAction Start() {
            return new CommsNextAction() { ShouldReply = true, Reply = "RDS" };
        }

        public CommsNextAction WorkOnResponse(List<CommCommand> sentCommands, List<CommCommand> recievedCommands) {
            var lastSentCmd = GetLastCommand(sentCommands);
            var lastRecCmd = GetLastCommand(recievedCommands, lastSentCmd.DateTime);
            var lastRecievedCmds = GetCommandsReceivedSinceLastSent(lastSentCmd, recievedCommands);
            //TODO:: Work on history? (how many RDSs, How many CALs)

            //work on the raw data
            if (lastSentCmd.Text == "RDS") {
                if (lastRecCmd != null) {
                    if (lastRecCmd.Text.Contains("DC")) {
                        //doors are closed, start calibrating checks
                        return new CommsNextAction() { ShouldReply = true, Reply = "RCS" };
                    }
                    if (lastRecCmd.Text.Contains("DO")) {
                        //doors are open, tell user to shut them
                    }
                }
            }

            if (lastSentCmd.Text == "RCS") {
                if (lastRecCmd != null) {
                    if (lastRecCmd.Text.Contains("CC")) {
                        //calibration is good, FET
                        return new CommsNextAction() { ShouldReply = true, Reply = "FET" };
                    }
                    if (lastRecCmd.Text.Contains("CI")) {
                        //needs calibrating
                        return new CommsNextAction() { ShouldReply = true, Reply = "CAL" };
                    }
                }
            }

            if (lastSentCmd.Text == "FET") {
                if (lastRecCmd != null) {
                    if (lastRecCmd.Text.Contains("MC")) {
                        //calibration is good, FET
                        return new CommsNextAction() { ShouldReply = true, Reply = "LON" };
                    }
                }
            }

            if (lastSentCmd.Text == "LON") {
                if (lastRecCmd != null) {
                    if (lastRecCmd.Text.Contains("DO")) {
                        //Door was opened user took the stuff
                        return new CommsNextAction() { ShouldReply = false, Milestone = VendingMilestone.DoorOpened };
                    }
                }
            }
            return new CommsNextAction() { ShouldReply = false };
        }

        private List<CommCommand> GetCommandsReceivedSinceLastSent(CommCommand sentCmd, List<CommCommand> recievedCommands) {
            return recievedCommands.Where(x => x.DateTime > sentCmd.DateTime).ToList();
        }

        private CommCommand GetLastCommand(List<CommCommand> cmds) {
            return cmds.OrderByDescending(x => x.DateTime).FirstOrDefault();
        }
        private CommCommand GetLastCommand(List<CommCommand> cmds, DateTime since) {
            return cmds.Where(x => x.DateTime > since).OrderByDescending(x => x.DateTime).FirstOrDefault();
        }
    }
}

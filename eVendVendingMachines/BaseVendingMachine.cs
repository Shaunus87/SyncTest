using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eVendVendingMachines {

    public static class VendingMachineFactory {
        /// <summary>
        /// Create a vending machine based on provided information
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IVend Create(string name, string connDetails) {
            return GetVendingMachine(name, connDetails);
        }

        /// <summary>
        /// Get the corrosponding vending machine for the station
        /// TODO:: Returnable VMs
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        private static IVend GetVendingMachine(string stationName, string connDetails) {
            BaseVendingMachine mach;
            switch (stationName.ToLower()) {
                case "robo":
                    mach = new Robo500TestVendingMachine(connDetails);
                    break;
                case "test1":
                    mach = new TestSuccessVendingMachine();
                    break;
                default:
                    mach = new NoVendingMachine();
                    break;
            }
            return mach;
        }
    }
    public interface IVend {
        void VendItems(List<VendRequest> bins);
        void StockItems(List<VendRequest> bins);
        event SuccessHandler OnVendSuccess;
        event FailureHandler OnVendFailure;
        event CompleteHandler OnVendComplete;
    }

    public delegate void SuccessHandler(object sender, SuccessEventArgs e);
    public delegate void FailureHandler(object sender, FailureEventArgs e);
    public delegate void CompleteHandler(object sender, CompleteEventArgs e);

    public abstract class BaseVendingMachine : IVend {

        /// <summary>
        /// Name of the vending machine
        /// </summary>
        public string Name { get; set; }

        public event SuccessHandler OnVendSuccess;
        public event FailureHandler OnVendFailure;
        public event CompleteHandler OnVendComplete;

        public List<VendRequest> Bins { get; protected set; }

        public BaseVendingMachine(string name) {
            Name = name;
        }

        /// <summary>
        /// Same as vend but instead waits/checks for a door close event/status
        /// </summary>
        /// <param name="bins"></param>
        public virtual void StockItems(List<VendRequest> bins) {
            Bins = bins;
            OnBeginStockItems(Bins);
        }

        /// <summary>
        /// Overridable method for subclasses called when consumer requests begin stock items
        /// </summary>
        /// <param name="bins"></param>
        protected virtual void OnBeginStockItems(List<VendRequest> bins) {
            throw new NotImplementedException("Implement OnBeginStockItems in the consuming class or remove the call to base.OnBeginStockItems();");
        }

        /// <summary>
        /// Vend an item by unlocking/opening a door or pushing item off shelf
        /// </summary>
        /// <param name="bins"></param>
        public virtual void VendItems(List<VendRequest> bins) {
            Bins = bins;
            OnBeginVendItems(Bins);
        }

        /// <summary>
        /// This is called when base recieves vend items call
        /// </summary>
        /// <param name="bins"></param>
        protected virtual void OnBeginVendItems(List<VendRequest> bins) {
            throw new NotImplementedException("Implement OnBeginVendItems in the consuming class or remove the call to base.OnBeginStockItems();");
        }

        /// <summary>
        /// Call this method to alert the caller that bins have been successfully vended
        /// </summary>
        /// <param name="vends"></param>
        protected virtual void VendSuccess(List<VendRequest> vends) {
            OnVendSuccess?.Invoke(this, new SuccessEventArgs(vends));
        }

        /// <summary>
        /// Call this method to alert the caller that bins have failed to vend
        /// </summary>
        /// <param name="vends"></param>
        /// <param name="message"></param>
        protected virtual void VendFailure(List<VendRequest> vends, string message) {
            OnVendFailure?.Invoke(this, new FailureEventArgs(vends, message));
        }

        /// <summary>
        /// Call this method to alert the caller that this vending machine has completed all vends
        /// (whether successful or failure or both)
        /// </summary>
        protected virtual void VendingComplete() {
            Dispose();
            OnVendComplete?.Invoke(this, new CompleteEventArgs());
        }
        
        private void Dispose() {
            OnDispose();
        }
        protected virtual void OnDispose() {

        }
    }
    public class CommCommand {
        public string Text { get; set; }
        public DateTime DateTime { get; set; }
    }
    public abstract class BaseCommVendingMachine : BaseVendingMachine {
        /// <summary>
        /// Comms class used to talk to the connected device
        /// </summary>
        public IComms Comms { get; protected set; }
        /// <summary>
        /// List of commands sent to the machine with the datetime they were sent
        /// </summary>
        public List<CommCommand> SentCommands { get; protected set; }
        /// <summary>
        /// List of commands recieved from the machine with the datetime they recieved
        /// </summary>
        public List<CommCommand> RecievedCommands { get; protected set; }
        /// <summary>
        /// Timeout in seconds before the vending machine gets a 'kick' as a last chance
        /// before it should automatically timeout
        /// </summary>
        public int TimeoutInSeconds { get; protected set; }

        const int defaultTimeoutInterval = 20;
        Timer timeoutTimer;

        public BaseCommVendingMachine(string name, IComms comms) : this(name, comms, 20) {

        }
        public BaseCommVendingMachine(string name, IComms comms, int timeoutIntervalSeconds) : base(name) {
            //Set up comms
            Comms = comms;
            Comms.DataRecieved += Comms_DataRecieved;
            Comms.OpenComms();
            //Set up list for storing commands
            SentCommands = new List<CommCommand>();
            RecievedCommands = new List<CommCommand>();
            //config the timeout
            TimeoutInSeconds = timeoutIntervalSeconds;
            timeoutTimer = new Timer(Timer_Tick);
            RestartTimer();
        }
        int timesTicked;
        int maxTimesTicked = 1;
        private void Timer_Tick(Object state) {
            //add to times ticked (this'll be reset when the timer is reset from a subclass)
            timesTicked++;
            //alert the subclass the timeout has been reached
            OnBeforeTimeout();
            //if the timer has not been reset more than the max then fail any pending items and complete the vend
            if (timesTicked >= maxTimesTicked) {
                VendFailure(Bins.Where(x => x.State == VendState.Pending).ToList(), string.Format("Vending machine {0} timed out.", Name));
                VendingComplete();
            }
        }
        /// <summary>
        /// Overridable method for the subclass that gets called when the timeout time has been reached
        /// </summary>
        protected virtual void OnBeforeTimeout() {
            throw new NotImplementedException("Implement OnBeforeTimeout in the consuming class or remove the call to base.OnBeforeTimeout();");
        }
        /// <summary>
        /// Restart the timer to tick once after the initially specified timeout
        /// </summary>
        protected virtual void RestartTimer() {
            int millisecondsTimeout = TimeoutInSeconds * 1000;
            timeoutTimer.Change(millisecondsTimeout, millisecondsTimeout);
            timesTicked = 0;
        }
        /// <summary>
        /// Fired when comms recieves data
        /// </summary>
        /// <param name="data"></param>
        private void Comms_DataRecieved(string data) {
            //immediatly add the command to recieved commands
            RecievedCommands.Add(new CommCommand() { Text = data, DateTime = DateTime.Now });
            //restart the timer
            RestartTimer();
            //call a method for the subclass
            OnDataRecieved(data);
        }
        /// <summary>
        /// Overridable method for subclass
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnDataRecieved(string data) {
            throw new NotImplementedException("Implement OnDataRecieved in the consuming class or remove the call to base.OnDataRecieved();");
        }
        /// <summary>
        /// Send text to the connected device
        /// </summary>
        /// <param name="command"></param>
        public void Send(string command) {
            //immediatly add the command to a list for later inspection
            SentCommands.Add(new CommCommand() { Text = command, DateTime = DateTime.Now });
            //restart the timer 
            RestartTimer();
            //finally send the command to the connected device
            Comms.SendCommand(command);
        }
        /// <summary>
        /// Overide on dispose to clean up comms and timeout timer
        /// </summary>
        protected override void OnDispose() {
            //kill comms
            Comms.CloseComms();
            Comms = null;
            //kill timer
            timeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
            timeoutTimer.Dispose();

            base.OnDispose();
        }
    }

    public abstract class BaseStructuredCommVendingMachine : BaseCommVendingMachine {
        ICommunicateViaComms machine;
        public BaseStructuredCommVendingMachine(string name, IComms comms, ICommunicateViaComms mach) : base(name, comms, int.MaxValue / 10000) {
            machine = mach;
        }
        protected override void OnBeginStockItems(List<VendRequest> bins) {

        }

        protected override void OnBeginVendItems(List<VendRequest> bins) {
            var test = machine.Start();
            if (test.ShouldReply) {
                Send(test.Reply);
            }
        }
        protected override void OnDataRecieved(string data) {
            var test = machine.WorkOnResponse(SentCommands, RecievedCommands);
            if (test.ShouldReply) {
                Send(test.Reply);
            }
            if (test.Milestone != null) {
                //TODO:: Send Message to user, complete vend etc.
                switch (test.Milestone) {
                    case VendingMilestone.DoorOpened:
                        //test success
                        VendSuccess(Bins);
                        //if all gone, complete
                        VendingComplete();
                        break;
                    default:
                        break;
                }
            }
        }
        protected override void OnBeforeTimeout() {
            var test = machine.Kick(SentCommands, RecievedCommands);
            if (test.ShouldReply) {
                Send(test.Reply);
            }
        }
    }

    public class Robo500TestVendingMachine : BaseStructuredCommVendingMachine {
        public Robo500TestVendingMachine(string commsConfig) : base("Robo500Test", CommsFactory.Create(commsConfig, true), new Robo500Communicator()) {

        }
    }
    public class TestTimeOutVendingMachine : BaseCommVendingMachine {
        public TestTimeOutVendingMachine() : base("TestTimeOutVendingMachine", CommsFactory.Create("Test", true), 1) { }
        protected override void OnBeforeTimeout() {

        }
        protected override void OnBeginVendItems(List<VendRequest> bins) {

        }
    }
    public class TestSuccessVendingMachine : BaseVendingMachine {
        public TestSuccessVendingMachine() : base("TestSuccess") {

        }
        protected override void OnBeginVendItems(List<VendRequest> bins) {
            //remove the base to stop NotImplementedException
            //base.OnBeginVendItems(bins);
            VendSuccess(bins);
            VendingComplete();
        }
    }
    public class NoVendingMachine : BaseVendingMachine {
        public NoVendingMachine() : base("NoVendingMachine") {

        }
        protected override void OnBeginVendItems(List<VendRequest> bins) {
            //base.OnBeginVendItems(bins);
            VendFailure(bins, "No vending machine was configured for these bins");
            VendingComplete();
        }
    }

    public class SuccessEventArgs : EventArgs {
        public SuccessEventArgs(List<VendRequest> bins) {
            SuccessfulBins = bins;
        }
        public List<VendRequest> SuccessfulBins { get; protected set; }
    }
    public class FailureEventArgs : EventArgs {
        public FailureEventArgs(List<VendRequest> bins, string message) {
            FailedBins = bins;
            Message = message;
        }
        public List<VendRequest> FailedBins { get; protected set; }
        public string Message { get; protected set; }
    }
    public class CompleteEventArgs : EventArgs {
    }
}

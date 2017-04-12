using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVendVendingMachines {
    public enum VendType {
        OpenDoor,
        Issue,
        Return,
        Stocking,
        Physical
    }
    public class VendingHelper {

        #region Bindables
        public VendType VendType { get; protected set; }
        /// <summary>
        /// denotes if the items should be vended or returned
        /// </summary>
        public bool IsStocking { get; protected set; }

        /// <summary>
        /// Bind these to the UI they will be updated as they succeed/fail
        /// </summary>
        public List<VendRequest> Bins { get; set; }
        /// <summary>
        /// As the vending takes place this will be updated to instruct the user in 
        /// what to do or what is happening
        /// </summary>
        public string UserMessage { get; set; }

        #endregion

        /// <summary>
        /// Called when all vending is complete whether successful or not
        /// </summary>
        public delegate void CompleteDel();
        /// <summary>
        /// Called when all vending is complete whether successful or not
        /// </summary>
        public event CompleteDel Complete;

        public VendingHelper(object context, List<VendRequest> binsToVend, VendType type) {
            //is stock going in or out?
            IsStocking = GetIsStocking(type);
            VendType = type;
            //set the bins for the UI to bind to
            Bins = binsToVend;
        }

        #region Helpers
        /// <summary>
        /// True or false value of whether stock is going into the machine
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool GetIsStocking(VendType type) {
            return type == VendType.Physical || type == VendType.Return || type == VendType.Stocking;
        }
        /// <summary>
        /// Log the bins that failed and the reason for failure
        /// </summary>
        /// <param name="failedBins"></param>
        /// <param name="message"></param>
        private void LogFailure(List<VendRequest> failedBins, string message) {
            //throw new NotImplementedException();
            //TODO:: Implement a logger
        }
        /// <summary>
        /// Update the UI models to say whether they have succeeded or failed
        /// </summary>
        /// <param name="bins"></param>
        /// <param name="success"></param>
        private void UpdateUIModelsState(List<VendRequest> bins, bool success) {
            foreach (var bin in bins) {
                var uiBin = Bins.FirstOrDefault(x => x.Id == bin.Id);
                uiBin.State = success ? VendState.Success : VendState.Failure;
            }
        }
        /// <summary>
        /// Write a transaction for each of the successful bins
        /// Issued = I
        /// Returned = R
        /// Stocked = S
        /// Physical = P
        /// </summary>
        /// <param name="successfulBins"></param>
        /// <param name="type"></param>
        private void WriteTransactions(List<VendRequest> successfulBins, VendType type) {
            //throw new NotImplementedException();
            //TODO:: Write a transaction for each of the successfully vended bins
        }
        #endregion

        #region Vending + Events
        /// <summary>
        /// Vend all bins
        /// </summary>
        /// <param name="binsToVend"></param>
        public void DoVending() {
            VendOutstandingBins();
        }

        private void VendOutstandingBins() {

            //group the bins by station
            var binsByStation = Bins.Where(x => x.State == VendState.Pending).GroupBy(x => x.StationName).FirstOrDefault();
            
            //get the bins
            var bins = binsByStation.Select(x => x).ToList();

            var firstBin = bins.FirstOrDefault();
            var stationName = firstBin.StationName;
            var stationConnDetails = firstBin.StationConnectionDetails;

            //find the station model
            IVend station = VendingMachineFactory.Create(stationName, stationConnDetails);

            //hook up the success,failure and complete events
            station.OnVendSuccess += Station_OnVendSuccess;
            station.OnVendFailure += Station_OnVendFailure;
            station.OnVendComplete += Station_OnVendComplete;

            //let the subclass know if it's returning or vending
            switch (VendType) {
                case VendType.OpenDoor:
                    //not sure what to do here
                    break;
                case VendType.Issue:
                    //Request to vend all the items
                    station.VendItems(bins);
                    break;
                case VendType.Return:
                case VendType.Stocking:
                case VendType.Physical:
                    //Request to return all items
                    station.StockItems(bins);
                    break;
                default:
                    //default should never get hit
                    throw new NotImplementedException(string.Format("VendType: {0} has not been implemented.", VendType.ToString()));
                    break;
            }

            //everything pass this point should be called via events
        }

        /// <summary>
        /// Once all vending machines have completed then tell the VM that we're done to go to next screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Station_OnVendComplete(object sender, CompleteEventArgs e) {
            //if there's any more bins to vend then do it
            if (VendsAreOutstanding()) {
                VendOutstandingBins();
            }
            else {
                //else complete the vend
                Complete?.Invoke();
            }
        }
        /// <summary>
        /// Returns true if any bins are in pending vend state
        /// </summary>
        /// <returns></returns>
        private bool VendsAreOutstanding() {
            return Bins.Any(x => x.State == VendState.Pending);
        }

        /// <summary>
        /// when the vender class says the bins have failed to vend then
        /// update the UI models and log the failure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Station_OnVendFailure(object sender, FailureEventArgs e) {
            UpdateUIModelsState(e.FailedBins, false);
            LogFailure(e.FailedBins, e.Message);
        }

        /// <summary>
        /// When the vender has said the bins were successfully vended then
        /// update the UI models and write a transaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Station_OnVendSuccess(object sender, SuccessEventArgs e) {
            UpdateUIModelsState(e.SuccessfulBins, true);
            WriteTransactions(e.SuccessfulBins, VendType);
        }
        #endregion

    }
}

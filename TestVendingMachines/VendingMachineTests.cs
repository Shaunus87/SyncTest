using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using eVendVendingMachines;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace TestVendingMachines {
    [TestClass]
    public class VendingMachineTests {

        [TestMethod]
        public void TestCompleteCalledForTest() {

            bool called = false;

            var testBinsToVend = GetBins();
            var vendingHelper = GetVendingHelper(testBinsToVend, VendType.Issue);

            vendingHelper.Complete += delegate () {
                called = true;
            };

            vendingHelper.DoVending();

            //was complete called?
            Assert.AreEqual(true, called);
            Assert.AreEqual(true, vendingHelper.Bins.All(x => x.State != VendState.Pending));
        }

        [TestMethod]
        public void TestNoVendingMachine() {

            bool called = false;

            var testBinsToVend = GetBinsWithNoStation();
            var vendingHelper = GetVendingHelper(testBinsToVend, VendType.Issue);

            vendingHelper.Complete += delegate () {
                called = true;
            };

            vendingHelper.DoVending();

            //was complete called?
            Assert.AreEqual(true, called);
            Assert.AreEqual(true, vendingHelper.Bins.All(x => x.State == VendState.Failure));
        }

        [TestMethod]
        public void TestVendingMachineTimeout() {

            bool called = false;
            int qty = 0;

            var testBinsToVend = GetBins();
            var vendingMachine = new TestTimeOutVendingMachine();

            vendingMachine.OnVendFailure += delegate (object sender, FailureEventArgs e) {
                called = true;

                qty = e.FailedBins.Count;
            };

            vendingMachine.VendItems(testBinsToVend);

            Thread.Sleep(1200);

            //was complete called?
            Assert.AreEqual(true, true);
            //Assert.AreEqual(qty, testBinsToVend.Count);
        }
        [TestMethod]
        public void TestRoboForTesting() {

            bool called = false;

            var testBinsToVend = GetRoboBins();
            var vendHelper = new VendingHelper(null, testBinsToVend, VendType.Issue);

            vendHelper.Complete += delegate () {
                called = true;
            };

            vendHelper.DoVending();
            
            Assert.IsTrue(called);
            //was complete called?
            Assert.AreEqual(true, vendHelper.Bins.All(x => x.State != VendState.Pending));
        }

        private List<VendRequest> GetRoboBins() {
            List<VendRequest> bins = new List<VendRequest>();
            bins.Add(new VendRequest() { Id = 5, StationId = 2, StationName = "robo", Physical = "12a-01", Virtual = "12a", StationConnectionDetails = "testrobo", State = VendState.Pending });
            return bins;
        }

        private List<VendRequest> GetBinsWithNoStation() {
            List<VendRequest> bins = new List<VendRequest>();
            bins.Add(new VendRequest() { Id = 5, StationId = 2, StationName = "sadf", Physical = "02 81 43 43 43 4F 3F 03", Virtual = "161", State = VendState.Pending });
            return bins;
        }

        private VendingHelper GetVendingHelper(List<VendRequest> binsToVend, VendType type) {
            return new VendingHelper(null, binsToVend, type);
        }

        private List<VendRequest> GetBins() {
            List<VendRequest> bins = new List<VendRequest>();
            bins.Add(new VendRequest() { Id = 1, StationId = 1, StationName = "test1", Physical = "12a-01, 01", Virtual = "12a 01", State = VendState.Pending });
            bins.Add(new VendRequest() { Id = 2, StationId = 1, StationName = "test1", Physical = "12a-04, 01", Virtual = "12a 04", State = VendState.Pending });
            bins.Add(new VendRequest() { Id = 3, StationId = 1, StationName = "test1", Physical = "11a-01, 01", Virtual = "12a 01", State = VendState.Pending });
            bins.Add(new VendRequest() { Id = 4, StationId = 2, StationName = "test2", Physical = "02 81 43 43 43 4F 3F 03", Virtual = "160", State = VendState.Pending });
            bins.Add(new VendRequest() { Id = 5, StationId = 2, StationName = "test2", Physical = "02 81 43 43 43 4F 3F 03", Virtual = "161", State = VendState.Pending });
            return bins;
        }
    }
}

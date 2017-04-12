using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVendVendingMachines {
    public interface IDBBin {
        long Id { get; set; }
        long StationId { get; set; }
    }
    public interface IVendableBin {
        string StationName { get; set; }
        string StationConnectionDetails { get; set; }
        string Virtual { get; set; }
        string Physical { get; set; }
    }
    public interface IUIBin {
        VendState State { get; set; }
    }
    public class VendRequest : IVendableBin, IUIBin, IDBBin {

        public long Id { get; set; }
        public long StationId { get; set; }

        public string StationName { get; set; }
        public string StationConnectionDetails { get; set; }

        public string Virtual { get; set; }
        public string Physical { get; set; }

        public int QtyToVend { get; set; }

        public VendState State { get; set; }
    }
    public enum VendState {
        Pending,
        Success,
        Failure
    }
}

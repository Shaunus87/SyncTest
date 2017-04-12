using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVendVendingMachines {
    public class BaseCommand {
        public string Command { get; protected set; }

        public BaseCommand(string command) {
            Command = command;
        }
    }
    public class AutoCribCommand : BaseCommand { 
        public AutoCribCommand(string command) : base(command) {

        }
    }
    public class AutoCribRDS : AutoCribCommand {
        public AutoCribRDS() : base("RDS") {

        }
    }
}

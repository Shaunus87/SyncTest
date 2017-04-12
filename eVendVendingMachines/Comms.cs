using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVendVendingMachines {

    public static class CommsFactory {

        public static IComms Create(string conn, bool crlf) {
            //a COM connection
            if (IsSerialConnection(conn)) {
                //Will the connected obj use newlines to denote end of comm?
                return GetSerialComms(conn, crlf);
            }
            //else try IP address
            if (IsSocketConnection(conn)) {
                return GetSocketComms(conn);
            }
            //else try test comms
            if (IsTestComms(conn)) {
                return GetTestComms(conn);
            }
            throw new CommsNotConfiguredException("Comms could not be found.");
        }

        /// <summary>
        /// Get test comms for testing purposes
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static IComms GetTestComms(string conn) {
            IComms comms;
            switch (conn.ToLower()) {
                case "testrobo":
                    comms = new TestRobo500Comms();
                    break;
                default:
                    comms = new TestEchoComms();
                    break;
            }
            return comms;
        }
        /// <summary>
        /// Get a socket connection to COM1 - ??
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static IComms GetSocketComms(string conn) {
            try {
                //split the conn by the port :
                string[] values = conn.Split(':');
                //set ip part and port part
                string ipAddress = values[0];
                string port = values[1];
                int portNum;
                //cast port to int
                if (int.TryParse(port, out portNum)) {
                    return new SocketComms(ipAddress, port);
                }
            }
            catch (Exception ex) {
                var newEx = new IPAndPortMalformedException("The IP address or Port is malformed.", ex);
                throw newEx;
            }
            //if we make it this far we must have thought it was an IP but couldn't parse part of it
            throw new IPAndPortMalformedException("The IP address or Port is malformed.");
        }
        /// <summary>
        /// Connect via IP and Port to dest
        /// </summary>
        /// <param name="comPort"></param>
        /// <param name="crlf"></param>
        /// <returns></returns>
        private static IComms GetSerialComms(string comPort, bool crlf) {
            if (crlf)
                return new SerialCommsWithCRLF(comPort);
            else {
                return new SerialCommsWithoutCRLF(comPort);
            }
        }
        /// <summary>
        /// determines if test comms
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static bool IsTestComms(string conn) {
            return conn.ToLower().Contains("test");
        }
        /// <summary>
        /// determines if socket comms
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static bool IsSocketConnection(string conn) {
            return (conn.Contains(":") && conn.Contains("."));
        }
        /// <summary>
        /// determines if serial comms
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static bool IsSerialConnection(string conn) {
            return conn.ToUpper().Contains("COM");
        }
    }

    public interface IComms {
        event CommsDataRecieved DataRecieved;

        void OpenComms();
        void CloseComms();
        void SendCommand(string commandText);
        void SendCommand(byte[] commandBytes);
    }
    public delegate void CommsDataRecieved(string data);

    #region ConcreteComms
    public class SocketComms : IComms {
        private string ipAddress;
        private string port;

        public SocketComms(string ipAddress, string port) {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        public event CommsDataRecieved DataRecieved;

        public void CloseComms() {
            throw new NotImplementedException();
        }

        public void OpenComms() {
            throw new NotImplementedException();
        }

        public void SendCommand(byte[] commandBytes) {
            throw new NotImplementedException();
        }

        public void SendCommand(string commandText) {
            throw new NotImplementedException();
        }
    }
    public class SerialCommsWithCRLF : IComms {
        private string comPort;

        public SerialCommsWithCRLF(string comPort) {
            this.comPort = comPort;
        }

        public event CommsDataRecieved DataRecieved;

        public void CloseComms() {
            throw new NotImplementedException();
        }

        public void OpenComms() {
            throw new NotImplementedException();
        }

        public void SendCommand(byte[] commandBytes) {
            throw new NotImplementedException();
        }

        public void SendCommand(string commandText) {
            throw new NotImplementedException();
        }
    }
    public class SerialCommsWithoutCRLF : IComms {
        private string comPort;

        public SerialCommsWithoutCRLF(string comPort) {
            this.comPort = comPort;
        }

        public event CommsDataRecieved DataRecieved;

        public void CloseComms() {
            throw new NotImplementedException();
        }

        public void OpenComms() {
            throw new NotImplementedException();
        }

        public void SendCommand(byte[] commandBytes) {
            throw new NotImplementedException();
        }

        public void SendCommand(string commandText) {
            throw new NotImplementedException();
        }
    }
    public class TestEchoComms : IComms {
        public event CommsDataRecieved DataRecieved;

        public void CloseComms() {
        }

        public void OpenComms() {
        }

        public void SendCommand(byte[] commandBytes) {
        }

        public void SendCommand(string commandText) {
        }
    }
    public class TestRobo500Comms : IComms {
        public event CommsDataRecieved DataRecieved;

        public void CloseComms() {
            //Do nothing
        }

        public void OpenComms() {
            //Do nothing
        }

        public void SendCommand(byte[] commandBytes) {
            //Do nothing, this won't be called
        }

        public void SendCommand(string commandText) {
            //switch on commandText and reply with some fake stuff
            //need to use if statements because of Contains

            //send RDS, get some chatter, DC for door closed, DO for door open
            if (commandText == ("RDS")) {
                //DataRecieved?.Invoke("l23423");
                //DataRecieved?.Invoke("m234");
                DataRecieved?.Invoke("DC");
            }
            //maybe get some chatter then get CI for calibration needed
            //or CC for calibration not needed
            if (commandText == ("RCS")) {
                //DataRecieved?.Invoke("l23423");
                //DataRecieved?.Invoke("m234");
                DataRecieved?.Invoke("CC");
            }
            //CAL, TST, Skipped

            //fetch bin
            if (commandText.Contains("FET")) {
                //DataRecieved?.Invoke("l23423");
                //DataRecieved?.Invoke("m234");
                DataRecieved?.Invoke("MC");
            }
            //Open door
            if (commandText.Contains("LON")) {
                //DataRecieved?.Invoke("l23423");
                //DataRecieved?.Invoke("m234");
                DataRecieved?.Invoke("DO");
            }

        }
    }
    #endregion

    #region Exceptions

    [Serializable]
    public class CommsNotConfiguredException : Exception {
        public CommsNotConfiguredException() { }
        public CommsNotConfiguredException(string message) : base(message) { }
        public CommsNotConfiguredException(string message, Exception inner) : base(message, inner) { }
        protected CommsNotConfiguredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class IPAndPortMalformedException : Exception {
        public IPAndPortMalformedException() { }
        public IPAndPortMalformedException(string message) : base(message) { }
        public IPAndPortMalformedException(string message, Exception inner) : base(message, inner) { }
        protected IPAndPortMalformedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    #endregion
}

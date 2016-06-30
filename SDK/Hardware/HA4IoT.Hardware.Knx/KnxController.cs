﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Hardware.Knx
{
    public class KnxController
    {
        //var lamp = new HA4IoT.Actuators.Lamp(new ComponentId("Lamp1"), knxController.CreateDigitalJoinEndpoint("d1"));
        SocketClient _socketClient;

        public KnxController(string hostName, int portNumber)
        {
            try
            {
                _socketClient = new SocketClient();
                string result = _socketClient.Connect(hostName, portNumber);
                Log.Verbose("knx-open socket: " + result);

                CheckPasword("");
                //Initialisation();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }

        public IBinaryStateEndpoint CreateDigitalJoinEndpoint(string identifier)
        {
            return new KnxDigitalJoinEnpoint(identifier, this);
        }

        private bool CheckPasword(string password)
        {
            string result = _socketClient.Send("p=" + password + "\x03");
            Log.Verbose("knx-send-pasword: " + result);

            if (result == "Success")
            {
                string sReceived = _socketClient.Receive();
                // p=ok\x03 or p=bad\x03
                Log.Verbose("knx-pasword answer: " + sReceived);
                if (sReceived == "p=ok\x03")
                {
                    return true;
                }
            }
            return false;

        }

        private void Initialisation()
        {
            string result = _socketClient.Send("i=1\x03");
            Log.Verbose("knx-init: " + result);

            if (result == "Success")
            {
                string sReceived = _socketClient.Receive();
                Log.Verbose("knx-init-answer: " + sReceived);
            }
        }

        public void DigitalJoinToggle(string join)
        {
            string result = _socketClient.Send(join + "=1\x03");
            Log.Verbose("knx-send-digitalJoinToggleOn: " + result);

            result = _socketClient.Send(join + "=0\x03");
            Log.Verbose("knx-send-digitalJoinToggleOff: " + result);
        }

        public void DigitalJoinOn(string join)
        {
            string result = _socketClient.Send(join + "=1\x03");
            Log.Verbose("knx-send-digitalJoinOn: " + result);
        }

        public void DigitalJoinOff(string join)
        {
            string result = _socketClient.Send(join + "=0\x03");
            Log.Verbose("knx-send-digitalJoinOff: " + result);
        }

        public void AnalogJoin(string join, double value)
        {
            if (value < 0)
                value = 0;

            if (value > 65535)
                value = 65535;
            
            string result = _socketClient.Send(join + "=" + value + "\x03");
            Log.Verbose("knx-send-analogJoin: " + result);
        }

        public void SerialJoin(string join, string value)
        { 
            string result = _socketClient.Send(join + "=" + value + "\x03");
            Log.Verbose("knx-send-SerialJoin: " + result);
        }

    }
}

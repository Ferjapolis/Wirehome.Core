﻿namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public interface ILPD433MHzSignalSender
    {
        void Send(LPD433MHzCodeSequence codeSequence);
    }
}

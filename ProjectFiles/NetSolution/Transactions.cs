#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.WebUI;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Alarm;
using FTOptix.OPCUAClient;
using FTOptix.RAEtherNetIP;
using FTOptix.Modbus;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using FTOptix.MicroController;
#endregion

public class Transactions : BaseNetLogic
{
    /// <summary>
    /// Method that will execute multiple methods related to the data
    /// transaction to and from the PLCs.
    /// </summary>
    public static void run()
    {
        //Read from every PLC in the dictionaries
        EGF.ReadFromPLCs();

        //Perform a Heartbeat Check to each PLC
        EGF.PlcHeartbeatCheck();

        //Shick <- Reading - Permission to run
        EGF.simplePLCs["101"].BMem[0] = EGF.simplePLCs["102"].B[0];

        //Reading <- Modbus 999 Test - Permission to run
        EGF.simplePLCs["102"].BMem[0] = EGF.modbusPLCs["999"].B[0];

        //Write to every PLC in the dictionaries
        EGF.WriteToPLCs();
    }    
}

#region Using directives
using UAManagedCore;
using FTOptix.NetLogic;
using FTOptix.Alarm;
using FTOptix.HMIProject;
using System;
using System.Linq;
using System.Security.Claims;
using FTOptix.Core;
using FTOptix.CoreBase;
using OpcUa = UAManagedCore.OpcUa;
using System.Collections.Generic;
using FTOptix.MicroController;
#endregion

public class PLC_Class_Simple : BaseNetLogic
{
    public static string className;
    public PLC_Class_Simple(){ }
    public bool alarmsPresent;
    public uint alarmCode = 0;
    public int [] D = new int[10];
    public bool [] B = new bool[64];
    public float [] R = new float[10];
    public int [] DMem = new int[10];
    public bool [] BMem = new bool[64];
    public float [] RMem = new float[10];
    public int heartbeatLossCounter = 0;
    public Equipment_Rockwell_Simple plc;
    public PLC_Class_Simple(Equipment_Rockwell_Simple plc)
    {
        this.plc = plc;
    }

    public override void Start()
    {
        // Class Varabile Initialization
        className = this.GetType().Name;
    }

    /// <summary>
    /// Method to read the User defined tags from the PLC.
    /// </summary>
    /// <param name="Timeout"></param>
    public void ReadFromPLC(int Timeout)
    {
        bool alarmActive = false;
        try
        {
            //Execute this command to enable field variable reading.
            plc.Data_FromPLC.ChildrenRemoteRead(Timeout);
            B = plc.Data_FromPLC.B;
            D = plc.Data_FromPLC.D;
            R = plc.Data_FromPLC.R;
        }
        catch
        {
            alarmActive = true;
        }
        GF.AlarmTrigger(plc.BrowseName, 1003, alarmActive);
    }

    /// <summary>
    /// Method to write memory data to the user defined tags in the PLC.
    /// </summary>
    /// <param name="Timeout"></param>
    public void WriteToPLC(int Timeout)
    {
        bool alarmActive = false;
        try
        {
            //Write Buffered Data to PLC tags
            plc.Data_ToPLC.GetVariable("B").RemoteWrite(BMem, Timeout);
            plc.Data_ToPLC.GetVariable("D").RemoteWrite(DMem, Timeout);
            plc.Data_ToPLC.GetVariable("R").RemoteWrite(RMem, Timeout);
        }
        catch
        {
            alarmActive = true;
        }

        GF.AlarmTrigger(plc.BrowseName, 1004, alarmActive);
    }

    /// <summary>
    /// Method to perform a Heartbeat Check on the PLC.
    /// </summary>
    public void HeartbeatCheck ()
    {
        bool alarmActive = false;
        if (DMem[0] != 0)
        {
            if (D[0] != DMem[0])
            {
                heartbeatLossCounter ++;
                if (heartbeatLossCounter > 1)
                {
                    alarmActive = true;                                 
                }
            }
            else
            {
                heartbeatLossCounter = 0;
            }
        }
        //Rollover reset
        if (DMem[0] > 19)
        {
            DMem[0] = 0;
        }

        DMem[0] = DMem[0] + 1;

        GF.AlarmTrigger(plc.BrowseName,1005, alarmActive);
    }
}

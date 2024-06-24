#region Using directives
using System;
using System.Linq;
using FTOptix.NetLogic;
using UAManagedCore;
using FTOptix.MicroController;
#endregion

public class PLC_Class_OPCUA : BaseNetLogic
{
    public static string className;
    public PLC_Class_OPCUA() { }
    public bool alarmsPresent;
    public uint alarmCode;
    public int[] D = new int[10];
    public bool[] B = new bool[64];
    public float[] R = new float[10];
    public int[] DMem = new int[10];
    public bool[] BMem = new bool[64];
    public float[] RMem = new float[10];
    public int heartbeatLossCounter = 0;
    public Equipment_OPCUA plc;
    public PLC_Class_OPCUA(Equipment_OPCUA plc)
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
            //Execute this command to enable field variable reading
            plc.Data_FromPLC.ChildrenRemoteRead(Timeout);

            //Bool Array
            int B_index = 0;

            plc.Data_FromPLC.B.ChildrenRemoteRead(Timeout).ToList().ForEach(item =>
            {
                B[B_index] = plc.Data_FromPLC.B.GetVariable(item.RelativePath.ToString()).Value;
                B_index++;
            });

            //DINT Array                
            int D_index = 0;

            plc.Data_FromPLC.D.ChildrenRemoteRead(Timeout).ToList().ForEach(item =>
            {
                D[D_index] = plc.Data_FromPLC.D.GetVariable(item.RelativePath.ToString()).Value;
                D_index++;
            });
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
            //Boolean array
            int toB_index = 0;

            plc.Data_ToPLC.B.ChildrenRemoteRead(Timeout).ToList().ForEach(item =>
            {
                plc.Data_ToPLC.B.GetVariable(item.RelativePath.ToString()).RemoteWrite(BMem[toB_index], Timeout);
                toB_index++;
            });

            //DINT array
            int toD_index = 0;

            plc.Data_ToPLC.D.ChildrenRemoteRead(Timeout).ToList().ForEach(item =>
            {
                plc.Data_ToPLC.D.GetVariable(item.RelativePath.ToString()).RemoteWrite(DMem[toD_index], Timeout);
                toD_index++;
            });
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
    public void HeartbeatCheck()
    {
        bool alarmActive = false;

        if (DMem[0] != 0)
        {
            if (D[0] != DMem[0])
            {
                heartbeatLossCounter++;
                if (heartbeatLossCounter > 3)
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

        GF.AlarmTrigger(plc.BrowseName, 1005, alarmActive);
    }
}

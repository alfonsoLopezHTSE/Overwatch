#region Using directives
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using System.Collections.Generic;
using System.Linq;
using FTOptix.MicroController;
#endregion

public class EGF : BaseNetLogic
{
    public static string className;
    public static int readTimeout;
    public static int writeTimeout;
    public static Dictionary<string,PLC_Class_Simple> simplePLCs= new Dictionary<string,PLC_Class_Simple>();
    public static Dictionary<string,PLC_Class_Modbus> modbusPLCs= new Dictionary<string,PLC_Class_Modbus>();
    public static Dictionary<string, PLC_Class_OPCUA> OPCUAPLCs = new Dictionary<string, PLC_Class_OPCUA>();


    public override void Start()
    {
        // Class Varabile Initialization
        className = this.GetType().Name;
        readTimeout = MainTask.period / 2;
        writeTimeout = MainTask.period / 2;

        GF.OverwatchLog(className, "Start", "Successful Completion");
    }

    /// <summary>
    /// Search the Model folder for the different Equipment UDT Types
    /// and create a corresponding dictionary using the PLC Id value
    /// as the Key and a unique class variable for the Value.
    /// </summary>
    public static void CreateDictionaries()
    {
        try{
            // Get Children Of The Model Folder
            var modelFolder = Project.Current.Find("Model");

            // Loop For Any Simple Rockwell PLC Object Type
            modelFolder.FindNodesByType<Equipment_Rockwell_Simple>().ToList().ForEach(e => 
            {
                simplePLCs.Add(e.PLC_Information.PLCId.ToString(), new PLC_Class_Simple(e));
            });

            //Loop for any Modbus PLC Object Type
            modelFolder.FindNodesByType<Equipment_Modbus>().ToList().ForEach(e => 
            {
                modbusPLCs.Add(e.PLC_Information.PLCId.ToString(), new PLC_Class_Modbus(e));
            });

            //Loop for any OPCUA PLC Object Type
            modelFolder.FindNodesByType<Equipment_OPCUA>().ToList().ForEach(e =>
            {
                OPCUAPLCs.Add(e.PLC_Information.PLCId.ToString(), new PLC_Class_OPCUA(e));
            });

            GF.OverwatchLog(className, "createDictionaries", "Successful Completion");
        }
        catch
        {
            GF.OverwatchAlarm(className, "createDictionaries", 1002);
        }
    }

    /// <summary>
    /// Read UDT Tags from every PLC according to its data type
    /// </summary>
    public static void ReadFromPLCs()
    {
        foreach(var item in simplePLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.ReadFromPLC(MainTask.period / 2);
            }
        }

        foreach(var item in modbusPLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.ReadFromPLC(MainTask.period / 2);
            }
        }

        foreach (var item in OPCUAPLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.ReadFromPLC(MainTask.period / 2);
            }
        }
    }

    /// <summary>
    /// Perform a Heartbeat Check to every PLC
    /// </summary>
    public static void PlcHeartbeatCheck()
    {
        foreach (var item in simplePLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.HeartbeatCheck();
            }
        }

        foreach (var item in modbusPLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.HeartbeatCheck();
            }
        }

        foreach (var item in OPCUAPLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.HeartbeatCheck();
            }

        }

    }

    /// <summary>
    /// Write UDT Tags to every PLC according to its data type
    /// </summary>
    public static void WriteToPLCs()
    {
        foreach(var item in simplePLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.WriteToPLC(MainTask.period / 2);
            }
        }

        foreach(var item in modbusPLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.WriteToPLC(MainTask.period / 2);
            }
        }

        foreach (var item in OPCUAPLCs)
        {
            if (!item.Value.plc.Controls.DisableRemoteControl)
            {
                item.Value.WriteToPLC(MainTask.period / 2);
            }
        }
    }
}

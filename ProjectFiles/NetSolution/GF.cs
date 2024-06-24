#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using UAManagedCore;
using FTOptix.Alarm;
using OpcUa = UAManagedCore.OpcUa;
using System.Security.Claims;
using FTOptix.MicroController;
#endregion

public class GF : BaseNetLogic
{
    private string className;
    private static bool EnableDebugLogging;
    public static ValueMapConverterType AlarmsConverter;
    public static Dictionary<UInt32, string> alarmsDict = new Dictionary<UInt32, string>();
    public static Dictionary<String, IUAVariable> alarmsDynamicLinkDict = new Dictionary<String, IUAVariable>();

    public override void Start()
    {
        // Class Varabile Initialization
        className = this.GetType().Name;

        //Get Alarms Converter
        GetAlarmsConverter();

        //Get Netlogic Variables
        GetLogicObjects();
    }

    /// <summary>
    /// Get the Logic Objects from the GF Netlogic Object and assign it to variables
    /// </summary>
    private void GetLogicObjects()
    {
        // Get NetLogic Variables
        try
        {
            EnableDebugLogging = LogicObject.GetVariable("EnableDebugLogging").Value;
            OverwatchLog(className, "Start", "Successful Completion");
        }
        catch
        {
            GF.OverwatchAlarm(className, "getLogicObjects", 1000);
        }
    }

    /// <summary>
    /// Method to extract the Alarms Converter from the project and to add them
    /// to a global dictionary to be used by the alarm methods.
    /// </summary>
    private void GetAlarmsConverter()
    {
        try
        {
            //Loop through the children of the alarm converter
            Project.Current.Find("Converters").FindNodesByType<ValueMapConverterType>().ToList().ForEach(converter =>
            {
                if (converter.BrowseName == "KeyValueConverter_Alarms")
                {
                    var arr = converter.ChildrenRemoteRead().ToArray();

                    //Loop through the converter array key-value sequence
                    //and add them to the alarms dictionary
                    for (int i = 0; i < arr.Length - 1; i+=2)
                    {
                        alarmsDict.Add(arr[i].Value, arr[i+1].Value);                        
                    }
                }
            });
            Log.Info("Alarm Converter", "Success");
        }
        catch
        {
            GF.OverwatchAlarm(className, "GetAlarmsConverter", 1006);
        }
    }

    /// <summary>
    /// Global method used to log messages for debugging purposes
    /// </summary>
    /// <param name="ClassName"></param>
    /// <param name="Routine"></param>
    /// <param name="LogMessage"></param>
    public static void OverwatchLog(string ClassName, string Routine, string LogMessage)
    {
        if (EnableDebugLogging)
        {
            string formattedMessage = String.Join(string.Empty, new String[] { "(", ClassName, " ", Routine, ") ", LogMessage });
            Log.Info("Overwatch Log", formattedMessage);
        }
    }

    /// <summary>
    /// Global method used to log Overwatch system alarms.
    /// </summary>
    /// <param name="ClassName"></param>
    /// <param name="Routine"></param>
    /// <param name="AlarmCode"></param>
    public static void OverwatchAlarm(string ClassName, string Routine, uint AlarmCode)
    {
        string formattedMessage = String.Join(string.Empty, new String[] { "(", ClassName, " ", Routine, " ", "Alarm Code: " + AlarmCode.ToString(),  ") ", alarmsDict[AlarmCode] });
        Log.Info("Overwatch Alarm", formattedMessage);
    }

    /// <summary>
    /// Global method used to trigger PLC related alarms
    /// </summary>
    /// <param name="plcName"></param>
    /// <param name="faultCode"></param>
    /// <param name="trigger"></param>
    public static void AlarmTrigger(string plcName, uint faultCode, bool trigger)
    {
        var alarmsFolder = Project.Current.Get("Alarms");
        String alarmTag = "";
        bool existingAlarm = false;
        string name = faultCode + "_" + plcName;

        //Check if there are any alarms on the alarms folder
        if (alarmsFolder.ChildrenRemoteRead().Count()!=0)
        {
            //Loop through the alarms in the folder to find if an alarm there matches the alarm being triggered
            foreach (var alarm in alarmsFolder.ChildrenRemoteRead().ToList())
            {
                //Extract the string up to the "/" character
                int stop = alarm.RelativePath.IndexOf("/");
                if (stop != -1)
                {
                    alarmTag = alarm.RelativePath.Substring(0, stop);
                    if (alarmTag == name)
                    {
                        existingAlarm = true;
                    }
                }
            }
        }
        //If this alarm does not already exist:
        if (!existingAlarm)
        {
            AlarmController newAlarm;

            //create a runtime variable and add it to the alarms Dynamic Link Dictionary
            //and assign it the "trigger" value.
            alarmsDynamicLinkDict.Add(name, InformationModel.MakeVariable(name, OpcUa.DataTypes.Boolean));
            alarmsDynamicLinkDict[name].Value = trigger;

            //Create the alarm object and set a dynamic link with the dynamic Link Dictionary from above
            newAlarm = InformationModel.MakeObject<DigitalAlarm>(name);
            newAlarm.InputValueVariable.SetDynamicLink(alarmsDynamicLinkDict[name]);
            newAlarm.AutoAcknowledge = false;
            newAlarm.AutoConfirm = false;
            newAlarm.Enabled = true;
            newAlarm.Severity = 1;
            //Extract the message of the alarm from the alarms Dictionary
            newAlarm.Message = alarmsDict[faultCode];            

            alarmsFolder.Add(newAlarm);
        }
        //If the alarm already exists, set the value to the "trigger" value
        else
        {
            if (alarmsDynamicLinkDict.ContainsKey(name))
                {
                    alarmsDynamicLinkDict[name].Value = trigger;
                }
            }
        }
}

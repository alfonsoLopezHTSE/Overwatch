#region Using directives
using UAManagedCore;
using FTOptix.NetLogic;
using System.Timers;
using FTOptix.Alarm;
using FTOptix.HMIProject;
using System.Linq;
using System.Transactions;
using FTOptix.MicroController;
#endregion

public class MainTask : BaseNetLogic
{
    public static string className;
    private PeriodicTask mainTask;
    public static int period = 0;
    public static int plcStartupTime = 0;
    public static bool systemInitialized = false;

    public override void Start()
    {
        // Class Varabile Initialization
        className = this.GetType().Name;

        //Get MainTask Logic Objects
        GetLogicObjects();

        //Trigger Startup Timer
        PLCStartupTimer();

        //Periodic Task
        mainTask = new PeriodicTask(Main, period, LogicObject);
        mainTask.Start();

        //Global Function Logging
        GF.OverwatchLog(className, "Start", "Successful Completion");
    }

    public override void Stop()
    {
        var alarmsFolder = Project.Current.Get("Alarms");
        alarmsFolder.Children.Clear();
        mainTask.Dispose();
    }

    public static void PLCStartupTimer()
    {
        System.Timers.Timer initTimer = new System.Timers.Timer();
        initTimer.Elapsed += new System.Timers.ElapsedEventHandler(PLCStartupTimerEvent);
        initTimer.Interval = plcStartupTime;
        initTimer.Enabled = true;
        initTimer.AutoReset = false;
        GF.OverwatchLog(className, "PLCStartupConnectionTimer", "Successful Start");
    }

    private static void PLCStartupTimerEvent(object source, ElapsedEventArgs e)
    {
        Initialize();
        GF.OverwatchLog(className, "StartupTimer", "Successful Completion");
    }

    public void Main()
    {
        if (systemInitialized)
        {
            Transactions.run();
        }
    }

    /// <summary>
    /// Get the Logic Objects from the GF Netlogic Object and assign it to variables
    /// </summary>
    private void GetLogicObjects()
    {
        try
        {
            period = LogicObject.GetVariable("period_ms").Value;
            plcStartupTime = LogicObject.GetVariable("PLCStartupConnectionDelay").Value;
        }
        catch
        {
            GF.OverwatchAlarm(className, "GetLogicObjects", 1000);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Initialize()
    {
        EGF.CreateDictionaries();
        var alarmsFolder = Project.Current.Get("Alarms");
        alarmsFolder.Children.Clear();
        systemInitialized = true;
        GF.OverwatchLog(className, "Initialize", "Successful Completion");
    }


}

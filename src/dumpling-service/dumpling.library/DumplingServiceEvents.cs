namespace DumplingLib
{
    public class CommonEvent
    {
        public string Type { get; set; } 
        public int Rank { get; private set; }
        public int Cardinality { get; set; } = 1;
        public string Category { get; set; } = "bizprofile";

        public CommonEvent(string type, int rank)
        {
            Type = type;
            Rank = rank;
        }
    }

    #region helix proxy event
    public class HelixServiceProxyWorkItemsStatus : CommonEvent
    {
        public HelixServiceProxyWorkItemsStatus() : base("HelixServiceProxy-WorkItems-Status", 0) { }

    }
    #endregion

    #region dumpling web api events
    public class WebAPIStartUploadChunkEvent : CommonEvent
    {
        public WebAPIStartUploadChunkEvent() : base("DumplingService-WebAPI-StartUploadChunk", 1) { }
    }
    public class WebAPIFinishedUploadChunkEvent : CommonEvent
    {
        public WebAPIFinishedUploadChunkEvent() : base("DumplingService-WebAPI-FinishedUploadChunk", 2) { }
    }

    public class WebAPIGreetingEvent : CommonEvent
    {
        public WebAPIGreetingEvent() : base("DumplingService-WebAPI-Greeting", 3) { }
    }
    public class WebAPIGetStatusEvent: CommonEvent
    {
        public WebAPIGetStatusEvent() : base("DumplingService-WebAPI-GetStatus", 4) { }
    }

    public class WebAPIGetDumpURIEvent : CommonEvent
    {
        public WebAPIGetDumpURIEvent() : base("DumplingService-WebAPI-GetDumpURI", 4) { } // same rank as GetStatus.
    }

    #endregion  

    #region dumpling data worker events

    public class DataWorkerStartEvent : CommonEvent
    {
        public DataWorkerStartEvent() : base("DumplingService-DataWorker-StartService", 16) { }
    };
    public class DataWorkerStopEvent : CommonEvent
    {
        public DataWorkerStopEvent() : base("DumplingService-DataWorker-StopService", 17) { }
    };
    public class DataWorkerRunEvent : CommonEvent
    {
        public DataWorkerRunEvent() : base("DumplingService-DataWorker-RunService", 18) { }
    };
    public class DataWorkerMessageReceivedEvent : CommonEvent
    {
        public DataWorkerMessageReceivedEvent() : base("DumplingService-DataWorker-ReceivedMessage", 12) { }
    };

    public class DataWorkerDeadLetterEvent : CommonEvent
    {
        public DataWorkerDeadLetterEvent() : base("DumplingService-DataWorker-DeadLetterMessage", 13) { }
    };

    public class DataWorkerExceptionEvent : CommonEvent
    {
        public DataWorkerExceptionEvent() : base("DumplingService-DataWorker-Exception", 14) { }
    };

    public class DataWorkerCompletedMessageEvent : CommonEvent
    {
        public DataWorkerCompletedMessageEvent() : base("DumplingService-DataWorker-CompletedMessage", 15) { }

    };
    #endregion
}

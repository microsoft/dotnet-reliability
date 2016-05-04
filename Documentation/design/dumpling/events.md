####The Events

The name scheme of the event types is:
{*Emitting Service*}-{*Emitting Piece of Service*}-{*Event Type*}

Where we see {os} means that any analyzer os we have setup will have a corresponding event.

**rank**.`event type`

0. HelixServiceProxy-WorkItems-Status
1. DumplingService-WebAPI-StartUploadChunk
2. DumplingService-WebAPI-FinishedUploadChunk
3. DumplingService-WebAPI-Greeting
4. DumplingService-WebAPI-GetStatus
5. DumplingService-WebAPI-GetDumpURI
6. DumplingService-{os}Analyzer-StartService
7. DumplingService-{os}Analyzer-StartDownload
8. DumplingService-{os}Analyzer-StartUnzip
9. DumplingService-{os}Analyzer-StartAnalyze
10. DumplingService-{os}Analyzer-SaveResults
11. DumplingService-{os}Analyzer-Complete
12. DumplingService-{os}Analyzer-Failure
13. DumplingService-DataWorker-ReceiveMessage
14. DumplingService-DataWorker-DeadLetteredMessage
15. DumplingService-DataWorker-Exception
16. DumplingService-DataWorker-CompletedMessage
17. DumplingService-DataWorker-StartService
18. DumplingService-DataWorker-StopService
19. DumplingService-DataWorker-RunService
20. DumplingService-DataWorker-SqlIndex

using RetrieveAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dumps
{
    class ReportEmitter
    {
        const int CONFIG_REPORT_LAST_MODIFIED_TIMEOUT = 60000*10; // milliseconds
        const int CONFIG_CHECK_JOB_STATUS_PERIOD = 2000; // milliseconds

        JobMetadataHelper _jobHelper;

        public async Task ConditionedConstruct(string state)
        {
            _jobHelper = new JobMetadataHelper(AzureClientInstances.RAPDropBlobHelper, state);
            var container = _jobHelper.BoundContainer;

            Console.WriteLine($"Job started at {_jobHelper.StartTime}");

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Job Id: {state}");
                Console.WriteLine($"Worker State: {_jobHelper.WorkerState} (running until state is 'done' or until timeout is reached. timeout is 10 minutes after last update.)");
                Console.WriteLine($"{_jobHelper.JobCountCompleted} / {_jobHelper.JobCount} - Last Update (UTC): {_jobHelper.LastUpdate}");
                
                if(Console.KeyAvailable)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Spacebar)
                    {
                        Console.WriteLine("Force writing a report.");
                        AnalysisStorage storage = new AnalysisStorage(state);

                        WriteReport(state, await storage.GroupOnKey());
                    }
                }
                else if (DateTimeOffset.UtcNow - container.Properties.LastModified <=  TimeSpan.FromMilliseconds(CONFIG_REPORT_LAST_MODIFIED_TIMEOUT)
                    && _jobHelper.JobCountCompleted != _jobHelper.JobCount && _jobHelper.WorkerState != "done")
                {
                    // read metadata to show wait status.
                    await Task.Delay(CONFIG_CHECK_JOB_STATUS_PERIOD);
                }
                else
                {
                    Console.WriteLine("Writing report.");
                    AnalysisStorage storage = new AnalysisStorage(state);

                    WriteReport(state, await storage.GroupOnKey());

                    break;
                }
            }
        }
        /// <summary>
        /// ew.
        /// </summary>
        /// <param name="data"></param>
        private void WriteReport(string state, string data)
        {
            string form = @"<!DOCTYPE html
<html lang='en'>
<head>
    <meta charset = 'utf-8' />
 
     <title> Stress Results </title>
    
        <style>
            body {
                font - family: 'Segoe UI', sans - serif;
            }

            span {
                font - style: italic;
            }

        .hsonuc {
                position: absolute;
                top: 0px;
                left: 1920px;
                margin - right:200px; /* Positions 200px to the left of center */
            }
    </style>
    <script>
        var ContextTable = (function() {
                function ContextTable(view) {
                    this._view = view;
                }
                ContextTable.prototype.clearTable = function() {
                    while (this._view.rows.length> 0)
                    {
                        this._view.deleteRow(0);
                    }
                };
                ContextTable.prototype.updateData = function(store) {
                    this.clearTable();
        for (var x in store)
                    {
                        var row = this._view.insertRow();
                        var cellKey = row.insertCell();
                        var cellValue = row.insertCell();
                        cellKey.vAlign = 'top';
                        cellKey.innerText = x;
                        if (store[x].substr(0, 5) == 'https')
                        {
                            var element = document.createElement('a');
                            element.text = 'DOWNLOAD CORE DUMP';
                            element.href = store[x];
                            cellValue.appendChild(element);
                        }
                        else {
                            cellValue.innerText = store[x];
                        }
                    }
                };
                return ContextTable;
            })();
            var Table = (function() {
                function Table(contentViewElement, context) {
                    Table.s_context = context;
                    if (!Table.static_initialized)
                    {
                        this.static_constructor();
                    }
                    this._view = contentViewElement;
                    this._view.border = '0';
                    var header = this._view.createTHead();
                    var data = " + data + @";
                    for (var x in data)
                    {
                        this.addData(data[x]);
                    }
                    this.createGroups();
                }
            
                Table.prototype.static_constructor = function() {
                    Table.testNameStore = { };
                    Table.blameSymbolStore = { };
                    Table.static_initialized = true;
                };
                Table.prototype.createCell = function(value) {
                    var cell = new HTMLTableDataCellElement();
                    cell.innerHTML = value;
                    return cell;
                };
                Table.prototype.tableCellHighlightOver = function(evt) {
                    if (Table.s_selectedElement != null)
                    {
                        Table.s_selectedElement.bgColor = '#FFFFFF';
                    }
                    var target = evt.target;
                    Table.s_selectedElement = evt.target;
                    target.bgColor = '#9090D4';
                    Table.s_context.updateData(Table.testNameStore[target.innerText].store);
                };
                Table.prototype.tableCellHighlightOut = function(evt) {
                    var target = evt.target;
                    target.bgColor = '#FFFFFF';
                };
                Table.prototype.createOrAddToGroup = function(testdata) {
                    if (Table.blameSymbolStore[testdata.groupSymbol] == undefined)
                    {
                        Table.blameSymbolStore[testdata.groupSymbol] = [];
                    }
                    Table.blameSymbolStore[testdata.groupSymbol].push(testdata);
                };
                Table.prototype.createGroups = function() {
                    var _this = this;
        for (var group in Table.blameSymbolStore)
                    {
                        var first = true;
                        Table.blameSymbolStore[group].forEach(function(x) {
                            var newRow = _this._view.insertRow();
                            if (first)
                            {
                                var groupCell = newRow.insertCell();
                                groupCell.rowSpan = Table.blameSymbolStore[group].length;
                                groupCell.textContent = group;
                                groupCell.vAlign = 'top';
                                first = false;
                            }
                            var testNameCell = newRow.insertCell();
                            testNameCell.onmouseover = _this.tableCellHighlightOver;
                            testNameCell.textContent = x.testName + '_' + x.store['HELIX_CORRELATION_ID'];
                        });
                    }
                };
                Table.prototype.addData = function(data) {
                    Table.testNameStore[data.testName + '_' + data.store['HELIX_CORRELATION_ID']] = data;
                    this.createOrAddToGroup(data);
                };
                Table.static_initialized = false;
                return Table;
            })();
            var g_groupsElement;
            var g_contextElement;
            window.onload = function() {
                g_groupsElement = document.getElementById('groups');
                g_contextElement = document.getElementById('context');
                var cTable = new ContextTable(g_contextElement);
                var table = new Table(g_groupsElement, cTable);
            };
    </script>
</head>
<body>
    <h1> Stress Core Dumps</h1>
   

           <table id = 'groups'> </table>
    

            <div class='hsonuc'>
            <table id = 'context'> </table>
        </div>


</body>
</html>
";
            File.WriteAllText(state + ".html", form);
            Console.WriteLine($"Created {Path.Combine(Environment.CurrentDirectory, state + ".html")}");
        }
    }
}

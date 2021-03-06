﻿// MIT License
// 
// Copyright (c) 2020 Mertcan Davulcu
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


//Libraries
open System
open DbEngine
open MhqlEngine
open MhqlStress
open FileEngine
open MochaDB
open MochaDB.Connection
open MochaDB.MochaScript

//OnEcho event of script debugger.
let OnEcho(e: MochaScriptEchoEventArgs) =
    Console.WriteLine(e.Message)

//Entry function.
[<EntryPoint>]
let main argv =
    Console.WriteLine "MochaDB FSharp Test Console"
    let path = new MochaPath __SOURCE_DIRECTORY__
    path.ParentDirectory()
    path.ParentDirectory()
    let path = path.Path + "/testdocs"
    let dbprovider = "path=" + (path + "/testdb.mhdb") + "; password=; AutoConnect=true"
    let db = GetDbWithProvider dbprovider
    while true do
        printf "Command: "
        let input = Console.ReadLine()
        if(input.Length > 0) then
            if input.Equals("clear",StringComparison.InvariantCultureIgnoreCase) then
                Console.Clear()
                Console.WriteLine "MochaDB FSharp Test Console"
            elif input.Equals("exit",StringComparison.InvariantCultureIgnoreCase) then
                exit 0
            elif input.Equals("connectdb",StringComparison.InvariantCultureIgnoreCase) then
                if db.State = MochaConnectionState.Connected then
                    Console.WriteLine "Connection is already open!"
                else
                    db.Connect()
                    Console.WriteLine "Connected!"
            elif input.Equals("disconnectdb",StringComparison.InvariantCultureIgnoreCase) then
                if db.State = MochaConnectionState.Disconnected then
                    Console.WriteLine "Connection is already closed!"
                else
                    db.Disconnect()
                    Console.WriteLine "Disconnected!"
            elif input.Equals("getscript",StringComparison.InvariantCultureIgnoreCase) then
                Console.WriteLine "\n\n------ Script Content ------\n\n"
                let content = GetFileContent(path + "/testscript.mochascript")
                Console.Write content
                Console.WriteLine "\n\n------ Script Content ------\n\n"
            elif input.StartsWith("reader ",StringComparison.InvariantCultureIgnoreCase) then
                Reader(db,input.[6..])
            elif input.StartsWith("scalar ",StringComparison.InvariantCultureIgnoreCase) then
                Scalar(db,input.[6..])
            elif input.Equals("cncstate",StringComparison.InvariantCultureIgnoreCase) then
                Console.WriteLine db.State
            elif input.StartsWith("mhqlstresscmd",StringComparison.InvariantCultureIgnoreCase) then
                Console.WriteLine "\n\n------ MHQL Stress Test -----\n\n"
                StartMhqlTableGetStressCmd(db,input.Split(' ').[1])
            elif input.StartsWith("mhqlstresst",StringComparison.InvariantCultureIgnoreCase) then
                Console.WriteLine "\n\n------ MHQL Stress Test -----\n\n"
                let parts = input.Split(' ');
                StartMhqlTableGetStressWithTickCmd(db,Int32.Parse(parts.[1]),input.Substring(13 + parts.[1].Length))
                Console.WriteLine "\n\n------ MHQL Stress Test -----\n\n"
            elif input.Equals("mhqlstress",StringComparison.InvariantCultureIgnoreCase) then
                Console.WriteLine "\n\n------ MHQL Stress Test -----\n\n"
                StartMhqlTableGetStress db
            elif input.StartsWith("mhqlstress",StringComparison.InvariantCultureIgnoreCase) then
                Console.WriteLine "\n\n------ MHQL Stress Test -----\n\n"
                StartMhqlTableGetStressWithTick(db,Int32.Parse(input.Split(' ').[1]))
                Console.WriteLine "\n\n------ MHQL Stress Test -----\n\n"
            elif input.Equals("runscript",StringComparison.InvariantCultureIgnoreCase) then
                let debugger = GetScriptDebugger(path + "/testscript.mochascript")
                debugger.Echo.Add(OnEcho)
                Console.WriteLine "\n\n------ Script Debug Output ------\n\n"
                db.Disconnect()
                debugger.DebugRun()
                Console.WriteLine "\n\n------ Script Debug Output ------\n\n"
                debugger.Dispose()
                db.Connect()
            else
                db.Query.MochaQ.SetCommand(input);
                ExecuteCommand(db)
        else
            Console.WriteLine "ERROR: Empty command!"

    0

﻿' MIT License
' 
' Copyright(c) 2020 Mertcan Davulcu
' 
' Permission Is hereby granted, free Of charge, to any person obtaining a copy
' of this software And associated documentation files (the "Software"), to deal
' in the Software without restriction, including without limitation the rights
' to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
' copies of the Software, And to permit persons to whom the Software Is
' furnished to do so, subject to the following conditions
' 
' The above copyright notice And this permission notice shall be included In all
' copies Or substantial portions of the Software.
' 
' THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
' IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
' LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
' OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
' SOFTWARE.


Imports System.IO
Imports MochaDB
Imports MochaDB.Mhql

Public Class app
    Private Sub codebox_KeyDown(sender As Object, e As KeyEventArgs) Handles codebox.KeyDown
        Dim path = New MochaPath(Directory.GetCurrentDirectory)
        path.ParentDirectory()
        path.ParentDirectory()
        path.ParentDirectory()
        Dim db As MochaDatabase = New MochaDatabase($"Path={path.Path}/testdocs/testdb; Password=; AutoConnect=True")

        Try
            If e.KeyCode = Keys.F5 Then
                Dim command As MochaDbCommand = New MochaDbCommand(codebox.Text, db)
                Dim mhqlcommand As MhqlCommand = codebox.Text
                If mhqlcommand.IsReaderCompatible Then
                    Dim result As MochaTableResult = command.ExecuteScalar

                    datasource.Columns.Clear()
                    datasource.Rows.Clear()

                    For index = 0 To result.Columns.Count - 1
                        datasource.Columns.Add("", result.Columns.ElementAt(index).Name)
                    Next
                    For index = 0 To result.Rows.Count - 1
                        datasource.Rows.Add(result.Rows.ElementAt(index).Datas.ToArray)
                    Next
                Else
                    command.ExecuteCommand()
                End If
            End If
        Catch excep As MochaException
            MessageBox.Show(excep.Message, "VB.NET MHQL Test App", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch excep As Exception
            MessageBox.Show(excep.Message, "VB.NET MHQL Test App", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        db.Dispose()

    End Sub
End Class

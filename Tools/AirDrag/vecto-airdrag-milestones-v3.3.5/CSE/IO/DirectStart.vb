Public Module DirectStart
    ' Control if values are given with the start
    Public Function fGetArgs() As Boolean
        ' Get the command lines
        Environment.GetCommandLineArgs()

        ' Use the command lines to start the calculation
        If Environment.GetCommandLineArgs().Length > 1 Then
            JobFile = Environment.GetCommandLineArgs(1)
            If Environment.GetCommandLineArgs().Length >= 3 Then
                OutFolder = Environment.GetCommandLineArgs(2)
            Else
                OutFolder = joinPaths(fPath(JobFile), "Results\")
            End If
            Return True
        Else
            Return False
        End If
    End Function
End Module

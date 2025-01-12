﻿Imports System.IO

''' <summary>
''' 日志模块，记录程序运行的事件、警告和错误信息。
''' </summary>
Public Module Logger
    Private ReadOnly LogFilePath As String = Path.Combine(Path.GetTempPath(), "OutlookPlugin.log")
    Private Const MaxLogFileSize As Long = 1024 * 1024 ' 1 MB
    Private ReadOnly LogFileLock As New Object()
    Private ReadOnly EnableConsoleOutput As Boolean = True ' 从配置加载

    ''' <summary>
    ''' 日志级别枚举
    ''' </summary>
    Public Enum LogLevel
        Debug
        Info
        Warning
        [Error]
        Critical
    End Enum

    ''' <summary>
    ''' 通用的日志记录方法，支持日志级别和模块名。
    ''' </summary>
    Public Sub WriteLog(level As LogLevel, moduleName As String, message As String)
        Try
            SyncLock LogFileLock
                ' 检查日志文件是否存在
                EnsureLogFileExists()

                ' 检查日志文件大小并归档
                ManageLogFile()

                ' 格式化日志条目
                Dim logEntry As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {moduleName} - {message}"

                ' 写入日志文件
                File.AppendAllText(LogFilePath, logEntry & Environment.NewLine)

                ' 控制台输出（根据配置和日志级别）
                If EnableConsoleOutput AndAlso level >= LogLevel.Warning Then
                    Console.WriteLine(logEntry)
                End If
            End SyncLock
        Catch ex As Exception
            Console.WriteLine($"无法写入日志: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' 确保日志文件存在，如果不存在则创建
    ''' </summary>
    Private Sub EnsureLogFileExists()
        Try
            If Not File.Exists(LogFilePath) Then
                File.WriteAllText(LogFilePath, "日志文件已创建。" & Environment.NewLine)
            End If
        Catch ex As Exception
            Console.WriteLine($"创建日志文件失败: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' 根据文件大小管理日志文件（归档旧日志）
    ''' </summary>
    Private Sub ManageLogFile()
        Try
            If File.Exists(LogFilePath) AndAlso New FileInfo(LogFilePath).Length > MaxLogFileSize Then
                Dim archivePath As String = Path.Combine(Path.GetDirectoryName(LogFilePath),
                                                          $"OutlookPlugin_{DateTime.Now:yyyyMMddHHmmss}.log")
                File.Move(LogFilePath, archivePath)
                Console.WriteLine($"日志文件已归档: {archivePath}")
            End If
        Catch ex As Exception
            Console.WriteLine($"日志归档失败: {ex.Message}")
        End Try
    End Sub
End Module
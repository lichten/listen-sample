Imports System.Text

Imports ListenSample.Utility

Public Class Form1
    ''' <summary>受信を行うオブジェクト</summary>
    Private ReadOnly _client As ReceiveThread = New ReceiveThread()

    ''' <summary>画面上に表示するログメッセージ</summary>
    Private ReadOnly _recentLog As StringBuilder = New StringBuilder()

    ''' <summary>排他処理用のロックオブジェクト</summary>
    Private ReadOnly _lockObject As Object = New Object()

    ''' <summary>受信を行うオブジェクトから、メッセージを受け取ったときの処理</summary>
    ''' <param name="message">メッセージ</param>
    Private Sub RecvLog(ByVal message As String)
        ' ログ追加する
        Dim logMessage As String = JpnFormat("{0}", message)
        ' 画面表示用の変数への代入
        SyncLock _lockObject
            ' 最大長を超えていたら、古いメッセージを削る
            Const maxLogLength As Integer = 7500
            If _recentLog.Length > maxLogLength Then
                _recentLog.Remove(0, maxLogLength \ 3)
            End If
            ' 改行してメッセージを追加する
            _recentLog.AppendLine(logMessage)
        End SyncLock
    End Sub

    Private Sub initButton_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles initButton.Click
        Dim post As PostLog = New PostLog(AddressOf RecvLog)

        Dim isSuccess As Boolean = _client.Initialize(post, IpAddressTextBox.Text, JpnIntegerParse(PortNumberTextBox.Text))
        If isSuccess = False Then
            RecvLog("Initialize呼び出しでエラー発生")
        End If

    End Sub

    Private Sub ConnectStartButton_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles ConnectStartButton.Click
        _client.RequestConnectStart(IpAddressTextBox.Text, JpnIntegerParse(PortNumberTextBox.Text))

    End Sub

    Private Sub ConnectStopButton_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles ConnectStopButton.Click
        _client.RequestConnectStop()

    End Sub

    Private Sub SendTextButton_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles SendTextButton.Click
        Dim req As ReqData = New ReqData()
        req.RequestKind = ReqType.SendText
        req.TextData = SendTextTextBox.Text

        _client.AddRequest(req)

    End Sub

    Private Sub LogUpdateTimer_Tick(ByVal sender As System.Object, ByVal e As EventArgs) Handles LogUpdateTimer.Tick
        ' ログの表示を更新
        SyncLock _lockObject
            LogTextBox.Text = _recentLog.ToString()
            LogTextBox.SelectionStart = LogTextBox.Text.Length
            ' LogTextBox.Focus()
            LogTextBox.ScrollToCaret()
        End SyncLock

    End Sub

    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        If _client IsNot Nothing Then
            _client.RequestStop()
        End If

        ' TODO アクセスする方法考える
        _client.StopCompleteEvent.WaitOne(2000)

    End Sub
End Class

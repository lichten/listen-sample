Public Class Form1
    ''' <summary>Listenするオブジェクト</summary>
    Private _listenObject As ListenThread = New ListenThread()

    ''' <summary>画面上に表示するログメッセージ</summary>
    Private _recentLog As System.Text.StringBuilder = New System.Text.StringBuilder()

    ''' <summary>排他処理用のロックオブジェクト</summary>
    Private _lockObject As Object = New Object()

    ''' <summary>
    ''' Listenするオブジェクトが出したメッセージを、メンバ変数に格納する。
    ''' </summary>
    ''' <param name="message">メンバ変数に格納する文字列</param>
    Private Sub RecvLog(ByVal message As String)
        ' 画面表示用の変数への代入
        SyncLock _lockObject
            ' 最大長を超えていたら、古いメッセージを削る
            Const maxLogLength As Integer = 2048
            If _recentLog.Length > maxLogLength Then
                _recentLog.Remove(0, maxLogLength \ 3)
            End If
            ' 改行してメッセージを追加する
            _recentLog.AppendLine(message)
        End SyncLock
    End Sub

    Private Sub StartListenButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StartListenButton.Click
        Dim post As PostLog = New PostLog(AddressOf RecvLog)
        _listenObject.Initialize(post, Integer.Parse(PortNoTextBox.Text))
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        ' Listenするオブジェクトに、処理終了するよう要求する
        Dim stopRequest As ListenSample.ReqData = New ListenSample.ReqData
        stopRequest.RequestKind = RequestType.ThreadFinish
        _listenObject.AddRequest(stopRequest)

        ' Listenしているオブジェクトの終了処理が終わるのを待つ
        Dim isStopped As Boolean = _listenObject.stopCompleteEvent.WaitOne(2000)
        If isStopped = False Then
            MessageBox.Show("Listenを強制終了します")
            _listenObject.ForceStop()
        End If
    End Sub

    Private Sub UpdateMessageTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UpdateMessageTimer.Tick
        ' ログの表示を更新
        SyncLock _lockObject
            LogTextBox.Text = _recentLog.ToString()
            LogTextBox.SelectionStart = LogTextBox.Text.Length
            LogTextBox.ScrollToCaret()
        End SyncLock
    End Sub
End Class

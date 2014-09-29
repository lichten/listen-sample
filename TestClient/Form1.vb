Imports System.Text

Imports ListenSample.Utility

Public Class Form1
    ''' <summary>��M���s���I�u�W�F�N�g</summary>
    Private ReadOnly _client As ReceiveThread = New ReceiveThread()

    ''' <summary>��ʏ�ɕ\�����郍�O���b�Z�[�W</summary>
    Private ReadOnly _recentLog As StringBuilder = New StringBuilder()

    ''' <summary>�r�������p�̃��b�N�I�u�W�F�N�g</summary>
    Private ReadOnly _lockObject As Object = New Object()

    ''' <summary>��M���s���I�u�W�F�N�g����A���b�Z�[�W���󂯎�����Ƃ��̏���</summary>
    ''' <param name="message">���b�Z�[�W</param>
    Private Sub RecvLog(ByVal message As String)
        ' ���O�ǉ�����
        Dim logMessage As String = JpnFormat("{0}", message)
        ' ��ʕ\���p�̕ϐ��ւ̑��
        SyncLock _lockObject
            ' �ő咷�𒴂��Ă�����A�Â����b�Z�[�W�����
            Const maxLogLength As Integer = 7500
            If _recentLog.Length > maxLogLength Then
                _recentLog.Remove(0, maxLogLength \ 3)
            End If
            ' ���s���ă��b�Z�[�W��ǉ�����
            _recentLog.AppendLine(logMessage)
        End SyncLock
    End Sub

    Private Sub initButton_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles initButton.Click
        Dim post As PostLog = New PostLog(AddressOf RecvLog)

        Dim isSuccess As Boolean = _client.Initialize(post, IpAddressTextBox.Text, JpnIntegerParse(PortNumberTextBox.Text))
        If isSuccess = False Then
            RecvLog("Initialize�Ăяo���ŃG���[����")
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
        ' ���O�̕\�����X�V
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

        ' TODO �A�N�Z�X������@�l����
        _client.StopCompleteEvent.WaitOne(2000)

    End Sub
End Class

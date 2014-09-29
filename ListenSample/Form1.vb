Public Class Form1
    ''' <summary>Listen����I�u�W�F�N�g</summary>
    Private ReadOnly _listenObject As ListenThread = New ListenThread()

    ''' <summary>��ʏ�ɕ\�����郍�O���b�Z�[�W</summary>
    Private ReadOnly _recentLog As System.Text.StringBuilder = New System.Text.StringBuilder()

    ''' <summary>�r�������p�̃��b�N�I�u�W�F�N�g</summary>
    Private ReadOnly _lockObject As Object = New Object()

    ''' <summary>
    ''' Listen����I�u�W�F�N�g���o�������b�Z�[�W���A�����o�ϐ��Ɋi�[����B
    ''' </summary>
    ''' <param name="message">�����o�ϐ��Ɋi�[���镶����</param>
    Private Sub RecvLog(ByVal message As String)
        ' ��ʕ\���p�̕ϐ��ւ̑��
        SyncLock _lockObject
            ' �ő咷�𒴂��Ă�����A�Â����b�Z�[�W�����
            Const maxLogLength As Integer = 2048
            If _recentLog.Length > maxLogLength Then
                _recentLog.Remove(0, maxLogLength \ 3)
            End If
            ' ���s���ă��b�Z�[�W��ǉ�����
            _recentLog.AppendLine(message)
        End SyncLock
    End Sub

    Private Sub StartListenButton_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles StartListenButton.Click
        Dim post As PostLog = New PostLog(AddressOf RecvLog)
        _listenObject.Initialize(post, Integer.Parse(PortNoTextBox.Text))

        _listenObject.ThreadStartEvent.WaitOne(1000)
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        ' Listen����I�u�W�F�N�g�ɁA�����I������悤�v������
        Dim stopRequest As ReqData = New ReqData
        stopRequest.RequestKind = RequestType.ThreadFinish
        _listenObject.AddRequest(stopRequest)

        ' Listen���Ă���I�u�W�F�N�g�̏I���������I���̂�҂�
        Dim isStopped As Boolean = _listenObject.StopCompleteEvent.WaitOne(2000)
        If isStopped = False Then
            MessageBox.Show("Listen�������I�����܂�")
            _listenObject.ForceStop()
        End If
    End Sub

    Private Sub UpdateMessageTimer_Tick(ByVal sender As System.Object, ByVal e As EventArgs) Handles UpdateMessageTimer.Tick
        ' ���O�̕\�����X�V
        SyncLock _lockObject
            LogTextBox.Text = _recentLog.ToString()
            LogTextBox.SelectionStart = LogTextBox.Text.Length
            LogTextBox.ScrollToCaret()
        End SyncLock
    End Sub
End Class

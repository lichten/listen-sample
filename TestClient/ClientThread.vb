Imports System.Threading
Imports System.Net.Sockets

Imports ListenSample.Utility

''' <summary>
''' 受信を行うオブジェクトへの要求種別
''' </summary>
Public Enum ReqType
    ''' <summary>接続開始</summary>
    ConnectStart
    ''' <summary>接続終了</summary>
    ConnectStop
    ''' <summary>テキスト送信</summary>
    SendText
    ''' <summary>スレッド終了要求</summary>
    ThreadFinish
End Enum

''' <summary>
''' 受信を行うオブジェクトに要求する時使用するデータ
''' </summary>
Public Class ReqData
    ''' <summary>要求種別</summary>
    Private _requestKind As ReqType
    Property RequestKind() As ReqType
        Get
            Return _requestKind
        End Get
        Set(ByVal value As ReqType)
            _requestKind = value
        End Set

    End Property

    ''' <summary>テキストデータ</summary>
    ''' <remarks>テキスト送信コマンドで使われます</remarks>
    Private _textData As String = ""
    Property TextData() As String
        Get
            Return _textData
        End Get
        Set(ByVal value As String)
            _textData = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return JpnFormat("type: {0} textData: {1}", _requestKind, TextData)
    End Function
End Class

''' <summary>ログ出力に使用する</summary>
Public Delegate Sub PostLog(ByVal message As String)

''' <summary>
''' 特定の接続先に接続して、主に受信処理を行うスレッドを管理する。
''' </summary>
Public Class ReceiveThread
    Implements IDisposable

    ''' <summary>スレッド開始したらシグナル状態になるイベント。スレッドの終了確認には使えません。</summary>
    Private ReadOnly _threadStartEvent As ManualResetEvent = New ManualResetEvent(False)

    ''' <summary>スレッドの終了処理が終わったときシグナル状態になるイベント。ソケット接続状態の確認には使えません。</summary>
    Private ReadOnly _stopCompleteEvent As ManualResetEvent = New ManualResetEvent(False)
    Public ReadOnly Property StopCompleteEvent() As ManualResetEvent
        Get
            Return _stopCompleteEvent
        End Get
    End Property

    ''' <summary>Dispose済みか調べる</summary>
    Private _disposed As Boolean

    ''' <summary>
    ''' Dispose処理を行う。
    ''' </summary>
    ''' <param name="disposing">Dispose中ならTrue</param>
    Private Overloads Sub Dispose(ByVal disposing As Boolean)
        If Not _disposed Then
            ' マネージドリソースの解放
            If disposing Then
                _threadStartEvent.Close()
                _stopCompleteEvent.Close()
                _threadFinishStart.Close()
                _reqEvent.Close()

                _currentCommsState.Dispose()
            End If

            ' アンマネージドリソースの解放（今は本クラスには無い）
        End If

        _disposed = True
    End Sub

#Region " IDisposable Support "
    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub
#End Region

    ''' <summary>現在の通信状況</summary>
    Private _currentCommsState As CommsProperty = New CommsProperty()

    ''' <summary>要求を受け付けたときシグナル状態にするイベント</summary>
    Private ReadOnly _reqEvent As ManualResetEvent = New ManualResetEvent(False)

    ''' <summary>スレッド開始関数のパラメータで渡される、ログの出力先</summary>
    Private _logger As PostLog

    ''' <summary>通信で使用するIPアドレス</summary>
    Private _myIP As String

    ''' <summary>通信で使用するポート番号</summary>
    Private _myPort As Integer

    ''' <summary>本クラスが生成する通信スレッド</summary>
    Private _commThread As Thread

    ''' <summary>画面のスレッドが設定するコマンド番号などにアクセスする時に使用するロックオブジェクト</summary>
    Private ReadOnly _lockObject As Object = New Object()

    ''' <summary>スレッド終了処理開始したかどうか（スレッド終了時非シグナルになる）</summary>
    Private ReadOnly _threadFinishStart As ManualResetEvent = New ManualResetEvent(False)

    ''' <summary>レスポンスを待つ時のタイムアウト値</summary>
    Private _timeOutRecvResponseSecond As Integer = 25
    Public Property TimeOutRecvResponseSecond() As Integer
        Get
            SyncLock _lockObject
                Return _timeOutRecvResponseSecond
            End SyncLock
        End Get

        Set(ByVal value As Integer)
            SyncLock _lockObject
                _timeOutRecvResponseSecond = value
            End SyncLock
        End Set
    End Property

    ''' <summary>通信で使用するポート番号</summary>
    Public Property MyPort() As Integer
        Get
            Return _myPort
        End Get
        Set(ByVal value As Integer)
            _myPort = value
        End Set
    End Property

    ''' <summary>通信で使用するIPアドレス</summary>
    Public Property MyIPAddress() As String
        Get
            Return _myIP
        End Get
        Set(ByVal value As String)
            _myIP = value
        End Set
    End Property

    ''' <summary>本クラスの利用側から来た要求のリスト。ReqDataが入る。</summary>
    Private ReadOnly _reqDataList As ArrayList = New ArrayList()

    ''' <summary>
    ''' 画面側から呼び出される、通信スレッドを開始する関数
    ''' </summary>
    ''' <param name="messageDelegate">ログメッセージが送られるデリゲート</param>
    ''' <param name="ip">接続先IPアドレス</param>
    ''' <param name="portno">接続先ポート番号</param>
    ''' <returns>通信スレッドが開始したらTRUE。それ以外はFALSE</returns>
    ''' <remarks>IPアドレス、ポート番号、接続IDについては、ここではダミー値を設定しておいて、接続開始要求時に設定し直す事も出来ます。</remarks>
    Public Function Initialize( _
        ByVal messageDelegate As PostLog, _
        ByVal ip As String, _
        ByVal portno As Integer) As Boolean

        If _threadStartEvent.WaitOne(0, True) Then
            _logger("Initialize 既にスレッド起動しています")
            Return False
        End If

        If messageDelegate Is Nothing Then
            Throw New ArgumentException("messageDelegateがNULLです", "messageDelegate")
        End If

        _logger = messageDelegate
        _currentCommsState.Logger = messageDelegate
        _logger("Initialize 開始")

        ' 接続先の情報
        MyIPAddress = ip
        MyPort = portno

        _logger("Initialize スレッド作成")
        Try
            _commThread = New Thread(New ThreadStart(AddressOf ThreadLoop))
            _commThread.Start()
        Catch ex As OutOfMemoryException
            _logger(JpnFormat("Start スレッド作成/開始時メモリ不足。 Message: {0} StackTrace: {1}", ex.Message, ex.StackTrace))
            Return False
        End Try

        _logger("Initialize 終了")
        Return True
    End Function

    ''' <summary>
    ''' 通信スレッドに、接続開始を要求する。
    ''' </summary>
    ''' <param name="ip">IPアドレス</param>
    ''' <param name="portno">ポート番号</param>
    Public Sub RequestConnectStart(ByVal ip As String, ByVal portno As Integer)
        If String.IsNullOrEmpty(ip) Then
            Throw New ArgumentException("ipはNothingで渡せません", "ip")
        End If

        Dim isSuccess As Boolean = CheckPreconditionReqest()
        If isSuccess = False Then
            _logger("requestConnectStart で事前条件チェック失敗")
        End If

        MyIPAddress = ip

        MyPort = portno
        
        ' 要求リストに、終了を追加する
        Dim req As ReqData = New ReqData
        req.RequestKind = ReqType.ConnectStart
        AddRequest(req)
    End Sub

    ''' <summary>
    ''' 通信スレッドに、接続終了を要求する。
    ''' </summary>
    Public Sub RequestConnectStop()
        Dim isSuccess As Boolean = CheckPreconditionReqest()
        If isSuccess = False Then
            _logger("RequestConnectStop で事前条件チェック失敗")
        End If

        ' 要求リストに、終了を追加する
        Dim req As ReqData = New ReqData
        req.RequestKind = ReqType.ConnectStop
        AddRequest(req)
    End Sub

    ''' <summary>
    ''' 通信スレッドに終了を要求する
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RequestStop()
        Dim isSuccess As Boolean = CheckPreconditionReqest()
        If isSuccess = False Then
            If Not _logger Is Nothing Then
                _logger("RequestStop で事前条件チェック失敗")
            End If
        End If

        ' 要求リストに、スレッド終了を追加する
        Dim req As ReqData = New ReqData
        req.RequestKind = ReqType.ThreadFinish
        AddRequest(req)
    End Sub

    ''' <summary>
    ''' 通信スレッドを強制終了する
    ''' </summary>
    Public Sub ForceStop()
        Dim isSuccess As Boolean = CheckPreconditionReqest()
        If isSuccess = False Then
            _logger("executeOneShotReading で事前条件チェック失敗")
        End If

        _commThread.Abort()
    End Sub

    ''' <summary>
    ''' 要求リストに、要求データを追加する処理
    ''' </summary>
    ''' <param name="req">追加する要求データ</param>
    Public Sub AddRequest(ByVal req As ReqData)
        SyncLock _lockObject
            _reqDataList.Insert(0, req) ' 先頭に追加する（FILOで処理する）
            _reqEvent.Set()
        End SyncLock
    End Sub

    ''' <summary>
    ''' 通信スレッドのループ関数
    ''' </summary>
    Private Sub ThreadLoop()
        _logger("ThreadLoop 開始")

        _threadStartEvent.Set()

        Do
            ' 要求をチェックする
            Do
                Dim req As ReqData = Nothing
                Dim messages As ArrayList = New ArrayList() ' ロック中はコールバックするとデッドロックの可能性があるので、変数に溜めておく
                SyncLock _lockObject
                    If _reqDataList.Count > 0 Then
                        Dim temp As Object = _reqDataList(_reqDataList.Count - 1)
                        req = CType(temp, ReqData)
                        _reqDataList.RemoveAt(_reqDataList.Count - 1)
                    End If

                    If req Is Nothing Then
                        messages.Add("ThreadLoop 残りの要求が無くなったので、要求イベントは非シグナル状態に移行")
                        _reqEvent.Reset()
                        Exit Do
                    End If
                End SyncLock

                ' 変数に溜めておいたログメッセージを出力する
                For i As Integer = 0 To messages.Count - 1
                    Dim message As String = CType(messages(i), String)
                    _logger(message)
                Next

                _logger(JpnFormat("ThreadLoop 取得した要求: {0}", req))
                If req.RequestKind = ReqType.SendText Then
                    _currentCommsState.SendTextData(req.TextData)
                End If
                ' TODO 終了イベントが来たら対応する
            Loop

            ' 終了イベントがシグナル状態になっていたら、ループを抜ける
            Dim isFinishStart As Boolean = _threadFinishStart.WaitOne(0, True)
            If isFinishStart Then
                Exit Do
            End If

            If _currentCommsState.IsConnected Then
                ' 接続済みなら、受信可能か調べる
                Dim recvList As ArrayList = New ArrayList()
                recvList.Add(_currentCommsState.WorkSocket)
                Try
                    Socket.Select(recvList, Nothing, Nothing, 1000)
                Catch ex As SocketException
                    _logger(JpnFormat("SelectでSocketException発生。 ErrorCode: {0} Message: {1} StackTrace: {2}", _
                        ex.ErrorCode, ex.Message, ex.StackTrace))
                    ' 切断処理
                End Try

                ' 受信可能なら、受信する
                For i As Integer = 0 To recvList.Count - 1
                    Dim readablesocket As Socket = CType(recvList(i), Socket)
                    If _currentCommsState.WorkSocket.Equals(readablesocket) Then
                        ExecuteRecv(_currentCommsState)
                    End If
                Next

            ElseIf _currentCommsState.IsInConnecting Then
                ' 接続中なら何もしない
            Else
                ' 未接続なら、接続開始する
                _currentCommsState.StartConnectingProcess(MyIPAddress, MyPort)
 
            End If
        Loop

        ' スレッド終了完了イベントをシグナル状態にする
        _stopCompleteEvent.Set()
        ' スレッド終了するので、スレッド終了開始イベントを非シグナル状態にする
        _threadFinishStart.Reset()

        _logger("ThreadLoop 終了")
    End Sub


    ''' <summary>
    ''' 本クラスの利用側から要求を受けた時、初期化済みかどうか、スレッド終了済みでないかどうか調べる。
    ''' </summary>
    ''' <returns>要求を受けられる状態ならTRUE</returns>
    Private Function CheckPreconditionReqest() As Boolean
        If _threadStartEvent.WaitOne(0, True) = False Then
            If Not _logger Is Nothing Then
                _logger(JpnFormat("CheckPreconditionReqest スレッド開始していない。"))
            End If
            Return False
        End If

        If _stopCompleteEvent.WaitOne(0, True) Then
            If Not _logger Is Nothing Then
                _logger(JpnFormat("CheckPreconditionReqest 既にスレッド終了している。"))
            End If
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Receive実行する
    ''' </summary>
    ''' <param name="state">受信になっている接続状態</param>
    ''' <remarks></remarks>
    Private Sub ExecuteRecv(ByRef state As CommsProperty)
        ' 受信する
        Dim recvBytes(1024) As Byte
        Dim recvSize As Integer
        Try
            recvSize = state.WorkSocket.Receive(recvBytes, recvBytes.Length, SocketFlags.None)
            If recvSize = 0 Then
                _logger(JpnFormat("切断を検知。"))
                state.Disconnect()
            Else
                _logger(JpnFormat("受信: {0}", HexString(recvBytes, recvSize)))
                Array.Resize(recvBytes, recvSize)
                state.AddReceiveData(recvBytes)
            End If
        Catch ex As SocketException
            _logger(JpnFormat("Receive処理でSocketException発生。SocketErrorCode: {0} Message: {1} StackTrace: {2}", _
                             ex.SocketErrorCode, ex.Message, ex.StackTrace))
            state.Disconnect()
        Catch ex As ObjectDisposedException
            _logger(JpnFormat("Receive処理でObjectDisposedException発生。Message: {0} StackTrace: {1}", _
                             ex.Message, ex.StackTrace))
            state.Disconnect()
        End Try
        ' TODO InvalidOperationExceptionもキャッチ
    End Sub

End Class

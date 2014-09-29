Imports System.Net.Sockets
Imports System.Threading

Imports ListenSample.Utility

''' <summary>
''' Listen中のオブジェクトに対して行う要求の種別
''' </summary>
Public Enum RequestType
    ''' <summary>スレッド終了要求</summary>
    ThreadFinish
End Enum

''' <summary>
''' Listen中のオブジェクトに対して要求する時使用するクラス
''' </summary>
Public Class ReqData
    ''' <summary>要求種別</summary>
    Private _type As RequestType
    Public Property RequestKind() As RequestType
        Get
            Return _type
        End Get
        Set(ByVal value As RequestType)
            _type = value
        End Set
    End Property

    ''' <summary>ログ出力用の文字列を返す</summary>
    Public Overrides Function ToString() As String
        Return JpnFormat("type: {0}", _type)
    End Function
End Class

''' <summary>ログ出力に使用する</summary>
Public Delegate Sub PostLog(ByVal message As String)

''' <summary>
''' Listenするスレッドを管理する
''' </summary>
Public NotInheritable Class ListenThread
    Implements IDisposable

    ''' <summary>スレッド開始したらシグナル状態になるイベント。スレッドの終了確認には使えません。</summary>
    Private ReadOnly _threadStartEvent As ManualResetEvent = New ManualResetEvent(False)
    ReadOnly Property ThreadStartEvent() As ManualResetEvent
        Get
            Return _threadStartEvent
        End Get
    End Property

    ''' <summary>スレッドの終了処理が終わったときシグナル状態になるイベント。ソケット接続状態の確認には使えません。</summary>
    Private ReadOnly _stopCompleteEvent As ManualResetEvent = New ManualResetEvent(False)
    ReadOnly Property StopCompleteEvent() As ManualResetEvent
        Get
            Return _stopCompleteEvent
        End Get
    End Property

    ''' <summary>要求を受け付けたときシグナル状態にするイベント</summary>
    Private ReadOnly _requestEvent As ManualResetEvent = New ManualResetEvent(False)

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
                _requestEvent.Close()
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

    ''' <summary>スレッド開始関数のパラメータで渡される、ログの出力先</summary>
    Private _logger As PostLog

    ''' <summary>通信で使用するポート番号</summary>
    Private _myPort As Integer
    Private Property Port() As Integer
        Get
            SyncLock _lockObject
                Return _myPort
            End SyncLock
        End Get
        Set(ByVal value As Integer)
            SyncLock _lockObject
                _myPort = value
            End SyncLock
        End Set
    End Property

    ''' <summary>本クラスが生成する通信スレッド</summary>
    Private _commThread As Thread

    ''' <summary>画面のスレッドが設定するコマンド番号などにアクセスする時に使用するロックオブジェクト</summary>
    Private ReadOnly _lockObject As Object = New Object()

    ''' <summary>本クラスの利用側から来た要求のリスト</summary>
    Private ReadOnly _reqDataList As List(Of ReqData) = New List(Of ReqData)

    ''' <summary>
    ''' 画面側から呼び出される、通信スレッドを開始する関数
    ''' </summary>
    ''' <param name="logger">ログメッセージが送られるデリゲート</param>
    ''' <param name="portNumber">接続受付ポート番号</param>
    ''' <returns>通信スレッドが開始したらTRUE。それ以外はFALSE</returns>
    Public Function Initialize(ByVal logger As PostLog, ByVal portNumber As Integer) As Boolean
        If logger Is Nothing Then
            Throw New ArgumentException("loggerにNothingが指定されました。", "logger")
        End If

        If _threadStartEvent.WaitOne(0, True) Then
            _logger("Initialize 既にスレッド起動しています")
            Return False
        End If

        _logger = logger
        _logger("Initialize 開始")

        Port = portNumber

        _logger("Initialize スレッド作成開始")
        Try
            _commThread = New Thread(New ThreadStart(AddressOf ThreadLoop))
            _commThread.Start()
        Catch ex As OutOfMemoryException
            _logger(JpnFormat("Initialize スレッド作成/開始時メモリ不足。 Message: {0} StackTrace: {1}", ex.Message, ex.StackTrace))
            Return False
        End Try

        _logger("Initialize 終了")
        Return True
    End Function

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
    ''' 要求リストに、要求データを追加する
    ''' </summary>
    ''' <param name="req">追加する要求データ</param>
    ''' <remarks>UIスレッドなどから呼び出されることを想定しています。</remarks>
    Public Sub AddRequest(ByVal req As ReqData)
        SyncLock _lockObject
            _reqDataList.Insert(0, req) ' 先頭に追加する（FILOで処理する）
            _requestEvent.Set() ' 通信スレッドに通知するため、シグナル状態にする
        End SyncLock
    End Sub

    ''' <summary>
    ''' 通信スレッドのループ関数
    ''' </summary>
    Private Sub ThreadLoop()
        _logger("ThreadLoop 開始")

        _threadStartEvent.Set()

        ' Listen開始
        Dim listenSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        Try
            Dim hostIp As Net.IPAddress = Net.IPAddress.Any
            Dim ep As New Net.IPEndPoint(hostIp, Port)
            listenSocket.Bind(ep)
            listenSocket.Listen(1)
        Catch ex As SocketException
            _logger(JpnFormat("Listen開始に失敗。 ErrorCode: {0} Message: {1} StackTrace: {2}", ex.SocketErrorCode, ex.Message, ex.StackTrace))
            GoTo ErrorListen
        End Try

        Dim connectingSock As Socket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) ' 接続中のソケット
        Dim receiveBuffer As List(Of Byte) = New List(Of Byte) ' 接続中のソケットで受信したデータ
        Do
            ' 他スレッドから要求が来ていたら処理を行う
            If _requestEvent.WaitOne(0) Then

                ' 要求リストの末尾のデータを取得する（FILOで処理する）
                Dim req As ReqData = Nothing
                SyncLock _lockObject
                    Dim dataCount As Integer = _reqDataList.Count()
                    If 0 < dataCount Then
                        req = _reqDataList(dataCount - 1)
                        _reqDataList.RemoveAt(dataCount - 1)
                    End If

                    ' 最後の要求だったら、イベントを非シグナルにする
                    If dataCount = 1 Then
                        _requestEvent.Reset()
                    End If
                End SyncLock

                ' 要求を取得できたら処理をする
                If Not req Is Nothing Then
                    If req.RequestKind = RequestType.ThreadFinish Then
                        receiveBuffer.Clear()
                        If connectingSock.Connected Then
                            connectingSock.Shutdown(SocketShutdown.Both)
                            connectingSock.Close()
                        End If
                        Exit Do
                    Else
                        _logger(JpnFormat("サポート外の要求を受信 req: {0}", req.ToString()))
                    End If
                End If
            End If

            ' Listen中のソケットと、接続中のソケットのリストを作成する。
            Dim recvList As ArrayList = New ArrayList()
            recvList.Add(listenSocket)
            If connectingSock.Connected Then
                recvList.Add(connectingSock)
            End If

            ' ソケットが受信可能になるまで待つ
            Try
                Socket.Select(recvList, Nothing, Nothing, 1000)
            Catch ex As SocketException
                _logger(JpnFormat("SelectでSocketException発生。 SocketErrorCode: {0} Message: {1} StackTrace: {2}", _
                    ex.SocketErrorCode, ex.Message, ex.StackTrace))
                Exit Do
            End Try

            ' 受信可能になったソケットに対して処理を行う
            For i As Integer = 0 To recvList.Count - 1
                Dim readableSocket As Socket = CType(recvList(i), Socket)

                ' Listen中のソケットが、Accept可能になった場合
                If listenSocket.Equals(readableSocket) Then
                    ExecuteAccept(readableSocket, connectingSock, receiveBuffer)
                End If

                ' 接続中のソケットが、Recv可能になった場合
                If Not connectingSock Is Nothing AndAlso connectingSock.Equals(readableSocket) Then
                    ExecuteRecv(connectingSock, receiveBuffer)
                End If
            Next
        Loop

        _logger("ThreadLoop 終了")
        _stopCompleteEvent.Set()
        Return

ErrorListen:
        _logger("ThreadLoop Listenエラー終了")
        _stopCompleteEvent.Set()
        Return
    End Sub

    ''' <summary>
    ''' Receive実行する
    ''' </summary>
    ''' <param name="readableSock">受信可能なソケット</param>
    ''' <param name="receiveBuffer">readableSockから受信したデータを入れる変数</param>
    ''' <remarks></remarks>
    Private Sub ExecuteRecv(ByRef readableSock As Socket, ByRef receiveBuffer As List(Of Byte))
        ' 受信する
        Dim recvBytes(1024) As Byte
        Dim recvSize As Integer
        Try
            recvSize = readableSock.Receive(recvBytes, recvBytes.Length, SocketFlags.None)
            If recvSize = 0 Then
                _logger(JpnFormat("切断を検知。"))
                ' ソケットを破棄する
                receiveBuffer.Clear()
                readableSock.Shutdown(SocketShutdown.Both)
                readableSock.Close()
            Else
                _logger(JpnFormat("受信: {0}", HexString(recvBytes, recvSize)))
                Array.Resize(recvBytes, recvSize)
                receiveBuffer.AddRange(recvBytes)
            End If
        Catch ex As SocketException
            _logger(JpnFormat("Receive処理でSocketException発生。SocketErrorCode: {0} Message: {1} StackTrace: {2}", _
                             ex.SocketErrorCode, ex.Message, ex.StackTrace))
            ' ソケットを破棄する
            receiveBuffer.Clear()
            readableSock.Shutdown(SocketShutdown.Both)
            readableSock.Close()
        Catch ex As ObjectDisposedException
            _logger(JpnFormat("Receive処理でObjectDisposedException発生。Message: {0} StackTrace: {1}", _
                             ex.Message, ex.StackTrace))
        End Try
    End Sub

    ''' <summary>
    ''' Accept実行する。
    ''' </summary>
    ''' <param name="acceptableSocket">Accept可能になったソケットを指定する</param>
    ''' <param name="connectingSock">Accept実行して、接続済みになったソケットを参照する</param>
    ''' <param name="receiveBuffer">connectionSockで受信したデータを入れている変数</param>
    Private Sub ExecuteAccept(ByVal acceptableSocket As Socket, ByRef connectingSock As Socket, ByRef receiveBuffer As List(Of Byte))
        ' Accept実行する
        Dim acceptedSock As Socket
        Try
            acceptedSock = acceptableSocket.Accept()
        Catch ex As SocketException
            _logger(JpnFormat("AcceptでSocketException発生。 ErrorCode: {0} Message: {1} StackTrace{2}", _
                ex.SocketErrorCode, ex.Message, ex.StackTrace))
            Return
        End Try
        _logger(JpnFormat("Accept終了。 RemoteEndPoint: {0}", acceptedSock.RemoteEndPoint))

        ' 2回目の接続なら、前のソケットは破棄する
        If connectingSock.Connected Then
            _logger(JpnFormat("2回目の接続なので、前回の接続を破棄。 Before RemoteEndPoint: {0}", connectingSock.RemoteEndPoint))
            ' ソケットを破棄する
            receiveBuffer.Clear()
            connectingSock.Shutdown(SocketShutdown.Both)
            connectingSock.Close()
        End If

        connectingSock = acceptedSock
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

End Class

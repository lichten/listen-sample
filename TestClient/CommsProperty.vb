Imports System.Net.Sockets

Imports ListenSample.Utility

''' <summary>
''' ソケットの非同期関数に渡すために、通信の情報をまとめて入れるクラス
''' </summary>
''' <remarks>非同期に呼び出されるため、スレッドセーフなクラスとして作成</remarks>
Public NotInheritable Class CommsProperty
    Implements IDisposable

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
                If Not WorkSocket Is Nothing Then
                    _workSocket.Close()
                End If
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

    ''' <summary>接続処理中に立つフラグ</summary>
    Private _isInConnecting As Boolean
    Property IsInConnecting() As Boolean
        Get
            SyncLock _lockObject
                Return _isInConnecting
            End SyncLock
        End Get
        Set(ByVal value As Boolean)
            SyncLock _lockObject
                _isInConnecting = value
            End SyncLock
        End Set
    End Property

    ''' <summary>接続が確立している時立つフラグ</summary>
    Private _isConnected As Boolean
    Property IsConnected() As Boolean
        Get
            SyncLock _lockObject
                Return _isConnected
            End SyncLock
        End Get
        Set(ByVal value As Boolean)
            SyncLock _lockObject
                _isConnected = value
            End SyncLock
        End Set
    End Property

    ''' <summary>
    ''' 接続を開始する。
    ''' </summary>
    ''' <param name="ip">IPアドレス</param>
    ''' <param name="port">ポート番号</param>
    ''' <remarks>接続完了を確認するには、IsConnectedを参照します。</remarks>
    Public Sub StartConnectingProcess(ByVal ip As String, ByVal port As Integer)
        _logger(JpnFormat("socketConnectTrans 開始 IP: {0} port: {1}", ip, port))

        ' ソケットを作り直す
        _workSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        Try
            ' 接続開始する
            _workSocket.BeginConnect(ip, port, New AsyncCallback(AddressOf FinishConnectingProcess), Me)
            _isInConnecting = True
            _isConnected = False
        Catch ex As SocketException
            _logger(JpnFormat("socketConnectTrans BeginConnectがSocketException例外を送信 Code: {0} Message: {1} StackTrace: {2}", _
            ex.ErrorCode, ex.Message, ex.StackTrace))
            Disconnect()
        End Try
    End Sub

    ''' <summary>
    ''' 接続処理を完了する。Socket.BeginConnect関数に登録して利用する。
    ''' </summary>
    ''' <param name="ar"></param>
    Public Sub FinishConnectingProcess(ByVal ar As IAsyncResult)
        Try
            _workSocket.EndConnect(ar)

            _isInConnecting = False
            _isConnected = True
        Catch ex As SocketException
            _logger(JpnFormat("ConnectCallback EndConnectでSocketException発生。 ErrorCode: {0} Message: {1} StackTrace: {2}", _
                ex.ErrorCode, ex.Message, ex.StackTrace))
            Disconnect()
            Return
        Catch ex As ObjectDisposedException
            _logger(JpnFormat("ConnectCallback EndConnectでObjectDisposedException発生。 Message: {0} StackTrace: {1}", _
                ex.Message, ex.StackTrace))
            Disconnect()
            Return
        End Try

    End Sub

    ''' <summary>
    ''' 切断する
    ''' </summary>
    Public Sub Disconnect()
        Try
            ' 受信バッファのデータを消す
            ClearReceiveData()

            ' ソケットクローズする
            If Not _workSocket Is Nothing Then
                _workSocket.Shutdown(SocketShutdown.Both)
                _workSocket.Close()
            End If
        Catch ex As SocketException
            _logger(JpnFormat("SocketException発生。SocketErrorCode: {0} Message: {1} StackTrace: {2}", _
                             ex.SocketErrorCode, ex.Message, ex.StackTrace))
        Catch ex As ObjectDisposedException
            _logger(JpnFormat("ObjectDisposedException発生。Message: {0} StackTrace: {1}", _
                             ex.Message, ex.StackTrace))
        Finally
            _isInConnecting = False
            _isConnected = False
        End Try
    End Sub

    ''' <summary>
    ''' 受信データを追加する。
    ''' </summary>
    ''' <param name="data">追加するデータ</param>
    Public Sub AddReceiveData(ByVal data As Byte())
        _receiveBuffer.AddRange(data)
    End Sub

    ''' <summary>
    ''' テキストデータを送信する。
    ''' </summary>
    ''' <param name="data">受信したデータ</param>
    Public Sub SendTextData(ByVal data As String)
        ' TODO 接続中でなければ無視する

        Dim bytesData As Byte() = New Byte() {}
        StringTelegramToBytesTelegram(data, bytesData)
        _workSocket.Send(bytesData)
    End Sub

    Private Sub ClearReceiveData()
        ' 受信バッファにデータが残っていたら、ログに内容を残す
        If _receiveBuffer.Count > 0 Then
            _logger(JpnFormat("socketCloseTrans 切断処理を行うとき、受信済みのデータが残っていた。 data: {0}", _
                HexString(_receiveBuffer.ToArray(), _receiveBuffer.Count())))
        End If

        _receiveBuffer.Clear()
    End Sub

    ''' <summary>処理対象のソケット</summary>
    Private _workSocket As Socket
    Public ReadOnly Property WorkSocket() As Socket
        Get
            Return _workSocket
        End Get
    End Property

    ''' <summary>受信データの入れ先</summary>
    Private ReadOnly _receiveBuffer As List(Of Byte) = New List(Of Byte)

    ''' <summary>ログ出力用</summary>
    Private _logger As PostLog
    Property Logger() As PostLog
        Get
            Return _logger
        End Get
        Set(ByVal value As PostLog)
            _logger = value
        End Set
    End Property

    ''' <summary>排他処理用のオブジェクト</summary>
    Private ReadOnly _lockObject As Object = New Object()
End Class

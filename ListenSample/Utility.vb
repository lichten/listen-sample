Imports System.Text

''' <summary>
''' よく使うShared関数をまとめたクラス。
''' </summary>
Public NotInheritable Class Utility
    ''' <summary>
    ''' インスタンス化して呼び出す意味が無いクラスなので、コンストラクタをPrivateで宣言。
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' バイト配列の内容を、16進数文字列にして返す
    ''' </summary>
    ''' <param name="targetData">文字列の基になるバイト配列</param>
    ''' <returns>1バイト2文字の、16進数文字列</returns>
    Public Shared Function HexString(ByVal targetData() As Byte, ByVal length As Integer) As String
        If targetData Is Nothing Then
            Throw New ArgumentException("targetDataにNothingが渡されました。", "targetData")
        End If

        Dim returnString As System.Text.StringBuilder = New System.Text.StringBuilder(2048)

        For i As Integer = 0 To length - 1
            returnString.Append(JpnFormat("{0:X2}", targetData(i)))

            If ((i + 1) Mod 2) = 0 Then ' 2バイト毎にスペースを入れる
                returnString.Append(" ")
            End If
        Next

        Return returnString.ToString()
    End Function

    ''' <summary>
    ''' "ja-JP"カルチャで、String.Formatを呼び出す
    ''' </summary>
    ''' <param name="format">String.Formatの1つ目の引数</param>
    ''' <param name="args">String.Formatの2つ目以降の引数</param>
    ''' <returns>String.Formatの返り値</returns>
    Public Shared Function JpnFormat(ByVal format As String, ByVal ParamArray args() As Object) As String
        Return String.Format(New Globalization.CultureInfo("ja-JP"), format, args)
    End Function

    ''' <summary>
    ''' "ja-JP"カルチャで、Integer.Parseを呼び出す
    ''' </summary>
    ''' <param name="target">Integer.Parseに渡す引数</param>
    ''' <returns>Integer.Parseの返り値</returns>
    Public Shared Function JpnIntegerParse(ByVal target As String) As Integer
        Return Integer.Parse(target, New Globalization.CultureInfo("ja-JP"))
    End Function

    ''' <summary>
    ''' ASCII文字列の電文の、バイト配列版を生成する
    ''' </summary>
    ''' <param name="textTelegram">ASCII文字列</param>
    ''' <param name="bytesTelegram">データの生成先。配列長はASCII文字列のバイト数と同じになる。</param>
    Public Shared Sub StringTelegramToBytesTelegram(ByVal textTelegram As String, ByRef bytesTelegram() As Byte)
        Dim byteLength As Integer = Encoding.ASCII.GetByteCount(textTelegram)
        Array.Resize(bytesTelegram, byteLength)
        Buffer.BlockCopy(Encoding.ASCII.GetBytes(textTelegram), 0, bytesTelegram, 0, bytesTelegram.Length)
    End Sub

End Class

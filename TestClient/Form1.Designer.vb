<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナで必要です。
    'Windows フォーム デザイナを使用して変更できます。  
    'コード エディタを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.SendTextButton = New System.Windows.Forms.Button
        Me.SendTextLabel = New System.Windows.Forms.Label
        Me.SendTextTextBox = New System.Windows.Forms.TextBox
        Me.ConnectStopButton = New System.Windows.Forms.Button
        Me.ConnectStartButton = New System.Windows.Forms.Button
        Me.initButton = New System.Windows.Forms.Button
        Me.LogTextBox = New System.Windows.Forms.TextBox
        Me.ConnectionInfomationGroupBox = New System.Windows.Forms.GroupBox
        Me.IpAddressLabel = New System.Windows.Forms.Label
        Me.IpAddressTextBox = New System.Windows.Forms.TextBox
        Me.PortNoLabel = New System.Windows.Forms.Label
        Me.PortNumberTextBox = New System.Windows.Forms.TextBox
        Me.LogUpdateTimer = New System.Windows.Forms.Timer(Me.components)
        Me.ConnectionInfomationGroupBox.SuspendLayout()
        Me.SuspendLayout()
        '
        'SendTextButton
        '
        Me.SendTextButton.Location = New System.Drawing.Point(363, 422)
        Me.SendTextButton.Name = "SendTextButton"
        Me.SendTextButton.Size = New System.Drawing.Size(75, 23)
        Me.SendTextButton.TabIndex = 62
        Me.SendTextButton.Text = "テキスト送信"
        Me.SendTextButton.UseVisualStyleBackColor = True
        '
        'SendTextLabel
        '
        Me.SendTextLabel.AutoSize = True
        Me.SendTextLabel.Location = New System.Drawing.Point(-161, 312)
        Me.SendTextLabel.Name = "SendTextLabel"
        Me.SendTextLabel.Size = New System.Drawing.Size(65, 12)
        Me.SendTextLabel.TabIndex = 61
        Me.SendTextLabel.Text = "送信テキスト"
        '
        'SendTextTextBox
        '
        Me.SendTextTextBox.Location = New System.Drawing.Point(165, 422)
        Me.SendTextTextBox.Name = "SendTextTextBox"
        Me.SendTextTextBox.Size = New System.Drawing.Size(192, 19)
        Me.SendTextTextBox.TabIndex = 60
        Me.SendTextTextBox.Text = "ABC123!""#"
        '
        'ConnectStopButton
        '
        Me.ConnectStopButton.Location = New System.Drawing.Point(69, 269)
        Me.ConnectStopButton.Name = "ConnectStopButton"
        Me.ConnectStopButton.Size = New System.Drawing.Size(250, 45)
        Me.ConnectStopButton.TabIndex = 59
        Me.ConnectStopButton.Text = "接続終了"
        Me.ConnectStopButton.UseVisualStyleBackColor = True
        '
        'ConnectStartButton
        '
        Me.ConnectStartButton.Location = New System.Drawing.Point(69, 206)
        Me.ConnectStartButton.Name = "ConnectStartButton"
        Me.ConnectStartButton.Size = New System.Drawing.Size(250, 45)
        Me.ConnectStartButton.TabIndex = 58
        Me.ConnectStartButton.Text = "接続開始"
        Me.ConnectStartButton.UseVisualStyleBackColor = True
        '
        'initButton
        '
        Me.initButton.Location = New System.Drawing.Point(69, 144)
        Me.initButton.Name = "initButton"
        Me.initButton.Size = New System.Drawing.Size(250, 45)
        Me.initButton.TabIndex = 57
        Me.initButton.Text = "初期化"
        Me.initButton.UseVisualStyleBackColor = True
        '
        'LogTextBox
        '
        Me.LogTextBox.Location = New System.Drawing.Point(334, 53)
        Me.LogTextBox.Multiline = True
        Me.LogTextBox.Name = "LogTextBox"
        Me.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.LogTextBox.Size = New System.Drawing.Size(399, 333)
        Me.LogTextBox.TabIndex = 56
        '
        'ConnectionInfomationGroupBox
        '
        Me.ConnectionInfomationGroupBox.Controls.Add(Me.IpAddressLabel)
        Me.ConnectionInfomationGroupBox.Controls.Add(Me.IpAddressTextBox)
        Me.ConnectionInfomationGroupBox.Controls.Add(Me.PortNoLabel)
        Me.ConnectionInfomationGroupBox.Controls.Add(Me.PortNumberTextBox)
        Me.ConnectionInfomationGroupBox.Location = New System.Drawing.Point(69, 53)
        Me.ConnectionInfomationGroupBox.Name = "ConnectionInfomationGroupBox"
        Me.ConnectionInfomationGroupBox.Size = New System.Drawing.Size(250, 76)
        Me.ConnectionInfomationGroupBox.TabIndex = 55
        Me.ConnectionInfomationGroupBox.TabStop = False
        Me.ConnectionInfomationGroupBox.Text = "接続先設定"
        '
        'IpAddressLabel
        '
        Me.IpAddressLabel.AutoSize = True
        Me.IpAddressLabel.Location = New System.Drawing.Point(6, 21)
        Me.IpAddressLabel.Name = "IpAddressLabel"
        Me.IpAddressLabel.Size = New System.Drawing.Size(51, 12)
        Me.IpAddressLabel.TabIndex = 0
        Me.IpAddressLabel.Text = "IPアドレス"
        '
        'IpAddressTextBox
        '
        Me.IpAddressTextBox.Location = New System.Drawing.Point(100, 18)
        Me.IpAddressTextBox.Name = "IpAddressTextBox"
        Me.IpAddressTextBox.Size = New System.Drawing.Size(144, 19)
        Me.IpAddressTextBox.TabIndex = 2
        Me.IpAddressTextBox.Text = "127.0.0.1"
        '
        'PortNoLabel
        '
        Me.PortNoLabel.AutoSize = True
        Me.PortNoLabel.Location = New System.Drawing.Point(6, 47)
        Me.PortNoLabel.Name = "PortNoLabel"
        Me.PortNoLabel.Size = New System.Drawing.Size(57, 12)
        Me.PortNoLabel.TabIndex = 1
        Me.PortNoLabel.Text = "ポート番号"
        '
        'PortNumberTextBox
        '
        Me.PortNumberTextBox.Location = New System.Drawing.Point(100, 44)
        Me.PortNumberTextBox.Name = "PortNumberTextBox"
        Me.PortNumberTextBox.Size = New System.Drawing.Size(144, 19)
        Me.PortNumberTextBox.TabIndex = 3
        Me.PortNumberTextBox.Text = "14576"
        '
        'LogUpdateTimer
        '
        Me.LogUpdateTimer.Enabled = True
        Me.LogUpdateTimer.Interval = 1000
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1057, 539)
        Me.Controls.Add(Me.SendTextButton)
        Me.Controls.Add(Me.SendTextLabel)
        Me.Controls.Add(Me.SendTextTextBox)
        Me.Controls.Add(Me.ConnectStopButton)
        Me.Controls.Add(Me.ConnectStartButton)
        Me.Controls.Add(Me.initButton)
        Me.Controls.Add(Me.LogTextBox)
        Me.Controls.Add(Me.ConnectionInfomationGroupBox)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ConnectionInfomationGroupBox.ResumeLayout(False)
        Me.ConnectionInfomationGroupBox.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents SendTextButton As System.Windows.Forms.Button
    Friend WithEvents SendTextLabel As System.Windows.Forms.Label
    Friend WithEvents SendTextTextBox As System.Windows.Forms.TextBox
    Friend WithEvents ConnectStopButton As System.Windows.Forms.Button
    Friend WithEvents ConnectStartButton As System.Windows.Forms.Button
    Friend WithEvents initButton As System.Windows.Forms.Button
    Friend WithEvents LogTextBox As System.Windows.Forms.TextBox
    Friend WithEvents ConnectionInfomationGroupBox As System.Windows.Forms.GroupBox
    Friend WithEvents IpAddressLabel As System.Windows.Forms.Label
    Friend WithEvents IpAddressTextBox As System.Windows.Forms.TextBox
    Friend WithEvents PortNoLabel As System.Windows.Forms.Label
    Friend WithEvents PortNumberTextBox As System.Windows.Forms.TextBox
    Friend WithEvents LogUpdateTimer As System.Windows.Forms.Timer

End Class

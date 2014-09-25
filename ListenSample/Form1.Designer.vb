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
        Me.StartListenButton = New System.Windows.Forms.Button
        Me.PortNoTextBox = New System.Windows.Forms.TextBox
        Me.PortNoLabel = New System.Windows.Forms.Label
        Me.LogTextBox = New System.Windows.Forms.TextBox
        Me.UpdateMessageTimer = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'StartListenButton
        '
        Me.StartListenButton.Location = New System.Drawing.Point(14, 31)
        Me.StartListenButton.Name = "StartListenButton"
        Me.StartListenButton.Size = New System.Drawing.Size(75, 23)
        Me.StartListenButton.TabIndex = 0
        Me.StartListenButton.Text = "Listen開始"
        Me.StartListenButton.UseVisualStyleBackColor = True
        '
        'PortNoTextBox
        '
        Me.PortNoTextBox.Location = New System.Drawing.Point(75, 6)
        Me.PortNoTextBox.Name = "PortNoTextBox"
        Me.PortNoTextBox.Size = New System.Drawing.Size(65, 19)
        Me.PortNoTextBox.TabIndex = 1
        Me.PortNoTextBox.Text = "14576"
        '
        'PortNoLabel
        '
        Me.PortNoLabel.AutoSize = True
        Me.PortNoLabel.Location = New System.Drawing.Point(12, 9)
        Me.PortNoLabel.Name = "PortNoLabel"
        Me.PortNoLabel.Size = New System.Drawing.Size(57, 12)
        Me.PortNoLabel.TabIndex = 2
        Me.PortNoLabel.Text = "ポート番号"
        '
        'LogTextBox
        '
        Me.LogTextBox.Location = New System.Drawing.Point(14, 60)
        Me.LogTextBox.Multiline = True
        Me.LogTextBox.Name = "LogTextBox"
        Me.LogTextBox.Size = New System.Drawing.Size(266, 201)
        Me.LogTextBox.TabIndex = 3
        '
        'UpdateMessageTimer
        '
        Me.UpdateMessageTimer.Enabled = True
        Me.UpdateMessageTimer.Interval = 500
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.Controls.Add(Me.LogTextBox)
        Me.Controls.Add(Me.PortNoLabel)
        Me.Controls.Add(Me.PortNoTextBox)
        Me.Controls.Add(Me.StartListenButton)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents StartListenButton As System.Windows.Forms.Button
    Friend WithEvents PortNoTextBox As System.Windows.Forms.TextBox
    Friend WithEvents PortNoLabel As System.Windows.Forms.Label
    Friend WithEvents LogTextBox As System.Windows.Forms.TextBox
    Friend WithEvents UpdateMessageTimer As System.Windows.Forms.Timer

End Class

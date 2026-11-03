<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SyncForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.chkSyncFaktur = New System.Windows.Forms.CheckBox()
        Me.chkSyncJurnal = New System.Windows.Forms.CheckBox()
        Me.cmbBrand = New System.Windows.Forms.ComboBox()


        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Lblcount = New System.Windows.Forms.Label()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.TextBox3 = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.DateTimePicker1 = New System.Windows.Forms.DateTimePicker()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'chkSyncFaktur
        '
        Me.chkSyncFaktur.AutoSize = True
        Me.chkSyncFaktur.Checked = True
        Me.chkSyncFaktur.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSyncFaktur.Location = New System.Drawing.Point(94, 110)
        Me.chkSyncFaktur.Name = "chkSyncFaktur"
        Me.chkSyncFaktur.Size = New System.Drawing.Size(120, 17)
        Me.chkSyncFaktur.TabIndex = 19
        Me.chkSyncFaktur.Text = "Sync Faktur && Payment"
        Me.chkSyncFaktur.UseVisualStyleBackColor = True
        '
        'chkSyncJurnal
        '
        Me.chkSyncJurnal.AutoSize = True
        Me.chkSyncJurnal.Location = New System.Drawing.Point(230, 110)
        Me.chkSyncJurnal.Name = "chkSyncJurnal"
        Me.chkSyncJurnal.Size = New System.Drawing.Size(110, 17)
        Me.chkSyncJurnal.TabIndex = 20
        Me.chkSyncJurnal.Text = "Sync Jurnal Umum"
        Me.chkSyncJurnal.UseVisualStyleBackColor = True
                '
        'cmbBrand
        '
        Me.cmbBrand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBrand.FormattingEnabled = True
        Me.cmbBrand.Location = New System.Drawing.Point(94, 130)
        Me.cmbBrand.Name = "cmbBrand"
        Me.cmbBrand.Size = New System.Drawing.Size(392, 21)
        Me.cmbBrand.TabIndex = 21
        '
        'Timer1
        '
        Me.Timer1.Interval = 1000
        '
        'TextBox2
        '
        Me.TextBox2.Location = New System.Drawing.Point(94, 37)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(392, 20)
        Me.TextBox2.TabIndex = 7
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(94, 11)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(392, 20)
        Me.TextBox1.TabIndex = 6
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 44)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(58, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Session ID"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 18)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(76, 13)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "Access Token"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(20, 160)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(145, 87)
        Me.Button1.TabIndex = 8
        Me.Button1.Text = "Get Access"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(181, 160)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(145, 87)
        Me.Button2.TabIndex = 9
        Me.Button2.Text = "Sync Item"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(346, 160)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(145, 87)
        Me.Button3.TabIndex = 10
        Me.Button3.Text = "Sync Transaction"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Lblcount
        '
        Me.Lblcount.AutoSize = True
        Me.Lblcount.Location = New System.Drawing.Point(507, 323)
        Me.Lblcount.Name = "Lblcount"
        Me.Lblcount.Size = New System.Drawing.Size(13, 13)
        Me.Lblcount.TabIndex = 11
        Me.Lblcount.Text = "0"
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(20, 253)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(145, 87)
        Me.Button4.TabIndex = 12
        Me.Button4.Text = "Sync Akun Payment"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'TextBox3
        '
        Me.TextBox3.Location = New System.Drawing.Point(94, 63)
        Me.TextBox3.Name = "TextBox3"
        Me.TextBox3.Size = New System.Drawing.Size(392, 20)
        Me.TextBox3.TabIndex = 14
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 70)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(85, 13)
        Me.Label2.TabIndex = 13
        Me.Label2.Text = "Sales Master No"
        '
        'Button5
        '
        Me.Button5.Location = New System.Drawing.Point(181, 253)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(145, 87)
        Me.Button5.TabIndex = 15
        Me.Button5.Text = "Sync Transaction Sales Sum Daily"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 92)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(60, 13)
        Me.Label5.TabIndex = 17
        Me.Label5.Text = "Trans Date"
        '
        'DateTimePicker1
        '
        Me.DateTimePicker1.Location = New System.Drawing.Point(94, 86)
        Me.DateTimePicker1.Name = "DateTimePicker1"
        Me.DateTimePicker1.Size = New System.Drawing.Size(392, 20)
        Me.DateTimePicker1.TabIndex = 16
        '
        'Button6
        '
        Me.Button6.Location = New System.Drawing.Point(346, 253)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(145, 87)
        Me.Button6.TabIndex = 18
        Me.Button6.Text = "Sync Item Category"
        Me.Button6.UseVisualStyleBackColor = True
        '
        'SyncForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(527, 370)
        Me.Controls.Add(Me.chkSyncFaktur)
        Me.Controls.Add(Me.chkSyncJurnal)
        Me.Controls.Add(Me.cmbBrand)

        Me.Controls.Add(Me.Button6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.DateTimePicker1)
        Me.Controls.Add(Me.Button5)
        Me.Controls.Add(Me.TextBox3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Lblcount)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.TextBox2)
        Me.Controls.Add(Me.TextBox1)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label4)
        Me.Name = "SyncForm"
        Me.Text = "Sync Data - DreamposXAccurate"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Timer1 As Timer
    Friend WithEvents TextBox2 As TextBox
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents Lblcount As Label
    Friend WithEvents Button4 As Button
    Friend WithEvents TextBox3 As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Button5 As Button
    Friend WithEvents Label5 As Label
    Friend WithEvents DateTimePicker1 As DateTimePicker
    Friend WithEvents Button6 As Button
    Friend WithEvents chkSyncFaktur As CheckBox
    Friend WithEvents chkSyncJurnal As CheckBox
    Friend WithEvents cmbBrand As ComboBox

End Class


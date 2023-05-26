Imports System.Runtime.InteropServices
Imports System.Threading
Imports Windows.Media
Imports Windows.Media.Audio
Imports Windows.Media.MediaProperties
Imports Windows.System

MustInherit Class 步骤
	Protected Const 无效字符 As Char = ChrW(0)

	Protected Shared Function 小鼠定位(按键 As VirtualKey, 计分表 As IReadOnlyList(Of 小鼠计分)) As SByte
		Dim 字符 As Char
		If 按键 >= VirtualKey.A AndAlso 按键 <= VirtualKey.Z Then
			字符 = ChrW(AscW("A"c) + (按键 - VirtualKey.A))
		ElseIf 按键 >= VirtualKey.NumberPad0 AndAlso 按键 <= VirtualKey.NumberPad9 Then
			字符 = ChrW(AscW("0"c) + (按键 - VirtualKey.NumberPad0))
		ElseIf 按键 >= VirtualKey.Number0 AndAlso 按键 <= VirtualKey.Number9 Then
			字符 = ChrW(AscW("0"c) + (按键 - VirtualKey.Number0))
		Else
			Return -1
		End If
		For a As SByte = 0 To 计分表.Count - 1
			If 计分表(a).参训小鼠 = 字符 Then
				Return a
			End If
		Next
		Return -1
	End Function

	MustOverride Function 运行() As Task

	Overridable Sub 暂停()
	End Sub

	Overridable Sub 继续()
	End Sub

	Overridable Sub 取消()
	End Sub

End Class

Public Interface I监视器
	Event 按键(键位 As VirtualKey)
End Interface

Class 冷静步骤
	Inherits 步骤
	ReadOnly 监视器 As I监视器
	ReadOnly 计分表 As IReadOnlyList(Of 小鼠计分)
	ReadOnly 最短时间 As UShort
	ReadOnly 最长时间 As UShort
	Shared ReadOnly 随机生成器 As New Random
	ReadOnly 事件等待句柄 As New EventWaitHandle(False, EventResetMode.AutoReset)
	Private 本次等待时间 As UShort
	ReadOnly 计时器 As New Timer(AddressOf 步骤结束)

	Private Sub 监视器_按键(按键 As VirtualKey)
		计时器.Change(本次等待时间, Timeout.Infinite)
		Dim 鼠号 As SByte = 小鼠定位(按键, 计分表)
		If 鼠号 >= 0 Then
			For a As Byte = 0 To 计分表.Count - 1
				If a <> 鼠号 Then
					计分表(a).加分(1)
				End If
			Next
		End If
	End Sub

	Private Sub 步骤结束()
		RemoveHandler 监视器.按键, AddressOf 监视器_按键
		事件等待句柄.Set()
	End Sub

	Public Overrides Function 运行() As Task
		本次等待时间 = 随机生成器.Next(最短时间, 最长时间)
		AddHandler 监视器.按键, AddressOf 监视器_按键
		计时器.Change(本次等待时间, Timeout.Infinite)
		Return Task.Run(AddressOf 事件等待句柄.WaitOne)
	End Function

	Sub New(监视器 As I监视器, 计分表 As IReadOnlyList(Of 小鼠计分), 最短时间 As UShort, 最长时间 As UShort)
		Me.监视器 = 监视器
		Me.计分表 = 计分表
		Me.最短时间 = 最短时间
		Me.最长时间 = 最长时间
	End Sub

	Public Overrides Sub 暂停()
		计时器.Change(Timeout.Infinite, Timeout.Infinite)
		RemoveHandler 监视器.按键, AddressOf 监视器_按键
	End Sub

	Public Overrides Sub 继续()
		计时器.Change(本次等待时间, Timeout.Infinite)
		AddHandler 监视器.按键, AddressOf 监视器_按键
	End Sub

	Public Overrides Sub 取消()
		计时器.Change(0, Timeout.Infinite)
	End Sub
End Class

Class 蓝光步骤
	Inherits 步骤
	ReadOnly 亮灯 As Action
	ReadOnly 熄灯计时器 As Timer
	ReadOnly 亮灯毫秒数 As UShort

	Public Overrides Async Function 运行() As Task
		亮灯()
		熄灯计时器.Change(亮灯毫秒数, Timeout.Infinite)
	End Function

	Sub New(亮灯 As Action, 熄灯 As TimerCallback, 亮灯毫秒数 As UShort)
		Me.亮灯 = 亮灯
		熄灯计时器 = New Timer(熄灯)
		Me.亮灯毫秒数 = 亮灯毫秒数
	End Sub
End Class

Class 竞速步骤
	Inherits 步骤
	ReadOnly 时长 As UShort
	ReadOnly 监视器 As I监视器
	ReadOnly 计分表 As IReadOnlyList(Of 小鼠计分)
	ReadOnly 事件等待句柄 As New EventWaitHandle(False, EventResetMode.AutoReset)
	Private 额外奖励 As Byte
	Private 延迟奖励 As Byte()
	ReadOnly 立即结算 As Boolean
	ReadOnly 奖励 As Boolean

	Private Sub 结束()
		RemoveHandler 监视器.按键, AddressOf 按键回调
		If Not 立即结算 Then
			For a As Byte = 0 To 计分表.Count - 1
				计分表(a).加分(延迟奖励(a))
			Next
		End If
		事件等待句柄.Set()
	End Sub

	ReadOnly 结束计时器 As New Timer(AddressOf 结束)

	Private Sub 按键回调(按键 As VirtualKey)
		Dim 鼠号 As SByte = 小鼠定位(按键, 计分表)
		If 鼠号 >= 0 Then
			If 奖励 Then
				For a As Byte = 0 To 计分表.Count - 1
					If a = 鼠号 Then
						If 延迟奖励(a) = 0 Then
							延迟奖励(a) = 额外奖励
							If 立即结算 Then
								计分表(a).加分(额外奖励)
							End If
							额外奖励 -= 1
						End If
					Else
						计分表(a).加分(1)
					End If
				Next
			Else
				Dim 加分 As Byte = 1
				If 延迟奖励(鼠号) = 0 Then
					加分 = 额外奖励
					延迟奖励(鼠号) = 额外奖励
					额外奖励 -= 1
				End If
				For a As Byte = 0 To 计分表.Count - 1
					If a <> 鼠号 Then
						计分表(a).加分(加分)
					End If
				Next
			End If
		End If
	End Sub

	Public Overrides Function 运行() As Task
		额外奖励 = 计分表.Count + If(奖励, 计分表.Count + 1, 1)
		ReDim 延迟奖励(计分表.Count - 1)
		结束计时器.Change(时长, Timeout.Infinite)
		AddHandler 监视器.按键, AddressOf 按键回调
		Return Task.Run(AddressOf 事件等待句柄.WaitOne)
	End Function
	''' <summary>
	''' 只有奖励才能延迟结算
	''' </summary>
	Sub New(毫秒数 As UShort, 监视器 As I监视器, 计分表 As IReadOnlyList(Of 小鼠计分), 立即结算 As Boolean, 奖励 As Boolean)
		If 立即结算 OrElse 奖励 Then
			时长 = 毫秒数
			Me.监视器 = 监视器
			Me.计分表 = 计分表
			Me.立即结算 = 立即结算
			Me.奖励 = 奖励
		Else
			Throw New ArgumentException("不支持延迟结算惩罚")
		End If
	End Sub

	Public Overrides Sub 暂停()
		结束计时器.Change(Timeout.Infinite, Timeout.Infinite)
		RemoveHandler 监视器.按键, AddressOf 按键回调
	End Sub

	Public Overrides Sub 继续()
		结束计时器.Change(时长, Timeout.Infinite)
		AddHandler 监视器.按键, AddressOf 按键回调
	End Sub

	Public Overrides Sub 取消()
		结束计时器.Change(0, Timeout.Infinite)
	End Sub
End Class

<ComImport>
<Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")>
<InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
Interface IMemoryBufferByteAccess
	Sub GetBuffer(ByRef buffer As IntPtr, ByRef capacity As UInteger)
End Interface

''' <summary>
''' 使用此类之前必须先完成初始化
''' </summary>
Class 声音步骤
	Inherits 步骤
	Shared 音频图 As AudioGraph
	Shared 音频设备输出节点 As AudioDeviceOutputNode
	Shared 音频编码属性 As AudioEncodingProperties
	WithEvents 音频帧输入节点 As AudioFrameInputNode = 音频图.CreateFrameInputNode(音频编码属性)
	ReadOnly 毫秒数 As UShort
	ReadOnly 计时器 As New Timer(AddressOf 音频帧输入节点.Stop)
	ReadOnly 音频帧 As New AudioFrame(音频图.SamplesPerQuantum * 4)

	Public Overrides Async Function 运行() As Task
		音频帧输入节点.Start()
		计时器.Change(毫秒数, Timeout.Infinite)
	End Function

	Sub New(频率 As UShort, 毫秒数 As UShort)
		音频帧输入节点.Stop()
		音频帧输入节点.AddOutgoingConnection(音频设备输出节点)
		Me.毫秒数 = 毫秒数
		Dim 指针 As IntPtr
		Dim 容量 As UInteger
		Dim 音频缓冲 As AudioBuffer = 音频帧.LockBuffer(AudioBufferAccessMode.Write)
		Dim 内存缓冲引用 As IMemoryBufferReference = 音频缓冲.CreateReference
		DirectCast(内存缓冲引用, IMemoryBufferByteAccess).GetBuffer(指针, 容量)
		Marshal.Copy((From a As Single In Enumerable.Range(0, 音频图.SamplesPerQuantum) Select CSng(Math.Sin(2 * Math.PI * 频率 / 音频编码属性.SampleRate * a))).ToArray, 0, 指针, 音频图.SamplesPerQuantum)
		内存缓冲引用.Dispose()
		音频缓冲.Dispose()
	End Sub

	''' <summary>
	''' 必须先初始化才能使用此类
	''' </summary>
	Shared Async Function 初始化() As Task
		音频图 = (Await AudioGraph.CreateAsync(New AudioGraphSettings(Render.AudioRenderCategory.Alerts))).Graph
		音频编码属性 = 音频图.EncodingProperties
		音频编码属性.ChannelCount = 1
		Dim 创建音频设备输出节点结果 As CreateAudioDeviceOutputNodeResult = Await 音频图.CreateDeviceOutputNodeAsync()
		If 创建音频设备输出节点结果.Status = AudioDeviceNodeCreationStatus.Success Then
			音频设备输出节点 = 创建音频设备输出节点结果.DeviceOutputNode
		Else
			Throw 创建音频设备输出节点结果.ExtendedError
		End If
		音频图.Start()
	End Function

	Private Sub 音频帧输入节点_QuantumStarted(sender As AudioFrameInputNode, args As FrameInputNodeQuantumStartedEventArgs) Handles 音频帧输入节点.QuantumStarted
		If 音频帧 IsNot Nothing Then
			sender.AddFrame(音频帧)
		End If
	End Sub
End Class

Class 等待步骤
	Inherits 步骤
	ReadOnly 毫秒数 As UShort
	ReadOnly 事件等待句柄 As New EventWaitHandle(False, EventResetMode.AutoReset)
	ReadOnly 监视器 As I监视器
	ReadOnly 计分表 As IReadOnlyList(Of 小鼠计分)

	Private Sub 按键回调(按键 As VirtualKey)
		Dim 鼠号 As SByte = 小鼠定位(按键, 计分表)
		If 鼠号 >= 0 Then
			For a As Byte = 0 To 计分表.Count - 1
				If a <> 鼠号 Then
					计分表(a).加分(1)
				End If
			Next
		End If
	End Sub

	Private Sub 步骤结束()
		RemoveHandler 监视器.按键, AddressOf 按键回调
		事件等待句柄.Set()
	End Sub

	ReadOnly 计时器 As New Timer(AddressOf 步骤结束)

	Public Overrides Function 运行() As Task
		AddHandler 监视器.按键, AddressOf 按键回调
		计时器.Change(毫秒数, Timeout.Infinite)
		Return Task.Run(AddressOf 事件等待句柄.WaitOne)
	End Function

	Sub New(毫秒数 As UShort, 监视器 As I监视器, 计分表 As IReadOnlyList(Of 小鼠计分))
		Me.毫秒数 = 毫秒数
		Me.监视器 = 监视器
		Me.计分表 = 计分表
	End Sub
End Class

Class 回合类
	Private 所有步骤 As 步骤()
	Private 当前步骤 As 步骤
	Private 已取消 As Boolean

	Async Function 运行() As Task
		已取消 = False
		For Each 当前步骤 In 所有步骤
			If 已取消 Then
				Exit For
			Else
				Await 当前步骤.运行()
			End If
		Next
	End Function

	Sub 暂停()
		当前步骤.暂停()
	End Sub

	Sub 继续()
		当前步骤.继续()
	End Sub

	Sub 取消()
		已取消 = True
		当前步骤.取消()
	End Sub

	Sub New(ParamArray 步骤 As 步骤())
		所有步骤 = 步骤
	End Sub
End Class

Structure 回合重复数
	Property 回合 As 回合类
	Property 重复数 As Byte
End Structure

Class 会话
	ReadOnly 所有回合 As 回合重复数()
	ReadOnly 随机 As Boolean
	Private 当前回合 As 回合类
	Private 已取消 As Boolean

	'如果需要等待结束回调才能结束此会话，返回True
	Async Function 运行() As Task
		If 随机 Then
			Dim 回合枚举 As IEnumerable(Of 回合类) = {}
			For Each 回合 As 回合重复数 In 所有回合
				回合枚举 = 回合枚举.Concat(Enumerable.Repeat(回合.回合, 回合.重复数))
			Next
			For Each 回合 As 回合类 In MathNet.Numerics.Combinatorics.SelectPermutation(回合枚举)
				If 已取消 Then
					Exit For
				Else
					当前回合 = 回合
					Await 当前回合.运行
				End If
			Next
		Else
			For Each 回合 As 回合重复数 In 所有回合
				当前回合 = 回合.回合
				For a = 1 To 回合.重复数
					If 已取消 Then
						Exit Function
					Else
						Await 当前回合.运行
					End If
				Next
			Next
		End If
	End Function

	Sub 暂停()
		当前回合.暂停()
	End Sub

	Sub 继续()
		当前回合.继续()
	End Sub

	Sub 取消()
		已取消 = True
		当前回合.取消()
	End Sub

	Sub New(随机 As Boolean, ParamArray 所有回合 As 回合重复数())
		Me.所有回合 = 所有回合
		Me.随机 = 随机
	End Sub

	Sub New(回合 As 回合类, 重复数 As Byte)
		所有回合 = {New 回合重复数 With {.回合 = 回合, .重复数 = 重复数}}
		随机 = False
	End Sub
End Class

Public Enum 会话状态
	未运行
	运行中
	暂停中
End Enum

Public Module 实验模块
	Private 所有会话 As 会话()
	Private 当前会话 As 会话
	Private 状态 As 会话状态 = 会话状态.未运行
	Private 已取消 As Boolean

	Async Function 运行会话() As Task
		If 状态 = 会话状态.未运行 Then
			状态 = 会话状态.运行中
			已取消 = False
			For Each 当前会话 In 所有会话
				If 已取消 Then
					Exit For
				Else
					Await 当前会话.运行
				End If
			Next
			状态 = 会话状态.未运行
		Else
			Throw New InvalidOperationException("当前已有会话在运行")
		End If
	End Function

	Sub 暂停会话()
		If 状态 = 会话状态.运行中 Then
			当前会话.暂停()
			状态 = 会话状态.暂停中
		Else
			Throw New InvalidOperationException("会话未运行")
		End If
	End Sub

	Sub 继续会话()
		If 状态 = 会话状态.暂停中 Then
			当前会话.继续()
			状态 = 会话状态.运行中
		Else
			Throw New InvalidOperationException("只有暂停的会话才能继续")
		End If
	End Sub

	Sub 取消会话()
		If 状态 = 会话状态.未运行 Then
			Throw New InvalidOperationException("会话未运行")
		Else
			已取消 = True
			当前会话.取消()
			状态 = 会话状态.未运行
		End If
	End Sub

	Async Function 初始化(监视器 As I监视器, 计分表 As IReadOnlyList(Of 小鼠计分), 亮灯 As Action, 熄灯 As TimerCallback) As Task
		Await 声音步骤.初始化
		Dim 冷静 As New 冷静步骤(监视器, 计分表, 5000, 10000)
		Dim 竞速 As New 竞速步骤(1000, 监视器, 计分表, False, True)
		Dim 蓝光 As New 蓝光步骤(亮灯, 熄灯, 200)
		Dim 等待 As New 等待步骤(1000, 监视器, 计分表)
		Dim 高音 As New 声音步骤(5000, 200)
		Dim 低音 As New 声音步骤(1000, 200)
		Dim 奖励 As New 竞速步骤(2000, 监视器, 计分表, True, True)
		Dim 惩罚 As New 竞速步骤(2000, 监视器, 计分表, True, False)
		所有会话 = {
			New 会话(New 回合类(冷静, New 声音步骤(2000, 200), 竞速), 10),
			New 会话(New 回合类(冷静, 蓝光, 竞速), 5),
			New 会话(True, New 回合重复数 With {.回合 = New 回合类(冷静, 高音, 等待, 蓝光, 奖励), .重复数 = 5}, New 回合重复数 With {.回合 = New 回合类(冷静, 低音, 等待, 蓝光, 惩罚), .重复数 = 5}),
			New 会话(True, New 回合重复数 With {.回合 = New 回合类(冷静, 高音, 等待, 惩罚), .重复数 = 5}, New 回合重复数 With {.回合 = New 回合类(冷静, 低音, 等待, 奖励), .重复数 = 5})
		}
	End Function
End Module
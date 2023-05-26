Imports System.Threading
Imports Windows.UI.Core

Public Class 小鼠计分
	Implements INotifyPropertyChanged
	Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
	Public Shared 调度方法 As Action(Of DispatchedHandler)
	Private i参训小鼠 As Char

	Property 参训小鼠 As Char
		Get
			Return i参训小鼠
		End Get
		Set(value As Char)
			i参训小鼠 = value
			Static 属性已更改事件参数 As New PropertyChangedEventArgs("参训小鼠")
			调度方法(Sub() RaiseEvent PropertyChanged(Me, 属性已更改事件参数))
		End Set
	End Property

	Private i总积分 As UShort = 0

	Property 总积分 As UShort
		Get
			Return i总积分
		End Get
		Set(value As UShort)
			i总积分 = value
			Static 属性已更改事件参数 As New PropertyChangedEventArgs("总积分")
			调度方法(Sub() RaiseEvent PropertyChanged(Me, 属性已更改事件参数))
		End Set
	End Property

	Private i得分 As Byte

	ReadOnly Property 得分 As String
		Get
			If i得分 = 0 Then
				Return ""
			Else
				Return $"+{i得分}"
			End If
		End Get
	End Property

	Sub New(参训小鼠 As Char)
		i参训小鼠 = 参训小鼠
	End Sub

	Private Sub 设置得分(得分 As Byte)
		i得分 = 得分
		Static 得分变化事件参数 As New PropertyChangedEventArgs("得分")
		调度方法(Sub() RaiseEvent PropertyChanged(Me, 得分变化事件参数))
	End Sub

	Private 清除得分 As New Timer(Sub() 设置得分(0))

	Sub 加分(分数 As Byte)
		总积分 += 分数
		设置得分(i得分 + 分数)
		清除得分.Change(1000, Timeout.Infinite)
	End Sub
End Class

' https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

Imports Windows.System
Imports Windows.UI
Imports Windows.UI.Core

Class 按键监视器
	Implements I监视器
	Public Event 按键(键位 As VirtualKey) Implements I监视器.按键

	Sub On按键(sender As CoreWindow, args As KeyEventArgs)
		RaiseEvent 按键(args.VirtualKey)
	End Sub
End Class

''' <summary>
''' 可用于自身或导航至 Frame 内部的空白页。
''' </summary>
Public NotInheritable Class 游戏界面
	Inherits Page
	Private 计分表 As New ObservableCollection(Of 小鼠计分)
	WithEvents 核心窗口 As CoreWindow = Window.Current.CoreWindow

	Sub 亮灯()
		Static 蓝色 As New SolidColorBrush(Color.FromArgb(255, 0, 0, 255))
		Call Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Sub() 蓝灯.Fill = 蓝色)
	End Sub

	Sub 熄灯()
		Call Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Sub() 蓝灯.Fill = Nothing)
	End Sub

	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。
		计分板.ItemsSource = 计分表
		Dim 监视器 As New 按键监视器
		AddHandler 核心窗口.KeyDown, AddressOf 监视器.On按键
		初始化(监视器, 计分表, AddressOf 亮灯, AddressOf 熄灯)
	End Sub

	Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
		MyBase.OnNavigatedTo(e)
		小鼠计分.调度方法 = Sub(方法 As DispatchedHandler) Call Dispatcher.RunAsync(CoreDispatcherPriority.Normal, 方法)
		For Each C As Char In DirectCast(e.Parameter, String)
			计分表.Add(New 小鼠计分(C))
		Next
	End Sub

	Private Sub 添加新鼠_Click(sender As Object, e As RoutedEventArgs) Handles 添加新鼠.Click
		Dim 鼠 As IEnumerable(Of Char) = (From a As 小鼠计分 In 计分表 Select a.参训小鼠).Concat(新鼠按键.Text)
		If 鼠.Distinct.Count < 鼠.Count Then
			报错.Text = "不允许有重复的按键"
			Exit Sub
		End If
		If 鼠.Count < 2 Then
			报错.Text = "至少需要2只鼠"
			Exit Sub
		End If
		For Each C As Char In 新鼠按键.Text
			If (C < "0"c OrElse C > "9"c) AndAlso (C < "A"c OrElse C > "Z"c) Then
				报错.Text = "按键只能使用数字或大写字母"
				Exit Sub
			End If
		Next
		For Each C As Char In 新鼠按键.Text
			计分表.Add(New 小鼠计分(C))
		Next
	End Sub

	Private Sub 确认重置小鼠_Click(sender As Object, e As RoutedEventArgs) Handles 确认重置小鼠.Click
		计分表.Clear()
	End Sub

	Private Sub 取消训练_Click(sender As Object, e As RoutedEventArgs) Handles 取消训练.Click
		取消会话()
	End Sub

	Private Async Sub 开始训练_Click(sender As Object, e As RoutedEventArgs) Handles 开始训练.Click
		添加新鼠.IsEnabled = False
		请求重置小鼠.IsEnabled = False
		开始训练.IsEnabled = False
		暂停训练.IsEnabled = True
		取消训练.IsEnabled = True
		Await 运行会话()
		添加新鼠.IsEnabled = True
		请求重置小鼠.IsEnabled = True
		开始训练.IsEnabled = True
		暂停训练.IsEnabled = False
		继续训练.IsEnabled = False
		取消训练.IsEnabled = False
	End Sub

	Private Sub 暂停训练_Click(sender As Object, e As RoutedEventArgs) Handles 暂停训练.Click
		暂停会话()
		继续训练.IsEnabled = True
		暂停训练.IsEnabled = False
	End Sub

	Private Sub 继续训练_Click(sender As Object, e As RoutedEventArgs) Handles 继续训练.Click
		继续会话()
		继续训练.IsEnabled = False
		暂停训练.IsEnabled = True
	End Sub
End Class

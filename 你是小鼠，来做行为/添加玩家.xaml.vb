' https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

''' <summary>
''' 可用于自身或导航至 Frame 内部的空白页。
''' </summary>
Public NotInheritable Class 添加玩家
    Inherits Page

	Private Sub 开始训行为_Click(sender As Object, e As RoutedEventArgs) Handles 开始训行为.Click
		If 玩家行动键.Text.Distinct.Count < 玩家行动键.Text.Length Then
			报错.Text = "不允许有重复的按键"
			Exit Sub
		End If
		If 玩家行动键.Text.Length < 2 Then
			报错.Text = "至少要有2个玩家"
			Exit Sub
		End If
		For Each C As Char In 玩家行动键.Text
			If (C < "0"c OrElse C > "9"c) AndAlso (C < "A"c OrElse C > "Z"c) Then
				报错.Text = "行动键只能使用数字或大写字母"
				Exit Sub
			End If
		Next
		DirectCast(Window.Current.Content, Frame).Navigate(GetType(游戏界面), 玩家行动键.Text)
	End Sub
End Class

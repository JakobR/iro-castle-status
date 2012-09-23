
<ValueConversion(GetType(Boolean), GetType(Visibility))>
Public Class BooleanVisibilityConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert

        If TypeOf value Is Boolean Then
            Dim b = CBool(value)

            Return If(b, Visibility.Visible, Visibility.Collapsed)
        Else
            Throw New NotSupportedException("BooleanVisibilityConverter.Convert only supports Boolean as value type.")
        End If

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack

        If TypeOf value Is Visibility Then
            Dim v = DirectCast(value, Visibility)

            Return (v = Visibility.Visible)
        Else
            Throw New NotSupportedException("BooleanVisibilityConverter.Convert only supports System.Windows.Visibility as value type.")
        End If

    End Function

End Class

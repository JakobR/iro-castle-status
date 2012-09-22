
<ValueConversion(GetType(Castle), GetType(Boolean))>
<ValueConversion(GetType(Realm), GetType(Boolean))>
Public Class CastleAndRealmVisibilityConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert

        If TypeOf value Is Castle Then
            Dim c = DirectCast(value, Castle)
            Return If(c.Enabled, Visibility.Visible, Visibility.Collapsed)
        Else
            Return Visibility.Visible
        End If

    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotSupportedException("Cannot convert Visibility to Castle or Realm.")
    End Function

End Class

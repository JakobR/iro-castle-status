
<ValueConversion(GetType(Object), GetType(System.Type))>
Public Class ObjectTypeConverter
    Implements IValueConverter


    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
        Return value.GetType
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException
    End Function
End Class

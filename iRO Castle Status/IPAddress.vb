
Public Module IPAddress

    ''' <summary>
    ''' Checks if the IP address is in Gravity's Network, which seems to be 128.241.92.0/23.
    ''' </summary>
    ''' <param name="IP">The IP address to check.</param>
    <System.Runtime.CompilerServices.Extension()>
    Public Function BelongsToGravity(IP As Net.IPAddress) As Boolean

        Debug.Assert(IP IsNot Nothing)

        Dim b = IP.GetAddressBytes

        Return b(0) = 128 AndAlso b(1) = 241 AndAlso (b(2) = 92 OrElse b(2) = 93)

    End Function

End Module

Namespace Core

    Public Class Globals
        Public Shared Property SecurityProtocol As System.Net.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12
        Public Shared Property BaseURL As String = "https://37latuqm766patrerdf5rvdhqe0wgrug.lambda-url.us-east-1.on.aws/"
        Public Shared Property BaseCaptcha As String = "03AFcWeA7vP299Z_sTJ_VK5LkLLro7iTpCG1GQ2oJHB3BVXoJ0x4CVSv_-fjCGt1c14X_84oA-jEUO25fmE28mm6EdpqkVH27f6UklJw2Odvo5FSgFy9klfTIdREM1gZARH3LjH8k1qK5PDOdr_7XJBypu7xae0Bcr31aNHOQU4sHAuV4RuWbXTG8lhhqWekMBAXmXqx4e0GisqlbjLy5tx9MKDCKAQxpES6dJLFhVFh-8wkfSz92-gUw4PynhWqvZR8fNWoTLtxJieA6UJw9q_h-k7yX-IzC-gymRqV-oINhBzVgruSHbAe-IgAGWKLkUVwiUNHa29ZBt3yEqDFhnXEimuZt79V6PyxHipJTKD2jWszCKPM7j0dYi_lv-F658Wv9OAKK-hKMTzQkC6U-uw1kAISeOt4uJdjpJ4e_hTkFGfRMSgm993EpEUphFfFf-1KjNPvLt3sxvKaa2qWQMS0NSUFg0PH1xWBrn8Npwal8o_FhFPf6y737114y6yMnJQ4j6_TOQ3GWH97lKuHi3RFiVajJsEAHooAAD5-3y8QbwBBXxnoTat4hm5ANc7EvFgtG3FvtoJkUCql2kpHQug_rWU53nwxA89d-u_ewCsVXoQqXJ6ZKPqIqL22bbGOmJNWj49kBKlJw_JtPvkwrIwaB4D3JEPtkHm5nYbn0FU-hr-T2205Bhhaw8MJ5JIfxnn3vmT7F222tORHmqJ2K-4ddDcAypF9QYSA1fe4jK_Dc610zG3vdNVolX2N8VhBc67i7EH-O__RojfFXu4nVlI3ye-NRQzfjvPI29XfJwwsSpuueTX1PvTgXUhdmTLTCU_ng6UCjKv6rgFgeTyEZAjvyGh4hamDeu8SsFhXbgC5KVvVMP_j21apYcyxkVmUioACKNbtvajtMv"

        Public Shared Function MakeUrlRequest(ByVal DNI As String) As String
            Return String.Format("{0}?cedula=V{1}&recaptcha={2}", BaseURL, DNI, BaseCaptcha)
        End Function

    End Class

End Namespace


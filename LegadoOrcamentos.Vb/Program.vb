Imports System
Imports System.IO
Imports System.Net
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Program
    Sub Main(args As String())
        Dim porta As Integer = If(args.Length > 0, Integer.Parse(args(0)), 5099)
        Using listener As New HttpListener()
            listener.Prefixes.Add($"http://127.0.0.1:{porta}/")
            listener.Start()
            Console.WriteLine($"Legado a servir em http://127.0.0.1:{porta}")
            While True
                Dim contexto = listener.GetContext()
                Try
                    Atender(contexto)
                Catch
                    Try
                        contexto.Response.Abort()
                    Catch
                    End Try
                End Try
            End While
        End Using
    End Sub

    Private Sub Atender(ctx As HttpListenerContext)
        Dim caminho = ctx.Request.Url.AbsolutePath
        If caminho = "/health" AndAlso ctx.Request.HttpMethod = "GET" Then
            Responder(ctx, 200, "{""status"":""ok""}")
        ElseIf caminho = "/orcamentos/calcular" AndAlso ctx.Request.HttpMethod = "POST" Then
            Dim corpo As String
            Using leitor As New StreamReader(ctx.Request.InputStream, Encoding.UTF8)
                corpo = leitor.ReadToEnd()
            End Using
            Try
                Dim pedido = JsonConvert.DeserializeObject(Of PedidoOrcamento)(corpo)
                If pedido Is Nothing Then Throw New JsonSerializationException("vazio")
                Dim resultado = OrcamentoServico.Calcular(pedido)
                Responder(ctx, 200, resultado.ToString(Formatting.None))
            Catch ex As CalculoException
                Responder(ctx, 400, Erro(ex.Message))
            Catch ex As JsonException
                Responder(ctx, 400, Erro("json invalido"))
            End Try
        Else
            Responder(ctx, 404, Erro("nao encontrado"))
        End If
    End Sub

    Private Function Erro(mensagem As String) As String
        Dim no As New JObject()
        no("erro") = mensagem
        Return no.ToString(Formatting.None)
    End Function

    Private Sub Responder(ctx As HttpListenerContext, status As Integer, json As String)
        Dim buffer = Encoding.UTF8.GetBytes(json)
        ctx.Response.StatusCode = status
        ctx.Response.ContentType = "application/json; charset=utf-8"
        ctx.Response.ContentLength64 = buffer.Length
        ctx.Response.OutputStream.Write(buffer, 0, buffer.Length)
        ctx.Response.OutputStream.Close()
    End Sub
End Module

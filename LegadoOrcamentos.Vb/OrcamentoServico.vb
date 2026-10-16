Imports System
Imports System.Collections.Generic
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class CalculoException
    Inherits Exception

    Public Sub New(mensagem As String)
        MyBase.New(mensagem)
    End Sub
End Class

Public Class LinhaPedido
    Public Property Sku As String
    Public Property Quantidade As Integer
    Public Property PrecoUnitario As Decimal
End Class

Public Class PedidoOrcamento
    Public Property Linhas As List(Of LinhaPedido)
    Public Property Pais As String
End Class

Public NotInheritable Class OrcamentoServico
    Private Sub New()
    End Sub

    Public Shared Function Calcular(pedido As PedidoOrcamento) As JObject
        If pedido Is Nothing OrElse pedido.Linhas Is Nothing OrElse pedido.Linhas.Count = 0 Then
            Throw New CalculoException("pelo menos uma linha")
        End If

        Dim subtotal As Decimal = 0D
        For Each linha In pedido.Linhas
            If linha Is Nothing OrElse String.IsNullOrWhiteSpace(linha.Sku) Then
                Throw New CalculoException("linha invalida")
            End If
            If linha.Quantidade < 1 Then
                Throw New CalculoException("quantidade deve ser >= 1")
            End If
            If linha.PrecoUnitario < 0 Then
                Throw New CalculoException("precoUnitario deve ser >= 0")
            End If
            subtotal += CDec(linha.Quantidade) * linha.PrecoUnitario
        Next

        Dim pct As Decimal
        Select Case subtotal
            Case Is < 100D
                pct = 0D
            Case Is < 500D
                pct = 2D
            Case Is < 1000D
                pct = 5D
            Case Else
                pct = 8D
        End Select

        Dim desconto = Math.Round(subtotal * pct / 100D, 2, MidpointRounding.ToEven)
        Dim baseValor = subtotal - desconto

        Dim ivaPct As Decimal
        Dim portesCheio As Decimal
        Select Case pedido.Pais
            Case "PT"
                ivaPct = 23D
                portesCheio = 4.9D
            Case "ES"
                ivaPct = 21D
                portesCheio = 9.9D
            Case "FR"
                ivaPct = 20D
                portesCheio = 14.9D
            Case "OUT"
                ivaPct = 0D
                portesCheio = 19.9D
            Case Else
                Throw New CalculoException("pais desconhecido")
        End Select

        Dim portes = If(subtotal >= 500D, 0D, portesCheio)
        Dim iva = Math.Round(baseValor * ivaPct / 100D, 2, MidpointRounding.ToEven)
        Dim total = baseValor + iva + portes

        Dim resultado As New JObject()
        resultado("subtotal") = subtotal
        resultado("descontoPercent") = pct
        resultado("descontoEuros") = desconto
        resultado("base") = baseValor
        resultado("ivaPercent") = ivaPct
        resultado("ivaEuros") = iva
        resultado("portes") = portes
        resultado("total") = total
        resultado("moeda") = "EUR"
        Return resultado
    End Function
End Class

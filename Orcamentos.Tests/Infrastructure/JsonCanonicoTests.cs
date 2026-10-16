using Orcamentos.Modern;

namespace Orcamentos.Tests.Infrastructure;

public sealed class JsonCanonicoTests
{
    [Fact]
    public void Ordem_das_propriedades_nao_importa() =>
        Assert.True(JsonCanonico.Igual("""{"a":1,"b":2}""", """{"b":2,"a":1}"""));

    [Fact]
    public void Valor_decimal_ignora_formato() =>
        Assert.True(JsonCanonico.Igual("""{"portes":4.90}""", """{"portes":4.9}"""));

    [Fact]
    public void Inteiro_e_decimal_com_mesmo_valor_sao_iguais() =>
        Assert.True(JsonCanonico.Igual("""{"descontoPercent":0}""", """{"descontoPercent":0.00}"""));

    [Fact]
    public void String_diferente_nao_igual() =>
        Assert.False(JsonCanonico.Igual("""{"moeda":"EUR"}""", """{"moeda":"USD"}"""));

    [Fact]
    public void Campo_extra_diverge() =>
        Assert.False(JsonCanonico.Igual("""{"a":1}""", """{"a":1,"b":2}"""));

    [Fact]
    public void Ordem_de_array_diferente_diverge() =>
        Assert.False(JsonCanonico.Igual("""[1,2]""", """[2,1]"""));

    [Fact]
    public void Json_invalido_compara_como_texto() =>
        Assert.True(JsonCanonico.Igual("nao-json", "nao-json"));

    [Fact]
    public void Divergencias_lista_caminho_do_campo() =>
        Assert.Equal(["total"], JsonCanonico.Divergencias(
            """{"subtotal":10,"total":20}""", """{"subtotal":10,"total":21}"""));

    [Fact]
    public void Divergencias_aninhadas_incluem_prefixo() =>
        Assert.Equal(["linhas[0].quantidade"], JsonCanonico.Divergencias(
            """{"linhas":[{"quantidade":2}]}""", """{"linhas":[{"quantidade":3}]}"""));

    [Fact]
    public void Divergencias_sem_diferencas_vazia() =>
        Assert.Empty(JsonCanonico.Divergencias("""{"a":1}""", """{"a":1}"""));
}

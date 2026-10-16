namespace Orcamentos.Tests.Infrastructure;

public sealed class GoldenStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"golden-{Guid.NewGuid():N}");

    public GoldenStoreTests() => Directory.CreateDirectory(_dir);

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private string Escrever(string nome, string conteudo)
    {
        var caminho = Path.Combine(_dir, nome);
        File.WriteAllText(caminho, conteudo);
        return caminho;
    }

    [Fact]
    public void Ler_extrai_nome_status_corpo_e_request()
    {
        var f = Escrever("a.golden.json", """
        {
          "nome": "a",
          "request": { "linhas": [], "pais": "PT" },
          "esperado": { "status": 400, "body": { "erro": "pelo menos uma linha" } }
        }
        """);
        var g = GoldenStore.Ler(f);
        Assert.Equal("a", g.Nome);
        Assert.Equal(400, g.Status);
        Assert.Equal("pelo menos uma linha", g.Corpo["erro"]!.GetValue<string>());
        Assert.Contains("\"pais\":\"PT\"", g.RequestJson.Replace(" ", ""));
    }

    [Fact]
    public void Carregar_lista_ficheiros_ordenados_por_nome()
    {
        Escrever("b.golden.json", """{ "nome":"b","request":{},"esperado":{"status":200,"body":{}} }""");
        Escrever("a.golden.json", """{ "nome":"a","request":{},"esperado":{"status":200,"body":{}} }""");
        var todos = GoldenStore.Carregar(_dir);
        Assert.Equal(["a", "b"], todos.Select(g => g.Nome).ToArray());
    }

    [Fact]
    public void Carregar_sem_diretorio_lanca()
    {
        var inexistente = Path.Combine(_dir, "nao-existe");
        Assert.Throws<DirectoryNotFoundException>(() => GoldenStore.Carregar(inexistente));
    }
}

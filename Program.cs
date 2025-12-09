using System;
using System.Collections.Generic;
using System.Linq;

//
// ========================================================
// 1. MODELOS IMUTÁVEIS (Records)
// ========================================================

/// <summary>
/// Representa uma laranja no estado bruto, antes do processamento.
/// É imutável (readonly record struct).
/// </summary>
public readonly record struct LaranjaBruta(
    int Id,
    double DiametroMM,
    double ManchaPercentual,
    bool Suja,
    int PesoGramas
);

/// <summary>
/// Representa o resultado final da classificação de uma laranja.
/// É imutável (readonly record struct).
/// </summary>
public readonly record struct LaranjaClassificada(
    int Id,
    bool Aprovada,
    string Motivo
);

/// <summary>
/// Representa um lote de laranjas, contendo uma lista imutável de LaranjaBruta.
/// É imutável (readonly record struct).
/// </summary>
public readonly record struct LoteDeLaranjas(
    int NumeroLote,
    IReadOnlyList<LaranjaBruta> Laranjas
);

//
// ========================================================
// 2. FUNÇÕES PURAS (REGRAS DO NEGÓCIO)
// ========================================================

/// <summary>
/// Contém as funções puras que implementam as regras de classificação.
/// </summary>
public static class LaranjaFn
{
    /// <summary>
    /// Função pura que "limpa" a laranja. Se estiver suja, reduz a mancha para 10% do valor original.
    /// Retorna uma nova LaranjaBruta (imutabilidade).
    /// </summary>
    /// <param name="l">A laranja bruta.</param>
    /// <returns>A laranja limpa (ou a original se já estiver limpa).</returns>
    public static LaranjaBruta Limpa(LaranjaBruta l) =>
        !l.Suja
            ? l
            : l with
            {
                ManchaPercentual = l.ManchaPercentual * 0.10,
                Suja = false
            };

    /// <summary>
    /// Verifica se o tamanho da laranja está entre 60mm e 90mm.
    /// </summary>
    public static bool TamanhoValido(LaranjaBruta l) =>
        l.DiametroMM is >= 60 and <= 90;

    /// <summary>
    /// Verifica se a percentagem de mancha é inferior a 5.0%.
    /// </summary>
    public static bool ManchaValida(LaranjaBruta l) =>
        l.ManchaPercentual < 5.0;

    /// <summary>
    /// Função principal de classificação, aplica todas as regras.
    /// </summary>
    /// <param name="l">A laranja bruta (já limpa).</param>
    /// <returns>O resultado da classificação (LaranjaClassificada).</returns>
    public static LaranjaClassificada Classificar(LaranjaBruta l)
    {
        // ERRO CORRIGIDO: O nome da função de limpeza deve ser 'Limpa', não 'Limpar'.
        var limpa = Limpa(l);

        bool tamanho = TamanhoValido(limpa);
        bool mancha = ManchaValida(limpa);

        string motivo =
            !tamanho ? "REJEITADA: Tamanho fora do padrão." :
            !mancha ? "REJEITADA: Mancha excessiva após limpeza." :
                        "APROVADA";

        return new LaranjaClassificada(l.Id, tamanho && mancha, motivo);
    }
}

//
// ========================================================
// 3. LEITURA DO USUÁRIO — CADASTRO DE LOTES COMPLETOS
// ========================================================
public static class EntradaUsuario
{
    /// <summary>
    /// Lida com a interação com o usuário para ler os dados dos lotes.
    /// </summary>
    public static List<LoteDeLaranjas> LerLotes()
    {
        Console.Write("Quantos lotes deseja cadastrar? ");
        // Lidar com possível Null ou falha de Parse
        if (!int.TryParse(Console.ReadLine(), out int qtdLotes))
        {
            qtdLotes = 0;
        }

        var lotes = new List<LoteDeLaranjas>();

        for (int i = 0; i < qtdLotes; i++)
        {
            int loteNum = 101 + i;

            Console.WriteLine($"\n--- Cadastro do Lote #{loteNum} ---");
            Console.WriteLine("Digite todas as laranjas no formato (separadas por |):");
            Console.WriteLine("ID;Diametro;Mancha%;Suja(s/n);Peso(g) ");
            Console.Write("Entrada do lote: ");

            string entrada = Console.ReadLine() ?? "";

            var laranjas = ParseLote(entrada);

            lotes.Add(new LoteDeLaranjas(loteNum, laranjas));
        }

        return lotes;
    }

    /// <summary>
    /// Função funcional para converter string do lote → lista de laranjas (pipeline de parsing)
    /// </summary>
    private static List<LaranjaBruta> ParseLote(string entrada) =>
        entrada
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Select(ConverterLinhaParaLaranja)
            .ToList();

    /// <summary>
    /// Pura: converte uma linha de texto (ex: "101;80;2;n;200") → objeto LaranjaBruta
    /// </summary>
    private static LaranjaBruta ConverterLinhaParaLaranja(string linha)
    {
        var partes = linha.Split(';');

        // Inclusão de TryParse para robustez, mas mantém a lógica original
        int id = int.TryParse(partes[0], out int tempId) ? tempId : -1;
        double diametro = double.TryParse(partes[1], out double tempDiametro) ? tempDiametro : 0;
        double mancha = double.TryParse(partes[2], out double tempMancha) ? tempMancha : 0;
        bool suja = partes.Length > 3 && partes[3].Trim().ToLower() == "s";
        int peso = partes.Length > 4 && int.TryParse(partes[4], out int tempPeso) ? tempPeso : 0;

        return new LaranjaBruta(id, diametro, mancha, suja, peso);
    }
}

//
// ========================================================
// 4. PROGRAMA PRINCIPAL — PIPELINE FUNCIONAL
// ========================================================
public static class Programa
{
    public static void Main()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== CLASSIFICAÇÃO FUNCIONAL DE LARANJAS POR LOTE ===");
        Console.ResetColor();

        // 1. Entrada de dados
        var lotes = EntradaUsuario.LerLotes();

        foreach (var lote in lotes)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n>>> PROCESSANDO LOTE #{lote.NumeroLote} <<<\n");
            Console.ResetColor();

            // 2. Pipeline Funcional (Limpeza -> Classificação)
            var resultados =
                lote.Laranjas
                    // 2.1. Limpa cada laranja (retorna uma nova LaranjaBruta)
                    .Select(LaranjaFn.Limpa)
                    // 2.2. Classifica a laranja (retorna uma LaranjaClassificada)
                    .Select(LaranjaFn.Classificar)
                    .ToList();

            // 3. Apresentação dos resultados
            foreach (var r in resultados)
            {
                string status = r.Aprovada ? "APROVADA" : "REJEITADA";
                Console.ForegroundColor = r.Aprovada ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"ID {r.Id} → {status,-10} | {r.Motivo}");
                Console.ResetColor();
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n*** PROCESSAMENTO FINALIZADO ***");
        Console.ResetColor();
    }
}
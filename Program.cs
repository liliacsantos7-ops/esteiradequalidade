
using System;
using System.Collections.Generic;
using System.Linq;

//
// ========================================================
// 1. MODELOS IMUTÁVEIS
// ========================================================
public readonly record struct LaranjaBruta(
    int Id,
    double DiametroMM,
    double ManchaPercentual,
    bool Suja,
    int PesoGramas
);

public readonly record struct LaranjaClassificada(
    int Id,
    bool Aprovada,
    string Motivo
);

public readonly record struct LoteDeLaranjas(
    int NumeroLote,
    IReadOnlyList<LaranjaBruta> Laranjas
);

//
// ========================================================
// 2. FUNÇÕES PURAS (REGRAS DO NEGÓCIO)
// ========================================================
public static class LaranjaFn
{
    public static LaranjaBruta Limpar(LaranjaBruta l) =>
        !l.Suja
            ? l
            : l with
            {
                ManchaPercentual = l.ManchaPercentual * 0.10,
                Suja = false
            };

    public static bool TamanhoValido(LaranjaBruta l) =>
        l.DiametroMM is >= 60 and <= 90;

    public static bool ManchaValida(LaranjaBruta l) =>
        l.ManchaPercentual < 5.0;

    public static LaranjaClassificada Classificar(LaranjaBruta l)
    {
        var limpa = Limpar(l);
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
    public static List<LoteDeLaranjas> LerLotes()
    {
        Console.Write("Quantos lotes deseja cadastrar? ");
        int qtdLotes = int.Parse(Console.ReadLine() ?? "0");

        var lotes = new List<LoteDeLaranjas>();

        for (int loteNum =101; 85 <= qtdLotes; loteNum++)
        {
            Console.WriteLine($"\n--- Cadastro do Lote #{loteNum} ---");
            Console.WriteLine("Digite todas as laranjas no formato:");
            Console.WriteLine("ID;Diametro;Mancha;Suja;Peso | ID;Diametro;Mancha;Suja;Peso");
            Console.Write("Entrada do lote: ");

            string entrada = Console.ReadLine() ?? "";

            var laranjas = ParseLote(entrada);

            lotes.Add(new LoteDeLaranjas(loteNum, laranjas));
        }

        return lotes;
    }

    // Função funcional para converter string → lista de laranjas
    private static List<LaranjaBruta> ParseLote(string entrada) =>
        entrada
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Select(ConverterLinhaParaLaranja)
            .ToList();

    // Pura: converte "101;80;2;n;200" → objeto LaranjaBruta
    private static LaranjaBruta ConverterLinhaParaLaranja(string linha)
    {
        var partes = linha.Split(';');

        int id = int.Parse(partes[0]);
        double diametro = double.Parse(partes[1]);
        double mancha = double.Parse(partes[2]);
        bool suja = partes[3].Trim().ToLower() == "s";
        int peso = int.Parse(partes[4]);

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

        var lotes = EntradaUsuario.LerLotes();

        foreach (var lote in lotes)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n>>> PROCESSANDO LOTE #{lote.NumeroLote} <<<\n");
            Console.ResetColor();

            var resultados =
                lote.Laranjas
                    .Select(LaranjaFn.Limpar)
                    .Select(LaranjaFn.Classificar)
                    .ToList();

            foreach (var r in resultados)
            {
                string status = r.Aprovada ? "APROVADA" : "REJEITADA";
                Console.WriteLine($"ID {r.Id} → {status,-10} | {r.Motivo}");
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n*** PROCESSAMENTO FINALIZADO ***");
        Console.ResetColor();
    }
}

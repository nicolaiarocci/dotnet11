using System.Text.Json.Serialization;

namespace UnionDemo;

// ---------------------------------------------------------------
// 1. Union type: un valore che è UNO tra un insieme chiuso di tipi.
//    I casi sono tipi indipendenti, senza una base comune.
// ---------------------------------------------------------------

public record CartaDiCredito(string Circuito, string UltimeCifre);
public record Bonifico(string Iban);
public record Contrassegno(decimal Supplemento);

public union MetodoDiPagamento(CartaDiCredito, Bonifico, Contrassegno);

// ---------------------------------------------------------------
// 2. Closed hierarchy: una gerarchia che si può estendere solo
//    dentro questo assembly. Il compilatore conosce tutti i figli.
// ---------------------------------------------------------------

// The attribute below is required if you want to serialize a closed 
// type, else you'll get an empty object.
// [JsonPolymorphic(InferClosedTypePolymorphism = true)]
public closed record StatoOrdine;
public record InAttesa : StatoOrdine;
public record Spedito(DateOnly Il, string Corriere) : StatoOrdine;
public record Annullato(string Motivo) : StatoOrdine;
// DEMO LIVE: decommentare la riga seguente e ricompilare (CS8509),
// poi decommentare anche il ramo Consegnato nello switch di Descrivi(StatoOrdine).
//public record Consegnato(DateOnly Il) : StatoOrdine;

public record Ordine(int Id, string Cliente, decimal Totale, MetodoDiPagamento Pagamento, StatoOrdine Stato);

public static class Descrizioni
{
    // Switch esaustivo sulla union: nessun ramo default.
    public static string Descrivi(MetodoDiPagamento pagamento) => pagamento switch
    {
        CartaDiCredito c => $"Carta {c.Circuito} •••• {c.UltimeCifre}",
        Bonifico b => $"Bonifico su {b.Iban}",
        Contrassegno c => $"Contrassegno (+{c.Supplemento:C})",
    };

    // Switch esaustivo sulla gerarchia chiusa: nessun ramo default.
    public static string Descrivi(StatoOrdine stato) => stato switch
    {
        InAttesa => "In attesa di spedizione",
        Spedito s => $"Spedito il {s.Il:dd/MM} con {s.Corriere}",
        Annullato a => $"Annullato: {a.Motivo}",
        //Consegnato c => $"Consegnato il {c.Il:dd/MM}",
    };
}

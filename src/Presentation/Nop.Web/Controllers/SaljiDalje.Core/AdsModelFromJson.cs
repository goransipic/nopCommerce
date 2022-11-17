// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SaljiDalje.Core.SaljiDalje;
public class AudioVideoIFotoOprema
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class AutoMoto
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class ChildCategory
    {
        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }
        [JsonProperty("ChildCategory")]
        public List<ChildCategory> NestedChildCategory { get; set; }
    }

    public class DječjiSvijetSveZaDjecuIBebe
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class GlazbalaGlazbeniInstrumenti
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class HranaIPiće
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class InformatičkaOprema
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class KućniLjubimici
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class Literatura
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class Mobiteli
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class Nautika
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class Nekretnine
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class OdGlaveDoPete
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class Ostalo
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class PosloviINatječajiZaZapošljavanje
    {
        public string CategoryImage { get; set; }
        public string CategoryImage2 { get; set; }
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class PronađenoBlago
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class AdsCategory
    {
        [JsonProperty("Auto Moto")]
        public AutoMoto AutoMoto { get; set; }
        public Nekretnine Nekretnine { get; set; }
        public Nautika Nautika { get; set; }

        [JsonProperty("Hrana i piće")]
        public HranaIPiće HranaIPiće { get; set; }
        public Turizam Turizam { get; set; }
        public Usluge Usluge { get; set; }

        [JsonProperty("Sve za dom")]
        public SveZaDom SveZaDom { get; set; }

        [JsonProperty("Kućni ljubimici")]
        public KućniLjubimici KućniLjubimici { get; set; }
        public InformatičkaOprema InformatičkaOprema { get; set; }
        public Mobiteli Mobiteli { get; set; }

        [JsonProperty("Audio, video i foto oprema")]
        public AudioVideoIFotoOprema AudioVideoIFotoOprema { get; set; }

        [JsonProperty("Glazbala / Glazbeni instrumenti")]
        public GlazbalaGlazbeniInstrumenti GlazbalaGlazbeniInstrumenti { get; set; }
        public Literatura Literatura { get; set; }

        [JsonProperty("Sport i oprema")]
        public SportIOprema SportIOprema { get; set; }

        [JsonProperty("Pronađeno blago")]
        public PronađenoBlago PronađenoBlago { get; set; }

        [JsonProperty("Dječji svijet - sve za djecu i bebe")]
        public DječjiSvijetSveZaDjecuIBebe DječjiSvijetSveZaDjecuIBebe { get; set; }

        [JsonProperty("Strojevi, alati i profesionalna oprema")]
        public StrojeviAlatiIProfesionalnaOprema StrojeviAlatiIProfesionalnaOprema { get; set; }

        [JsonProperty("Od glave do pete")]
        public OdGlaveDoPete OdGlaveDoPete { get; set; }

        [JsonProperty("Poslovi i natječaji za zapošljavanje")]
        public PosloviINatječajiZaZapošljavanje PosloviINatječajiZaZapošljavanje { get; set; }
        public Ostalo Ostalo { get; set; }
    }

    public class SportIOprema
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class StrojeviAlatiIProfesionalnaOprema
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class SveZaDom
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class Turizam
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }

    public class Usluge
    {
        public string CategoryName { get; set; }
        public List<ChildCategory> ChildCategory { get; set; }
    }


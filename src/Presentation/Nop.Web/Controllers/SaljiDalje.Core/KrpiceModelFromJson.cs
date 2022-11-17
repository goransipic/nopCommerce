using System.Collections.Generic;
using Newtonsoft.Json;

namespace SaljiDalje.Core.Krpice;
public class ChildCategory
{
    public string CategoryName { get; set; }
    public string CategoryImage { get; set; }
    [JsonProperty("ChildCategory")]
    public List<ChildCategory> NestedChildCategory { get; set; }
}

public class Djeca
{
    public string CategoryName { get; set; }
    public string CategoryImage { get; set; }
    public List<ChildCategory> ChildCategory { get; set; }
}

public class Dom
{
    public string CategoryName { get; set; }
    public string CategoryImage { get; set; }
    public List<ChildCategory> ChildCategory { get; set; }
}

public class KućniLjubimci
{
    public string CategoryName { get; set; }
    public string CategoryImage { get; set; }
    public List<ChildCategory> ChildCategory { get; set; }
}

public class Muškarci
{
    public string CategoryName { get; set; }
    public string CategoryImage { get; set; }
    public List<ChildCategory> ChildCategory { get; set; }
}

public class KrpiceCategory
{
    [JsonProperty("Žene")]
    public Žene ene { get; set; }
    [JsonProperty("Muškarci")]
    public Muškarci Mukarci { get; set; }
    [JsonProperty("Djeca")]
    public Djeca Djeca { get; set; }
    [JsonProperty("Dom")]
    public Dom Dom { get; set; }

    [JsonProperty("Kućni ljubimci")]
    public KućniLjubimci Kuniljubimci { get; set; }
}

public class Žene
{
    public string CategoryName { get; set; }
    public string CategoryImage { get; set; }
    public List<ChildCategory> ChildCategory { get; set; }
}
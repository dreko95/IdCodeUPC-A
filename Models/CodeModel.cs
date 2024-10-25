using System;

namespace IdCode.Models;

public class CodeModel {
    public CodeModel() { }

    public CodeModel(string prefix, string manufacturerId, string productId, string checkDigit) {
        ProductId = Convert.ToInt32(productId);
        ManufacturerId = Convert.ToInt32(manufacturerId);
        Prefix = Convert.ToByte(prefix);
        CheckDigit = Convert.ToByte(checkDigit);
    }


    public int ProductId { get; set; }
    public int ManufacturerId { get; set; }
    public byte Prefix { get; set; }
    public byte CheckDigit { get; set; }

    public string CodeData => Prefix + ManufacturerId.ToString("00000") + ProductId.ToString("00000");

    public bool IsValid() => Prefix < 10 && ManufacturerId > -1 && ManufacturerId < 100000
                             && ProductId > -1 && ProductId < 100000;
}
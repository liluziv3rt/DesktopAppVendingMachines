using System;
using System.Collections.Generic;

namespace VendingApp.Models;

public partial class Dictionary
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DBEntityGenerate.Linq
{
    public interface IPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChanged;
    }
}

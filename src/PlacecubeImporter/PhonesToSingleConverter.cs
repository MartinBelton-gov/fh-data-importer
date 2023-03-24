using AutoMapper;
using PlacecubeImporter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginBase;

public class PhonesToSingleConverter : ITypeConverter<Phone[], string>
{
    public string Convert(Phone[] source, string destination, ResolutionContext context)
    {
        if (source != null && source.Any())
        {
            return source[0].number;
        }

        return default!;
    }
}

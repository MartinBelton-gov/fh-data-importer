using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginBase;

public class DateTimeTypeConverter : ITypeConverter<string, DateTime?>
{
    public DateTime? Convert(string source, DateTime? destination, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source))
            return null;

        if (DateTime.TryParse(source, out DateTime date))
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        return null;
    }
}

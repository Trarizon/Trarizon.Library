using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Experimental.TaggingNavigation;
public class DirectoryMonitor
{
    private string _basePath;
    private string _regex;
    private List<Item> _items;

    public DirectoryMonitor(string monitorFile)
    {
        _basePath = Path.GetDirectoryName(monitorFile) ?? "";

    }
}

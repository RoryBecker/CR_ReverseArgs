using System.ComponentModel.Composition;
using DevExpress.CodeRush.Common;

namespace CR_ReverseArgs
{
    [Export(typeof(IVsixPluginExtension))]
    public class CR_ReverseArgsExtension : IVsixPluginExtension { }
}
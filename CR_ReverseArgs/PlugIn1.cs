using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.CodeRush.Core;
using DevExpress.CodeRush.PlugInCore;
using DevExpress.CodeRush.StructuralParser;

namespace CR_ReverseArgs
{
    public partial class PlugIn1 : StandardPlugIn
    {
        // DXCore-generated code...
        #region InitializePlugIn
        public override void InitializePlugIn()
        {
            base.InitializePlugIn();
            registerReverseArgs();
        }
        #endregion
        #region FinalizePlugIn
        public override void FinalizePlugIn()
        {
            base.FinalizePlugIn();
        }
        #endregion
        public void registerReverseArgs()
        {
            DevExpress.CodeRush.Core.CodeProvider ReverseArgs = new DevExpress.CodeRush.Core.CodeProvider(components);
            ((System.ComponentModel.ISupportInitialize)(ReverseArgs)).BeginInit();
            ReverseArgs.ProviderName = "ReverseArgs"; // Should be Unique
            ReverseArgs.DisplayName = "Reverse Args";
            ReverseArgs.CheckAvailability += ReverseArgs_CheckAvailability;
            ReverseArgs.Apply += ReverseArgs_Apply;
            ((System.ComponentModel.ISupportInitialize)(ReverseArgs)).EndInit();
        }
        private LanguageElement _startElement;
        private LanguageElement _endElement;
        private LanguageElement PromoteIfReference(LanguageElement element)
        {
            if (element.ElementType == LanguageElementType.MethodReferenceExpression)
                if (element.Parent.ElementType == LanguageElementType.MethodCallExpression)
                    return element.Parent;

            return element;
        }
        private void ReverseArgs_CheckAvailability(Object sender, CheckContentAvailabilityEventArgs ea)
        {
            TextDocument ActiveDoc = ea.TextDocument;
            TextViewSelection Selection = ActiveDoc.ActiveViewSelection;
            SourcePoint StartPoint = Selection.StartSourcePoint;
            SourcePoint EndPoint = Selection.EndSourcePoint;

            _startElement = PromoteIfReference(ActiveDoc.GetNodeAt(StartPoint));
            _endElement = PromoteIfReference(ActiveDoc.GetNodeAt(EndPoint));

            // Exit if selectionText does not contain ','
            if (_startElement == _endElement)
                return;

            if (_startElement.Parent == null)
                return; // not sure how, but cover ourselves anyway :)

            // Exit if Elements do not have same parent
            if (_startElement.Parent != _endElement.Parent)
                return;

            // Exit is elements are not siblings
            if (_startElement.PreviousCodeSibling != _endElement
                && _startElement.NextCodeSibling != _endElement)
                return;

            // don't overlap the name of the method\property.
            if (_startElement.ElementType == LanguageElementType.MethodReferenceExpression)
                return;
            if (_startElement.ElementType == LanguageElementType.ElementReferenceExpression)
                return;

            var Parent = _startElement.Parent.GetDeclaration();
            if (!(Parent is IHasParameters || Parent is IWithParameters))
                return; // Parent takes no parameters (ie method, property etc)

            // Restricted implementation at first.
            if (CodeRush.Selection.Height > 1)
                return;
            ea.Available = true;
        }

        private void ReverseArgs_Apply(Object sender, ApplyContentEventArgs ea)
        {
            using (ea.TextDocument.NewCompoundAction("Reverse Params"))
            {
                // Determine Range
                if (_startElement.PreviousCodeSibling == _endElement)
                {
                    var temp = _endElement;
                    _endElement = _startElement;
                    _startElement = temp;
                }

                var OutputRange = new SourceRange(_startElement.Range.Start, _endElement.Range.End);

                var outString = CodeRush.CodeMod.GenerateCode(_endElement)
                    + ", "
                    + CodeRush.CodeMod.GenerateCode(_startElement);

                ea.TextDocument.SetText(OutputRange, outString);
            }
        }
    }
}
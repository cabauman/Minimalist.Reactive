using Minimalist.Reactive.SourceGenerator.OperatorData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minimalist.Reactive.SourceGenerator.SourceCreator
{
    internal class StringBuilderSourceCreator : ISourceCreator
    {
        public string Create(ClassDatum classDatum)
        {
            var stateFields = new List<string>();
            foreach (var operatorDatum in classDatum.OperatorData)
            {
                var field = operatorDatum.GetField();
                stateFields.Add(field);
            }

            foreach (var operatorDatum in classDatum.OperatorData)
            {
                var source = operatorDatum.Accept(this);
            }

            // Create new method to use as the callback.
            throw new NotImplementedException();
        }

        public string Create(Where operatorData)
        {
            throw new NotImplementedException();
        }

        public string Create(Return operatorData)
        {
            throw new NotImplementedException();
        }
    }
}

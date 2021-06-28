using System.Collections.Generic;
using Demetra.Common;
using Demetra.Model;

namespace Demetra.Aggregates.Attr
{
    public class AttrPayloadBase : Payload
    {
        protected AttrPayloadBase(Model.Attr attr)
        {
            Attr = attr;
        }

        protected AttrPayloadBase(IReadOnlyList<UserError> errors) : base(errors)
        {
        }

        public Model.Attr? Attr { get; }
    }
}
using Demetra.Common;
using Demetra.Model;

namespace Demetra.Aggregates.AttrDescr
{
    public class AddAttrDescrPayload : Payload
    {
        public AddAttrDescrPayload(Model.AttrDescr descr)
        {
            Descr = descr;
        }

        public AddAttrDescrPayload(UserError error) : base(new[] {error})
        {
        }

        public Model.AttrDescr? Descr { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PerceptiveDialogBasedAgent.V3;

namespace PerceptiveDialogBasedAgent.V4
{
    class BodyState2
    {
        private readonly InputPhrase[] _input;

        private readonly Dictionary<InputPhrase, RankedPointing> _pointings = new Dictionary<InputPhrase, RankedPointing>();

        internal InputPhrase LastInputPhrase => _input.LastOrDefault();

        internal static BodyState2 Empty()
        {
            return new BodyState2(new InputPhrase[0], new Dictionary<InputPhrase, RankedPointing>());
        }

        private BodyState2(InputPhrase[] input, Dictionary<InputPhrase, RankedPointing> pointings)
        {
            _input = input;
            _pointings = pointings;
        }

        internal BodyState2 ExpandLastPhrase(string word)
        {
            if (_input.Length == 0)
                return null;

            var lastPhrase = _input.Last().ExpandBy(word);
            var newInput = _input.Concat(new[] { lastPhrase }).ToArray();
            return new BodyState2(newInput, _pointings);
        }

        internal BodyState2 AddPhrase(string word)
        {
            return new BodyState2(_input.Concat(new[] { InputPhrase.FromWord(word) }).ToArray(), _pointings);
        }

        internal BodyState2 Add(IEnumerable<RankedPointing> pointings)
        {
            var newPointings = new Dictionary<InputPhrase, RankedPointing>(_pointings);
            foreach (var pointing in pointings)
            {
                newPointings.Add(pointing.InputPhrase, pointing);
            }

            return new BodyState2(_input, newPointings);
        }
    }
}

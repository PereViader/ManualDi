using UnityEngine.UI;

namespace ManualDi.Async.Unity3d.Samples.Ticking
{
    [ManualDi]
    public class IncreaseValueOnEachUpdate : ITickable
    {
        private readonly Text _text;
        
        public IncreaseValueOnEachUpdate(Text text)
        {
            _text = text;
        }

        public void Tick() //Just like Update on MonoBehaviour but without Inheriting from it
        {
            int.TryParse(_text.text, out var value);
            _text.text = (value + 1).ToString();
        }
    }
}
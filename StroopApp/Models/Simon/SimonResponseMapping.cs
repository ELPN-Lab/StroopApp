using System.Windows.Input;

namespace StroopApp.Models.Simon
{
    public class SimonResponseMapping(SimonAnswer answer, Key key)
    {
        public SimonAnswer Answer { get; set; } = answer;
        public Key Key { get; set; } = key;
    }
}



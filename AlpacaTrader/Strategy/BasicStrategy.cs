using Component;

namespace Strategy;

public class BasicStrategy : IStrategy
{
    public int GetSignal(List<Bar> bars, Bar currentBar)
    {
        throw new NotImplementedException();
    }
}
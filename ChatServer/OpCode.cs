namespace ChatUtils
{
    public enum OpCode
    {
        Join = 0,
        Quit = 1,
        
        SetName = 10,
        GetNames = 11,
        
        Message = 20,
        DirectMessage = 21,
    }
}

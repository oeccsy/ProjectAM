public class Singleton<T> where T : Singleton<T>, new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            Init();
            return instance;
        }
    }

    private static void Init()
    {
        if(instance != null) return;
        
        instance = new T();
    }
}
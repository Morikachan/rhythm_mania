using System.Collections.Generic;

public class MultiGameDataStorage {
    public class PlayerInfo {
        public string Nickname;
        public int Score;
        public int CardID;
        public int InitialPosition; // 1 == P1, 2 == P2
        public bool IsLocal;
    }

    public static Dictionary<int, PlayerInfo> CachedPlayers = new System.Collections.Generic.Dictionary<int, PlayerInfo>();

    public static void Clear()
    {
        CachedPlayers.Clear();
    }
}
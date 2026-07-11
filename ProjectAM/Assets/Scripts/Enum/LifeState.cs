public enum LifeState
{
    Alive,

    // 밤 사건으로 희생됨. 집을 방문한 NPC만 이 사실을 알 수 있다.
    Victim,

    // 광장에서 추방됨. 모두가 아는 사실이다.
    Banished
}

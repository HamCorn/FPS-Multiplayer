using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "FPS/Game Mode")]
public class GameMode : ScriptableObject
{
    [SerializeField] private string _name;      //模式名稱
    [SerializeField] private byte _maxPlayers;  //最大人數
    [SerializeField] private bool _hasTeams;    //是否分隊                
    [SerializeField] private int _teamSize;     //隊伍人數

    public string Name
    {
        get { return _name; }
        private set { _name = value; }
    }
    public byte MaxPlayers
    {
        get { return _maxPlayers; }
        private set { _maxPlayers = value; }
    }
    public bool HasTeams
    {
        get { return _hasTeams; }
        private set { _hasTeams = value; }
    }
    public int TeamSize
    {
        get { return _teamSize; }
        private set { _teamSize = value; }
    }
}


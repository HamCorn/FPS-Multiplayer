using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "FPS/Game Mode")]
public class GameMode : ScriptableObject
{
    [SerializeField] private string _name;      //家Α嘿
    [SerializeField] private byte _maxPlayers;  //程计
    [SerializeField] private bool _hasTeams;    //琌だ钉                
    [SerializeField] private int _teamSize;     //钉ヮ计

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


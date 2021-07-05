using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "FPS/Game Mode")]
public class GameMode : ScriptableObject
{
    [SerializeField] private string _name;      //�Ҧ��W��
    [SerializeField] private byte _maxPlayers;  //�̤j�H��
    [SerializeField] private bool _hasTeams;    //�O�_����                
    [SerializeField] private int _teamSize;     //����H��

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


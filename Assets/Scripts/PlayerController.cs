﻿using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Characteristics")]
    public int   level;
    public int   health;
    public int   levelPoints;
    public int   depthOfVision;
    public int   maxHealth;
    public float luck;

    [Header("Inventory")]
    public int keysAmount    = 0;
    public int bugsAmount    = 0;
    public int trojansAmount = 0;
    public int wormsAmount   = 0;

    int maxLevelPoints;

    // Minimum Characteristics
    int   minLevel          = 1;
    int   minLevelPoints    = 0;
    int   minMaxLevelPoints = 100; 
    int   minDepthOfVision  = 2;
    int   minMaxHealth      = 5;
    float minLuck           = 0.2f;

    // Upgrades
    int   depthOfVisionUpgrade = 1;
    int   maxHealthUpgrade     = 1;
    float luckUpgrade          = 0.1f;

    int amountOfVisiableRooms;

    GameManager gameManager;

    Dictionary<string, int> visionForRoom = new Dictionary<string, int>();
    List<string> keys = new List<string>();
    List<string> clearedRooms = new List<string>();

    IDbCommand command;

    void Start()
    {
        amountOfVisiableRooms = depthOfVision;
        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        command = gameManager.dbConnection.CreateCommand();
        // command.CommandText = "INSERT INTO inventory(trojan, worm, bug) VALUES("+trojansAmount.ToString()+","+wormsAmount.ToString()+","+bugsAmount.ToString()+")";
        // command.ExecuteNonQuery();
    }

    void Update()
    {
        if(health <= 0)
        {
            gameManager.EndGame();
        }
    }


    public void SetIDbCommand(IDbCommand command)
    {
        this.command = command;
    }

    public void SetStartCharacteristics()
    {
        level          = minLevel;
        levelPoints    = minLevelPoints;
        maxLevelPoints = minMaxLevelPoints * level;
        depthOfVision  = minDepthOfVision;
        maxHealth      = minMaxHealth;
        luck           = minLuck;
        health         = maxHealth;
    }

    public void SetCharacteristics(SaveData data)
    {
        this.level         = data.level;
        this.levelPoints   = data.levelPoints;
        this.depthOfVision = data.depthOfVision;
        this.maxHealth     = data.maxHealth;
        this.luck          = data.luck;
    }

    public void AddRoomForVision(string roomName)
    {
        if(!visionForRoom.ContainsKey(roomName))
            visionForRoom.Add(roomName, depthOfVision);
    }

    public void IncrementVisiableRooms(string roomName)
    {
        if(!clearedRooms.Contains(roomName))
            amountOfVisiableRooms++;
    }

    public void IncrementVisionForRoom(string roomName)
    {
        visionForRoom[roomName]++;
    }

    public int GetVisionForRoom(string roomName)
    {
        return visionForRoom[roomName];
    }

    public int GetVisiableRooms()
    {
        return amountOfVisiableRooms;
    }

    public void TakeMonsterAttack()
    {
        health--;
    }

    public string AddKey(string key_id, string room_name)
    {
        command.CommandText = "SELECT key_id FROM " + room_name + " WHERE id = " + key_id;
        IDataReader reader  = command.ExecuteReader();
        string real_key_id  = "";
        if(reader.Read())
        {
            real_key_id = reader.GetInt64(0).ToString();
        }
        reader.Close();
        command.CommandText = "SELECT room_name FROM keys WHERE id = "+real_key_id;
        reader = command.ExecuteReader();
        string locked_room = "";
        if(reader.Read())
        {
            locked_room = reader.GetString(0);
        }
        reader.Close();
        if(locked_room != "")
        {
            keys.Add(locked_room);
            keysAmount = keys.Count;
            command.CommandText = "DELETE FROM keys WHERE id = "+real_key_id;
            command.ExecuteNonQuery();
            command.CommandText = "DELETE FROM "+room_name+" WHERE id = "+key_id;
            command.ExecuteNonQuery();
        }
        return locked_room;
    }

    public bool OpenTheRoom(string room_name)
    {
        Debug.Log("Openning " + room_name);
        if(keys.Contains(room_name))
        {
            keys.Remove(room_name);
            keysAmount = keys.Count;
            return true;
        }
        else
        {
            foreach(string room in keys)
            {
                Debug.Log(room);
            }
        }
        return false;
    }

    public void TakeItem(string item_type)
    {
        if(item_type.Equals("trojan"))
        {
            trojansAmount++;
        }
        if(item_type.Equals("worm"))
        {
            wormsAmount++;
        }
        if(item_type.Equals("bug"))
        {
            bugsAmount++;
        }
    }
}

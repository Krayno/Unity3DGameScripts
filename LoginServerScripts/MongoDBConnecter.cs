using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MongoDBConnecter : MonoBehaviour
{
    public MongoClient Client;
    public IMongoDatabase ProjectX;
    public IMongoDatabase GroupedData;
    public List<string> StoredNames;

    private void Awake()
    {
        Client = new MongoClient("mongodb+srv://Michael:pJdsIkE1qPeNHoU2@cluster0.dc2o6.mongodb.net/ProjectX?retryWrites=true&w=majority");
        ProjectX = Client.GetDatabase("ProjectX");
        GroupedData = Client.GetDatabase("GroupedData");
        StoredNames = GetAllNames();
    }

    public void CreateNewAccount(long steamID)
    {
        BsonDocument NewAccount = new BsonDocument
        {
            { "_id", steamID },
            { "Characters", new BsonDocument
                {
                    { "0", new BsonDocument
                        {
                            { "Name", BsonNull.Value},
                            { "Guild", BsonNull.Value },
                            { "TotalLevel", BsonNull.Value},
                            { "ZoneID", BsonNull.Value},
                            { "Appearance", BsonNull.Value },
                            { "CurrentPosition", new BsonDocument
                                        {
                                            { "PositionX", BsonNull.Value },
                                            { "PositionY", BsonNull.Value },
                                            { "PositionZ", BsonNull.Value },
                                            { "RotationY", BsonNull.Value },
                                        }
                            },
                        }
                    },
                    { "1", new BsonDocument
                        {
                            { "Name", BsonNull.Value},
                            { "Guild", BsonNull.Value },
                            { "TotalLevel", BsonNull.Value},
                            { "ZoneID", BsonNull.Value},
                            { "Appearance", BsonNull.Value },
                            { "CurrentPosition", new BsonDocument
                                        {
                                            { "PositionX", BsonNull.Value },
                                            { "PositionY", BsonNull.Value },
                                            { "PositionZ", BsonNull.Value },
                                            { "RotationY", BsonNull.Value },
                                        }
                            },
                        }
                    },
                    { "2", new BsonDocument
                        {
                            { "Name", BsonNull.Value},
                            { "Guild", BsonNull.Value },
                            { "TotalLevel", BsonNull.Value},
                            { "ZoneID", BsonNull.Value},
                            { "Appearance", BsonNull.Value },
                            { "CurrentPosition", new BsonDocument
                                        {
                                            { "PositionX", BsonNull.Value },
                                            { "PositionY", BsonNull.Value },
                                            { "PositionZ", BsonNull.Value },
                                            { "RotationY", BsonNull.Value },
                                        }
                            },

                        }
                    },
                    { "3", new BsonDocument
                        {
                            { "Name", BsonNull.Value},
                            { "Guild", BsonNull.Value },
                            { "TotalLevel", BsonNull.Value},
                            { "ZoneID", BsonNull.Value},
                            { "Appearance", BsonNull.Value },
                            { "CurrentPosition", new BsonDocument
                                        {
                                            { "PositionX", BsonNull.Value },
                                            { "PositionY", BsonNull.Value },
                                            { "PositionZ", BsonNull.Value },
                                            { "RotationY", BsonNull.Value },
                                        }
                            },
                        }
                    },
                    { "4", new BsonDocument
                        {
                            { "Name", BsonNull.Value},
                            { "Guild", BsonNull.Value },
                            { "TotalLevel", BsonNull.Value},
                            { "ZoneID", BsonNull.Value},
                            { "Appearance", BsonNull.Value },
                            { "CurrentPosition", new BsonDocument
                                        {
                                            { "PositionX", BsonNull.Value },
                                            { "PositionY", BsonNull.Value },
                                            { "PositionZ", BsonNull.Value },
                                            { "RotationY", BsonNull.Value },
                                        }
                            },
                        }
                    },
                    { "5", new BsonDocument
                        {
                            { "Name", BsonNull.Value},
                            { "Guild", BsonNull.Value },
                            { "TotalLevel", BsonNull.Value},
                            { "ZoneID", BsonNull.Value},
                            { "Appearance", BsonNull.Value },
                            { "CurrentPosition", new BsonDocument
                                        {
                                            { "PositionX", BsonNull.Value },
                                            { "PositionY", BsonNull.Value },
                                            { "PositionZ", BsonNull.Value },
                                            { "RotationY", BsonNull.Value },
                                        }
                            },
                        }
                    },
                    { "6", new BsonDocument
                        {
                            { "Name", BsonNull.Value},
                            { "Guild", BsonNull.Value },
                            { "TotalLevel", BsonNull.Value},
                            { "ZoneID", BsonNull.Value},
                            { "Appearance", BsonNull.Value },
                            { "CurrentPosition", new BsonDocument
                                        {
                                            { "PositionX", BsonNull.Value },
                                            { "PositionY", BsonNull.Value },
                                            { "PositionZ", BsonNull.Value },
                                            { "RotationY", BsonNull.Value },
                                        }
                            },
                        }
                    },
                    { "7", new BsonDocument
                        {
                            { "Name", BsonNull.Value},
                            { "Guild", BsonNull.Value },
                            { "TotalLevel", BsonNull.Value},
                            { "ZoneID", BsonNull.Value},
                            { "Appearance", BsonNull.Value },
                            { "CurrentPosition", new BsonDocument
                                        {
                                            { "PositionX", BsonNull.Value },
                                            { "PositionY", BsonNull.Value },
                                            { "PositionZ", BsonNull.Value },
                                            { "RotationY", BsonNull.Value },
                                        }
                            },
                        }
                    },
                }
            },
        };

        ProjectX.GetCollection<BsonDocument>("Accounts").InsertOne(NewAccount);
    }

    private List<string> GetAllNames()
    {
        List<string> Names = new List<string>();

        var NamesFilter = Builders<BsonDocument>.Filter.Exists("Names");
        BsonDocument Document = ProjectX.GetCollection<BsonDocument>("GroupedData").Find(NamesFilter).FirstOrDefault();
        BsonArray BsonNames = Document.GetValue("Names").AsBsonArray;

        foreach (BsonValue Name in BsonNames)
        {
            Names.Add(Name.AsString);
        }

        return Names;
    }
}

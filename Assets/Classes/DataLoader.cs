using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


[Serializable] class TransactionWrapper { public List<TransactionEvent> transactions; }
[Serializable] class CheckInOutWrapper { public List<CheckInOutJsonHelper> events; }
[Serializable] class CheckInOutJsonHelper { public string userId; public string @event; public double timestamp; }

public class DataLoader : MonoBehaviour
{
    [SerializeReference] public List<AbstractVisitor> visitors = new();

    public string ArriveExitFileName;
    public string TransactionsFileName;

    void Start()
    {
        string transactionsFilePath = Path.Combine(Application.streamingAssetsPath, TransactionsFileName + ".json");
        string merchantNamesOutputPath = Path.Combine(Application.streamingAssetsPath, "merchant-names.txt");

        WriteMerchantNamesToFile(transactionsFilePath, merchantNamesOutputPath);
    }

    public void LoadAndOrganizeEvents()
    {
        var visitorDict = new Dictionary<string, AbstractVisitor>();

        // Load transactions
        string transactionsJson = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, TransactionsFileName+ ".json"));
        var transactionWrapper = JsonUtility.FromJson<TransactionWrapper>(transactionsJson);
        foreach (var transaction in transactionWrapper.transactions)
        {
            transaction.timestamp = UnixTimeToString(double.Parse(transaction.timestamp));
            AddEventToVisitor(transaction.userId, transaction, visitorDict);
        }

        // Load check-in/check-out events
        string checkInOutJson = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, ArriveExitFileName+ ".json"));
        var checkWrapper = JsonUtility.FromJson<CheckInOutWrapper>(checkInOutJson);

        foreach (var cio in checkWrapper.events)
        {
            var cioEvent = new CheckInOutEvent
            {
                userId = cio.userId,
                timestamp = UnixTimeToString(cio.timestamp),
                action = cio.@event == "arrive" ? CheckAction.CheckIn : CheckAction.CheckOut
            };

            AddEventToVisitor(cio.userId, cioEvent, visitorDict);
        }

        visitors = visitorDict.Values.ToList();
        visitors.ForEach(visitor => visitor.events.Sort());
    }

    void AddEventToVisitor(string userId, GameEvent gameEvent, Dictionary<string, AbstractVisitor> visitorDict)
    {
        if (!visitorDict.TryGetValue(userId, out var visitor))
        {
            visitor = new AbstractVisitor { userId = userId };
            visitorDict[userId] = visitor;
        }

        visitor.events.Add(gameEvent);
    }

    string UnixTimeToString(double unixMillis)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)unixMillis);
        return dateTimeOffset.ToString("yyyyMMddHHmmss");
    }

    public void WriteMerchantNamesToFile(string transactionsFilePath, string outputTxtFilePath)
    {
        string transactionsJson = File.ReadAllText(transactionsFilePath);
        var transactionData = JsonUtility.FromJson<TransactionWrapper>(transactionsJson);

        var merchantNames = transactionData.transactions
            .Select(t => t.merchantName)
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        File.WriteAllLines(outputTxtFilePath, merchantNames);
        Debug.Log($"Merchant names written to {outputTxtFilePath}");
    }

    // private Dictionary<string, AbstractVisitor> visitorDict = new();
}
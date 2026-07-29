using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public struct OrderQuest
{
    public string id;
    public string displayName;

    public OrderQuest(string id, string displayName)
    {
        this.id = id;
        this.displayName = displayName;
    }
}

public class OrderQuestUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI lineTemplate;
    [SerializeField] private Transform listRoot;
    [SerializeField] private string headerLabel = "Orders";

    private readonly List<OrderQuest> orders = new List<OrderQuest>();
    private readonly Dictionary<string, TextMeshProUGUI> lineById = new Dictionary<string, TextMeshProUGUI>();
    private readonly HashSet<string> completedIds = new HashSet<string>();

    void Awake()
    {
        if (lineTemplate != null)
            lineTemplate.gameObject.SetActive(false);

        if (headerText != null)
            headerText.text = headerLabel;

        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(visible);
        else
            gameObject.SetActive(visible);
    }

    public void SetOrders(IEnumerable<OrderQuest> newOrders)
    {
        Clear();
        if (newOrders == null) return;

        foreach (var order in newOrders)
        {
            if (string.IsNullOrEmpty(order.id)) continue;
            orders.Add(order);
            CreateLine(order);
        }

        SetVisible(orders.Count > 0);
    }

    public void CompleteOrder(string id)
    {
        if (string.IsNullOrEmpty(id) || completedIds.Contains(id)) return;
        if (!lineById.TryGetValue(id, out var line) || line == null) return;

        completedIds.Add(id);
        line.text = $"<s>{line.text}</s>";
        line.alpha = 0.45f;
    }

    public void Clear()
    {
        orders.Clear();
        completedIds.Clear();

        foreach (var pair in lineById)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        lineById.Clear();
    }

    void CreateLine(OrderQuest order)
    {
        if (lineTemplate == null) return;

        Transform parent = listRoot != null ? listRoot : lineTemplate.transform.parent;
        var line = Instantiate(lineTemplate, parent);
        line.gameObject.SetActive(true);
        line.text = order.displayName;
        line.alpha = 1f;
        lineById[order.id] = line;
    }
}

using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    [ContextMenu("รวมร่าง Mesh ตอนนี้เลย!")]
    public void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        int i = 0;
        int combineIndex = 0;
        MeshFilter myMeshFilter = GetComponent<MeshFilter>();
        if (myMeshFilter == null) myMeshFilter = gameObject.AddComponent<MeshFilter>();
        if (GetComponent<MeshRenderer>() == null) gameObject.AddComponent<MeshRenderer>();

        while (i < meshFilters.Length)
        {
            if (meshFilters[i].gameObject == gameObject) { i++; continue; }
            
            combine[combineIndex].mesh = meshFilters[i].sharedMesh;
            combine[combineIndex].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false); // ซ่อนเศษชิ้นงานเก่า

            i++;
            combineIndex++;
        }

        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);
        
        Debug.Log("🎉 รวมชิ้นงานสำเร็จเป็น Mesh เดียวแล้วครับน้า!");
    }
}
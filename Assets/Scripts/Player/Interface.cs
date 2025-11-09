using UnityEngine;
using TMPro;

public class Interface : MonoBehaviour
{
    public PlayerLook playerLook;

    public TMP_Text coordText;
    public TMP_Text normal;
    public TMP_Text globalCoords;
    public TMP_Text localPosText;
    public TMP_Text chunkCoords;

    TerrainChunk chunk;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // TMP_Text properties:
        // .text → visible content (string).
        // .color → text color.
        // .fontSize → font size.
        // .enabled → show/hide.
        // .alpha → transparency.

        if(playerLook != null)
        {
            coordText.text = "GLOBAL COORD: " + playerLook.CurrentHit.point.ToString();
            normal.text = "NORMAL: " + playerLook.CurrentHit.normal.ToString();

            // GameObject target = playerLook.CurrentHit.collider.gameObject;

            // if (target.TryGetComponent<TerrainChunk>(out TerrainChunk chunk))
            // {
            //     Debug.Log(chunk.name);
            //     Debug.Log(chunk.chunkCoord);
            //     chunkCoords.text = "Chunk coords: " + chunk.chunkCoord.ToString();
            //     Vector3Int buildPos = Vector3Int.FloorToInt(playerLook.CurrentHit.point + playerLook.CurrentHit.normal * 0.5f);

            //     globalCoords.text = "GLOBAL BUILD :"+ buildPos.ToString();;
            //     localPosText.text = "LOCAL BUILD :"+ chunk.GlobalToLocal(buildPos).ToString();;
            // } else 
            return;
        }

    }
}
